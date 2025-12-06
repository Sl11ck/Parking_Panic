using System.Collections;
using UnityEngine;

public class Lv1_SignStop1 : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] private Transform targetCar;
    [SerializeField] private Camera carCamera;

    [Header("Stop Sign")]
    [SerializeField] private CircleCollider2D Sign_AreaCollider;
    [SerializeField] private GameObject Sign_Sprite;


    // State tracking
    private bool isInside = false;
    private Coroutine activeAnimation; // Keep track of running animation to stop it if needed

    void Start()
    {
        // Sprite 1 setup
        Sign_Sprite.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        Sign_Sprite.SetActive(false);
    }

    void Update()
    {
        bool currentlyOverlapping = Sign_AreaCollider.OverlapPoint(targetCar.position);
        // 1. CAR JUST ENTERED
        if (currentlyOverlapping && !isInside)
        {
            isInside = true;
            if (activeAnimation != null) StopCoroutine(activeAnimation);
            activeAnimation = StartCoroutine(ScaleSprite(Sign_Sprite, 0.01f, 2.0f, true));
        }
        // 2. CAR JUST EXITED
        else if (!currentlyOverlapping && isInside)
        {
            isInside = false;
            if (activeAnimation != null) StopCoroutine(activeAnimation);
            activeAnimation = StartCoroutine(ScaleSprite(Sign_Sprite, 2.0f, 0.01f, false));
        }
        Sign_Sprite.transform.rotation = carCamera.transform.rotation;
    }


    private IEnumerator ScaleSprite(GameObject givenSprite, float startScale, float endScale, bool keepActive)
    {
        float duration = 0.25f;
        float currentTime = 0f;

        // Ensure object is enabled before we start animating visibility
        if (startScale < endScale) 
        {
            givenSprite.SetActive(true);
        }

        Vector3 initialScaleVec = new Vector3(startScale, startScale, startScale);
        Vector3 finalScaleVec = new Vector3(endScale, endScale, endScale);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration;

            // Smoothly interpolate between start and end
            givenSprite.transform.localScale = Vector3.Lerp(initialScaleVec, finalScaleVec, t);

            yield return null; // Wait for the next frame
        }

        // Ensure exact final scale
        givenSprite.transform.localScale = finalScaleVec;

        // If we are shrinking (keepActive is false), turn it off now
        if (!keepActive)
        {
            givenSprite.SetActive(false);
        }
    }
}