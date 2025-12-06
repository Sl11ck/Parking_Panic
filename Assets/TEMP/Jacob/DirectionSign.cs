using UnityEngine;

public class DirectionSign : MonoBehaviour
{
    [SerializeField] SignManager signManager;

    private bool wentWrongWay = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!wentWrongWay && col.CompareTag("Player"))
        {
            var car_rb = col.GetComponent<Rigidbody2D>();
            if (car_rb != null)
            {
                wentWrongWay = true;
                signManager.RegisterMistake();
                Debug.Log("Player has went wrong way");
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (wentWrongWay && col.CompareTag("Player"))
        {
            wentWrongWay = false;
        }
    }
}
