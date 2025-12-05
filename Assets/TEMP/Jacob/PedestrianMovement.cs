using UnityEngine;

public class PedestrianMovement : MonoBehaviour
{
    public float speed = 10f;
    public float obedience = 1f; // How much do node weights affect pathfinding, lower obedience means that lower weights are chosen more

    public float nodeSwitchRadius = 2f;
    private PedestrianNode currentNode;
    private PedestrianNode targetNode;

    void Start()
    {
        // Find the nearest node to start at
        PedestrianNode[] nodes = FindObjectsByType<PedestrianNode>(FindObjectsSortMode.None);
        if (nodes.Length == 0) return;

        currentNode = nodes[0];
        float minDist = Vector3.Distance(transform.position, currentNode.transform.position);
        foreach (var node in nodes)
        {
            float dist = Vector3.Distance(transform.position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                currentNode = node;
            }
        }

        // Choose initial target
        targetNode = currentNode.RandomNode(obedience);
    }

    void Update()
    {
        if (targetNode == null) return;

        // Move towards target node
        Vector3 dir = (targetNode.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // Check if reached the target node
        if (Vector3.Distance(transform.position, targetNode.transform.position) <= nodeSwitchRadius)
        {
            currentNode = targetNode;
            targetNode = currentNode.RandomNode(obedience);
        }
    }
}
