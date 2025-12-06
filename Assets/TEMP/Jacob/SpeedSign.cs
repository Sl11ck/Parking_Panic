using UnityEngine;

public class SpeedSign : MonoBehaviour
{
    public float speedLimit = 10f;

    private bool exceeded = false;

    [SerializeField] SignManager signManager;
    
    void OnTriggerStay2D(Collider2D col)
    {
        if (!exceeded && col.CompareTag("Player"))
        {
            var car_rb = col.GetComponent<Rigidbody2D>();
            if (car_rb != null && car_rb.linearVelocity.magnitude > speedLimit)
            {
                exceeded = true;
                signManager.RegisterMistake();
                Debug.Log("Player has exceeded the speed limit");
            }
        }
    }
}
