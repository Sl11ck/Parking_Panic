using UnityEngine;

public class PedestrianMovement : MonoBehaviour
{
    public float speed = 10f;
    public float obedience = 1f;

    public float nodeSwitchRadius = 2f;

    public bool isCar = false;
    public float slowRadius = 5f;
    public float fovAngle = 45f;
    public float slowMultiplier = 0.2f;
    public float slowLerpSpeed = 3f;

    private PedestrianNode currentNode;
    private PedestrianNode targetNode;

    private int direction;

    [SerializeField] private Animator animator;
    [SerializeField] private SignManager signManager;

    private GameObject player;

    float currentSpeed;

    void Start()
    {
        currentSpeed = speed;
        player = GameObject.FindGameObjectWithTag("Player");
        signManager = GameObject.FindGameObjectWithTag("SignManager").GetComponent<SignManager>();

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

        targetNode = currentNode.RandomNode(obedience);
    }

    void Update()
    {
        if (targetNode == null) return;

        Vector3 dir = (targetNode.transform.position - transform.position).normalized;

        float targetSpeed = speed;

        // Only CARS slow down based on player's position
        if (isCar)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            float distance = toPlayer.magnitude;

            Vector3 forward = dir;
            float angle = Vector3.Angle(forward, toPlayer);

            bool playerInFront = angle < fovAngle;
            bool playerClose = distance < slowRadius;

            if (playerInFront && playerClose)
            {
                float t = distance / slowRadius;
                float factor = Mathf.Lerp(slowMultiplier, 1f, t);
                targetSpeed = speed * factor;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * slowLerpSpeed);
        }
        else
        {
            // Pedestrians always move at full speed
            currentSpeed = speed;
        }

        // Move object
        transform.position += dir * currentSpeed * Time.deltaTime;

        // ROTATION / ANIMATION
        if (!isCar)
        {
            // Pedestrian → animator directions
            direction = GetDirection(dir);
            animator.SetInteger("Direction", direction);
        }
        else
        {
            // Car → rotate body toward movement direction
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        // Node switching
        if (Vector3.Distance(transform.position, targetNode.transform.position) <= nodeSwitchRadius)
        {
            if (targetNode.isDespawner)
            {
                Destroy(gameObject);
                return;
            }

            currentNode = targetNode;
            targetNode = currentNode.RandomNode(obedience);
        }
    }

    int GetDirection(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.0001f)
            return direction;

        bool horizontal = Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y);

        if (horizontal)
            return moveDir.x > 0 ? 3 : 2;   // Right : Left
        else
            return moveDir.y > 0 ? 0 : 1;   // Up : Down
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < 10; i++)
            {
                signManager.RegisterMistake();
            }
        }
    }
}
