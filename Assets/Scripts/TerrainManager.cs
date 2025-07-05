using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Terrain Configuration")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite[] smoothSprites;
    [SerializeField] private Sprite[] stickySprites;
    [SerializeField] private Sprite[] ruggedSprites;

    private string currentTerrain;

    void Awake()
    {
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetTerrain(string terrainType)
    {
        var normalizedType = terrainType?.ToLower().Trim();

        var sprites = normalizedType switch
        {
            "smooth" => smoothSprites,
            "sticky" => stickySprites,
            "rugged" => ruggedSprites,
            _ => HandleInvalidTerrain(terrainType)
        };

        if (sprites?.Length > 0)
        {
            currentTerrain = normalizedType;
            ApplyRandomSprite(sprites);
        }
    }

    private Sprite[] HandleInvalidTerrain(string terrainType)
    {
        Debug.LogWarning($"Invalid terrain type '{terrainType}'. Using smooth terrain.");
        return smoothSprites;
    }

    private void ApplyRandomSprite(Sprite[] sprites)
    {
        if (backgroundRenderer != null && sprites?.Length > 0)
        {
            var sprite = sprites[Random.Range(0, sprites.Length)];
            backgroundRenderer.sprite = sprite;
        }
    }

    // Public accessors
    public string CurrentTerrain => currentTerrain;
    public bool IsSmooth => currentTerrain == "smooth";
    public bool IsSticky => currentTerrain == "sticky";
    public bool IsRugged => currentTerrain == "rugged";
}