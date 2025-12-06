using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleFogZone : MonoBehaviour
{
    [Header("Fog Appearance")]
    [SerializeField] private Color fogColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    [SerializeField][Range(0f, 2f)] private float fogDensity = 1.0f; // Controls fog thickness

    [Header("Fog Size")]
    [SerializeField] private Vector2 fogSize = new Vector2(10f, 10f);

    [Header("Gradient Settings")]
    [SerializeField][Range(0.1f, 5f)] private float falloffPower = 2f; // Lower = thicker fog, Higher = softer edges

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 50;

    private SpriteRenderer spriteRenderer;
    private Texture2D fogTexture;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateFogSprite();
    }

    void OnValidate()
    {
        // Only update if playing, and ensure spriteRenderer is initialized
        if (Application.isPlaying && spriteRenderer != null)
        {
            // Recreate texture when gradient settings change
            if (fogTexture != null)
            {
                Destroy(fogTexture);
            }
            CreateFogSprite();
            UpdateFogAppearance();
        }
    }

    private void CreateFogSprite()
    {
        // Safety check: ensure spriteRenderer exists
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SimpleFogZone: SpriteRenderer component is missing!");
                return;
            }
        }

        // Create gradient texture
        fogTexture = CreateGradientTexture(256, 256);

        Sprite fogSprite = Sprite.Create(
            fogTexture,
            new Rect(0, 0, fogTexture.width, fogTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        spriteRenderer.sprite = fogSprite;
        spriteRenderer.sortingOrder = sortingOrder;

        UpdateFogAppearance();
    }

    private Texture2D CreateGradientTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDist = Mathf.Min(width, height) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distFromCenter = Vector2.Distance(pos, center);

                // Create radial gradient with density control
                float normalizedDist = Mathf.Clamp01(distFromCenter / maxDist);
                float alpha = 1f - normalizedDist;

                // Apply falloff curve (lower value = thicker fog)
                alpha = Mathf.Pow(alpha, falloffPower);

                // Apply density multiplier
                alpha *= fogDensity;
                alpha = Mathf.Clamp01(alpha);

                pixels[y * width + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;

        return texture;
    }

    private void UpdateFogAppearance()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = fogColor;
            transform.localScale = new Vector3(fogSize.x, fogSize.y, 1);
        }
    }

    /// <summary>
    /// Set fog density (0 = invisible, 1 = normal, 2 = very thick)
    /// </summary>
    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp(density, 0f, 2f);
        if (fogTexture != null)
        {
            Destroy(fogTexture);
        }
        CreateFogSprite();
    }

    public void SetFogColor(Color color)
    {
        fogColor = color;
        UpdateFogAppearance();
    }

    public void SetFogSize(Vector2 size)
    {
        fogSize = size;
        UpdateFogAppearance();
    }

    void OnDestroy()
    {
        if (fogTexture != null)
        {
            Destroy(fogTexture);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(fogColor.r, fogColor.g, fogColor.b, 0.5f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(fogSize.x, fogSize.y, 0.1f));
    }
}