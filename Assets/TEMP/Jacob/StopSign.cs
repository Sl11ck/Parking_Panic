using NUnit.Framework;
using UnityEngine;

public class StopSign : MonoBehaviour
{
    private bool hasStopped = false;
    public float stopThreshold = 0.1f;

    [SerializeField] SignManager signManager;
    
    void OnTriggerStay2D(Collider2D col)
    {
        if (!hasStopped && col.CompareTag("Player"))
        {
            var car_rb = col.GetComponent<Rigidbody2D>();
            if (car_rb != null && car_rb.linearVelocity.magnitude < stopThreshold)
            {
                hasStopped = true;
                Debug.Log("Player has fully stopped!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!hasStopped)
        {
            signManager.RegisterMistake();
        }
    }
}
