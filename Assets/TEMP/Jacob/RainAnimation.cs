using UnityEngine;
using UnityEngine.UI;

public class RainAnimation : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0f, -0.2f);
    private RawImage raw;

    void Start() => raw = GetComponent<RawImage>();

    void Update()
    {
        Rect uv = raw.uvRect;
        uv.position += scrollSpeed * Time.deltaTime;
        raw.uvRect = uv;
    }
}
