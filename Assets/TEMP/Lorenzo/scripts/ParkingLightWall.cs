using System.Collections;
using UnityEngine;

public class ParkingLightWall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D parkingSpaceCollider;
    [SerializeField] private Material lightWallMaterial; // Optional: Assign a glowing/emissive material

    [Header("Wall Settings")]
    [SerializeField] private float maxWallHeight = 3f;
    [SerializeField] private Color wallColor = new Color(0.2f, 0.8f, 1f, 0.5f); // Light blue glow with transparency

    [Header("Animation")]
    [SerializeField] private float shrinkDuration = 2f; // Match parking time

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private GameObject[] wallSegments;
    private MeshRenderer[] wallRenderers;
    private MeshFilter[] wallMeshFilters;
    private float currentHeight;
    private bool isAnimating = false;
    private Vector3[] cornerPositions;

    void Start()
    {
        if (parkingSpaceCollider == null)
        {
            Debug.LogError("ParkingLightWall: parkingSpaceCollider not assigned!");
            return;
        }

        currentHeight = maxWallHeight;
        CreateLightWalls();

        if (showDebug)
        {
            Debug.Log($"ParkingLightWall: Created {wallSegments.Length} solid wall segments at height {currentHeight}");
        }
    }

    private void CreateLightWalls()
    {
        // Get the parking space bounds
        Bounds bounds = parkingSpaceCollider.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // Calculate corner positions (2D, on the ground)
        cornerPositions = new Vector3[4];
        cornerPositions[0] = new Vector3(center.x - size.x / 2, center.y + size.y / 2, center.z); // Top left
        cornerPositions[1] = new Vector3(center.x + size.x / 2, center.y + size.y / 2, center.z); // Top right
        cornerPositions[2] = new Vector3(center.x + size.x / 2, center.y - size.y / 2, center.z); // Bottom right
        cornerPositions[3] = new Vector3(center.x - size.x / 2, center.y - size.y / 2, center.z); // Bottom left

        // Create 4 wall segments (one for each side)
        wallSegments = new GameObject[4];
        wallRenderers = new MeshRenderer[4];
        wallMeshFilters = new MeshFilter[4];

        for (int side = 0; side < 4; side++)
        {
            CreateSolidWall(side, $"Wall_Side{side}");
        }

        if (showDebug)
        {
            Debug.Log($"ParkingLightWall: Created {wallSegments.Length} solid walls");
        }
    }

    private void CreateSolidWall(int index, string name)
    {
        GameObject wallObj = new GameObject(name);
        wallObj.transform.parent = transform;
        wallObj.transform.position = Vector3.zero;

        // Add MeshFilter and MeshRenderer
        MeshFilter mf = wallObj.AddComponent<MeshFilter>();
        MeshRenderer mr = wallObj.AddComponent<MeshRenderer>();

        // Create material
        Material mat;
        if (lightWallMaterial != null)
        {
            mat = new Material(lightWallMaterial);
        }
        else
        {
            mat = new Material(Shader.Find("Sprites/Default"));
        }
        mat.color = wallColor;
        mr.material = mat;

        // Set sorting layer
        mr.sortingLayerName = "Default";
        mr.sortingOrder = 100;

        // Create mesh
        Mesh mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mf.mesh = mesh;

        // IMPORTANT: Assign to arrays BEFORE calling UpdateWallMesh
        wallSegments[index] = wallObj;
        wallRenderers[index] = mr;
        wallMeshFilters[index] = mf;

        // Now update the mesh with current wall geometry
        UpdateWallMesh(index);
    }

    private void UpdateWallMesh(int sideIndex)
    {
        Vector3 startCorner = cornerPositions[sideIndex];
        Vector3 endCorner = cornerPositions[(sideIndex + 1) % 4];

        Mesh mesh = wallMeshFilters[sideIndex].mesh;
        mesh.Clear();

        // Create a quad (4 vertices, 2 triangles)
        Vector3[] vertices = new Vector3[4];
        vertices[0] = startCorner; // Bottom left
        vertices[1] = endCorner; // Bottom right
        vertices[2] = startCorner + Vector3.up * currentHeight; // Top left
        vertices[3] = endCorner + Vector3.up * currentHeight; // Top right

        // UVs for texture mapping
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        // Triangles (two triangles make a quad)
        int[] triangles = new int[6];
        triangles[0] = 0; // First triangle
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2; // Second triangle
        triangles[4] = 3;
        triangles[5] = 1;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void UpdateWallHeights()
    {
        for (int i = 0; i < wallSegments.Length; i++)
        {
            UpdateWallMesh(i);
        }
    }

    public void StartShrinking()
    {
        if (!isAnimating)
        {
            if (showDebug)
            {
                Debug.Log("ParkingLightWall: Starting shrink animation");
            }
            StartCoroutine(ShrinkWalls());
        }
    }

    private IEnumerator ShrinkWalls()
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            currentHeight = Mathf.Lerp(maxWallHeight, 0f, t);

            // Update wall heights and meshes
            UpdateWallHeights();

            // Fade out the color
            Color fadedColor = wallColor;
            fadedColor.a = Mathf.Lerp(wallColor.a, 0f, t);

            foreach (MeshRenderer mr in wallRenderers)
            {
                mr.material.color = fadedColor;
            }

            yield return null;
        }

        // Ensure walls are completely hidden
        currentHeight = 0f;
        UpdateWallHeights();

        foreach (GameObject wall in wallSegments)
        {
            wall.SetActive(false);
        }

        if (showDebug)
        {
            Debug.Log("ParkingLightWall: Shrink animation complete");
        }

        isAnimating = false;
    }

    public void ResetWalls()
    {
        if (showDebug)
        {
            Debug.Log("ParkingLightWall: Resetting walls");
        }

        StopAllCoroutines();
        isAnimating = false;
        currentHeight = maxWallHeight;

        // Reactivate and reset all walls
        foreach (GameObject wall in wallSegments)
        {
            wall.SetActive(true);
        }

        UpdateWallHeights();

        foreach (MeshRenderer mr in wallRenderers)
        {
            mr.material.color = wallColor;
        }
    }

    // Visualize in editor
    private void OnDrawGizmos()
    {
        if (parkingSpaceCollider == null) return;

        Bounds bounds = parkingSpaceCollider.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // Draw the wall boundary
        Gizmos.color = wallColor;

        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(center.x - size.x / 2, center.y + size.y / 2, center.z);
        corners[1] = new Vector3(center.x + size.x / 2, center.y + size.y / 2, center.z);
        corners[2] = new Vector3(center.x + size.x / 2, center.y - size.y / 2, center.z);
        corners[3] = new Vector3(center.x - size.x / 2, center.y - size.y / 2, center.z);

        float height = Application.isPlaying ? currentHeight : maxWallHeight;

        for (int i = 0; i < 4; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % 4];

            // Draw base
            Gizmos.DrawLine(start, end);

            // Draw verticals
            Gizmos.DrawLine(start, start + Vector3.up * height);

            // Draw top
            Gizmos.DrawLine(start + Vector3.up * height, end + Vector3.up * height);
        }
    }
}