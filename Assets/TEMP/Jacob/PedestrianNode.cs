using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NodeWeightPair
{
    public PedestrianNode target;
    public float weight;
}

public class PedestrianNode : MonoBehaviour
{
    public bool isSpawner = false;
    public bool isDespawner = false;
    public List<NodeWeightPair> nodeWeightsList = new();

    private Dictionary<PedestrianNode, float> nodeWeightsDict;

    void Awake()
    {
        nodeWeightsDict = new Dictionary<PedestrianNode, float>();

        foreach (var pair in nodeWeightsList)
        {
            if (!nodeWeightsDict.ContainsKey(pair.target))
                nodeWeightsDict.Add(pair.target, pair.weight);
        }
    }

    public PedestrianNode RandomNode(float obedience = 1f)
    {
        if (nodeWeightsDict == null || nodeWeightsDict.Count == 0)
            return this;

        float totalWeight = 0f;
        Dictionary<PedestrianNode, float> modifiedWeights = new Dictionary<PedestrianNode, float>();

        foreach (var kvp in nodeWeightsDict)
        {
            PedestrianNode nextNode = FindNodeAtPosition(kvp.Key.transform.position);
            if (nextNode == null) continue;

            float modifiedWeight = Mathf.Pow(kvp.Value, obedience);
            modifiedWeights[nextNode] = modifiedWeight;
            totalWeight += modifiedWeight;
        }

        float randomValue = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var kvp in modifiedWeights)
        {
            cumulative += kvp.Value;
            if (randomValue <= cumulative)
                return kvp.Key;
        }

        // fallback
        foreach (var kvp in modifiedWeights)
            return kvp.Key;

        return this;
    }

    private PedestrianNode FindNodeAtPosition(Vector2 pos)
    {
        PedestrianNode[] nodes = FindObjectsByType<PedestrianNode>(FindObjectsSortMode.None);
        foreach (var node in nodes)
        {
            if ((Vector2)node.transform.position == pos)
                return node;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        // Draw this node
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.15f);

        if (nodeWeightsDict == null)
            return;

        foreach (var pair in nodeWeightsDict)
        {
            Vector2 targetPos = pair.Key.transform.position;
            float weight = pair.Value;

            // Line color based on weight (heavier = red)
            Gizmos.color = Color.Lerp(Color.green, Color.red, weight);

            // Draw line
            Gizmos.DrawLine(transform.position, targetPos);

            // Draw weight label in Scene view
    #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                (transform.position + (Vector3)targetPos) / 2f,
                $"W:{weight:F1}"
            );
    #endif
        }
    }
}
