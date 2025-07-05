using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private Sprite smoothTerrainSprite;
    [SerializeField] private Sprite stickyTerrainSprite;
    [SerializeField] private Sprite ruggedTerrainSprite;

    [Header("Background Renderer")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Terrain Colors (Optional)")]
    [SerializeField] private Color smoothColor = Color.white;
    [SerializeField] private Color stickyColor = Color.yellow;
    [SerializeField] private Color ruggedColor = Color.gray;

    private string currentTerrainType;

    void Start()
    {
        // Get background renderer if not assigned
        if (backgroundRenderer == null)
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
        }

        // Set default terrain
        SetTerrain("smooth");
    }

    public void SetTerrain(string terrainType)
    {
        currentTerrainType = terrainType.ToLower();

        switch (currentTerrainType)
        {
            case "smooth":
                ApplySmooth();
                break;
            case "sticky":
                ApplySticky();
                break;
            case "rugged":
                ApplyRugged();
                break;
            default:
                Debug.LogWarning($"Unknown terrain type: {terrainType}. Using smooth terrain.");
                ApplySmooth();
                break;
        }

        Debug.Log($"Terrain changed to: {currentTerrainType}");
    }

    void ApplySmooth()
    {
        if (backgroundRenderer != null)
        {
            if (smoothTerrainSprite != null)
                backgroundRenderer.sprite = smoothTerrainSprite;
            backgroundRenderer.color = smoothColor;
        }

        // Optional: Add smooth terrain particle effects, audio, etc.
    }

    void ApplySticky()
    {
        if (backgroundRenderer != null)
        {
            if (stickyTerrainSprite != null)
                backgroundRenderer.sprite = stickyTerrainSprite;
            backgroundRenderer.color = stickyColor;
        }

        // Optional: Add sticky terrain effects
    }

    void ApplyRugged()
    {
        if (backgroundRenderer != null)
        {
            if (ruggedTerrainSprite != null)
                backgroundRenderer.sprite = ruggedTerrainSprite;
            backgroundRenderer.color = ruggedColor;
        }

        // Optional: Add rugged terrain effects
    }

    public string GetCurrentTerrainType()
    {
        return currentTerrainType;
    }

    // Optional: Methods for terrain-specific effects
    public bool IsSmoothTerrain() => currentTerrainType == "smooth";
    public bool IsStickyTerrain() => currentTerrainType == "sticky";
    public bool IsRuggedTerrain() => currentTerrainType == "rugged";
}