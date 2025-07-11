using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[System.Serializable]
public class StartGameRequest
{
    public string playerId;

    public StartGameRequest(string playerId)
    {
        this.playerId = playerId;
    }
}

[System.Serializable]
public class GameConfig
{
    public TerrainConfig terrain;
    public BossConfig boss;
}

[System.Serializable]
public class TerrainConfig
{
    public string type; // "smooth", "sticky", "rugged"
    public float movementModifier;
}

[System.Serializable]
public class BossConfig
{
    public float speed;
    public int health;
    public int damage;
    public int shield;
}

[System.Serializable]
public class SessionResponse
{
    public bool success;
    public string sessionId;
    public GameConfig config;
    public string message;
}

public class GameSessionManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiEndpoint = "https://places-burning-furnished-na.trycloudflare.com/api/game/start";
    [SerializeField] private string fallbackEndpoint = ""; // Backup URL if main fails
    [SerializeField] private string playerId = "test"; // Player ID to send in request
    [SerializeField] private float totalLevelDuration = 60f; // Total time shown to player (1 minute)
    [SerializeField] private float judgmentPeriod = 30f; // When player performance is judged
    [SerializeField] private float bufferPeriod = 30f; // Buffer for API response and transition
    [SerializeField] private bool allowInsecureConnections = false; // Now using HTTPS
    [SerializeField] private float requestTimeout = 10f; // Timeout in seconds

    [Header("Game References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Enemy boss;
    [SerializeField] private TerrainManager terrainManager;

    [Header("Events")]
    public UnityEvent<GameConfig> OnConfigLoaded;
    public UnityEvent OnLevelStart;
    public UnityEvent OnJudgmentPeriodEnd; // When performance judging ends
    public UnityEvent OnLevelEnd;
    public UnityEvent OnBufferPeriodStart; // When buffer period begins

    private string currentSessionId;
    private GameConfig currentConfig;
    private GameConfig nextConfig; // Store next level config during buffer
    private bool isGameActive = false;
    private bool isInJudgmentPeriod = true;
    private bool isInBufferPeriod = false;
    private bool nextConfigReady = false;
    private float levelTimer;

    void Start()
    {
        StartNewLevel();
    }

    void Update()
    {
        if (isGameActive)
        {
            levelTimer -= Time.deltaTime;

            // Check if judgment period just ended
            if (isInJudgmentPeriod && levelTimer <= bufferPeriod)
            {
                EndJudgmentPeriod();
            }

            // Check if entire level ended
            if (levelTimer <= 0)
            {
                EndLevel();
            }
        }
    }

    public void StartNewLevel()
    {
        StartCoroutine(FetchLevelConfig());
    }

    IEnumerator FetchLevelConfig()
    {
        Debug.Log("Fetching level configuration...");

        // Try main endpoint first
        yield return StartCoroutine(TryFetchFromEndpoint(apiEndpoint));

        // If main failed and we have a fallback, try it
        if (currentConfig == null && !string.IsNullOrEmpty(fallbackEndpoint))
        {
            Debug.Log("Main endpoint failed, trying fallback...");
            yield return StartCoroutine(TryFetchFromEndpoint(fallbackEndpoint));
        }

        // If both failed, use default config
        if (currentConfig == null)
        {
            Debug.LogWarning("All endpoints failed, using default configuration");
            UseDefaultConfig();
        }
        else
        {
            ApplyConfiguration();
            BeginLevel();
        }
    }

    IEnumerator TryFetchFromEndpoint(string endpoint)
    {
        // Create the request body using proper serializable class
        StartGameRequest requestData = new StartGameRequest(this.playerId);
        string requestBody = JsonUtility.ToJson(requestData);

        Debug.Log($"Request body before encoding: {requestBody}");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set timeout
            request.timeout = (int)requestTimeout;

            // Allow insecure connections if needed
            if (allowInsecureConnections)
            {
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
            }

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User-Agent", "Unity-Game-Client");

            Debug.Log($"Sending POST request to: {endpoint}");
            Debug.Log($"Request body: {requestBody}");
            Debug.Log($"Body raw length: {bodyRaw.Length}");

            yield return request.SendWebRequest();

            Debug.Log($"Request completed. Result: {request.result}, Response Code: {request.responseCode}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"API Response: {jsonResponse}");

                    SessionResponse response = JsonUtility.FromJson<SessionResponse>(jsonResponse);

                    if (response.success)
                    {
                        currentSessionId = response.sessionId;
                        currentConfig = response.config;
                        Debug.Log($"Level config loaded! Session ID: {currentSessionId}");
                    }
                    else
                    {
                        Debug.LogError($"API returned error: {response.message}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response: {e.Message}");
                    Debug.LogError($"Raw response: {request.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"API request failed: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"URL: {endpoint}");

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("Connection Error - Check if server is running and URL is correct");
                }
                else if (request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError("Data Processing Error - Server returned invalid data");
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Protocol Error - HTTP {request.responseCode}");
                    if (request.downloadHandler != null)
                    {
                        Debug.LogError($"Error response: {request.downloadHandler.text}");
                    }
                }
            }
        }
    }

    void ApplyConfiguration()
    {
        // Apply terrain settings
        if (terrainManager != null)
        {
            terrainManager.SetTerrain(currentConfig.terrain.type);
        }

        // Apply player movement modifier
        if (playerMovement != null)
        {
            float baseSpeed = playerMovement.moveSpeed;
            playerMovement.moveSpeed = baseSpeed * currentConfig.terrain.movementModifier;
            Debug.Log($"Player speed modified: {baseSpeed} -> {playerMovement.moveSpeed}");
        }

        // Apply boss settings
        if (boss != null)
        {
            ApplyBossConfig(currentConfig.boss);
        }

        // Trigger event for other systems
        OnConfigLoaded?.Invoke(currentConfig);

        Debug.Log($"Configuration applied - Terrain: {currentConfig.terrain.type}, " +
                  $"Boss Health: {currentConfig.boss.health}");
    }

    void ApplyBossConfig(BossConfig bossConfig)
    {
        // You'll need to add these methods to your Enemy script
        boss.SetStats(bossConfig.speed, bossConfig.health, bossConfig.damage, bossConfig.shield);
    }

    void BeginLevel()
    {
        isGameActive = true;
        isInJudgmentPeriod = true;
        isInBufferPeriod = false;
        nextConfigReady = false;
        levelTimer = totalLevelDuration; // Full 60 seconds
        OnLevelStart?.Invoke();

        Debug.Log($"Level started! Total duration: {totalLevelDuration}s (Judgment: {judgmentPeriod}s, Buffer: {bufferPeriod}s)");
    }

    void EndJudgmentPeriod()
    {
        isInJudgmentPeriod = false;
        isInBufferPeriod = true;
        OnJudgmentPeriodEnd?.Invoke();
        OnBufferPeriodStart?.Invoke();

        Debug.Log("Judgment period ended! Starting buffer period and fetching next config...");

        // Start fetching next level config during buffer period
        StartCoroutine(FetchNextLevelConfig());
    }

    void EndLevel()
    {
        isGameActive = false;
        isInJudgmentPeriod = false;
        isInBufferPeriod = false;
        OnLevelEnd?.Invoke();

        Debug.Log("Level ended! Starting next level...");

        // Apply next config if ready, otherwise use current
        if (nextConfigReady && nextConfig != null)
        {
            currentConfig = nextConfig;
            nextConfig = null;
            nextConfigReady = false;

            Debug.Log("Applying pre-loaded next level configuration");
            ApplyConfiguration();
            BeginLevel();
        }
        else
        {
            Debug.Log("Next config not ready, starting new fetch...");
            StartNewLevel();
        }
    }

    IEnumerator FetchNextLevelConfig()
    {
        Debug.Log("Fetching next level configuration during buffer period...");

        // Try main endpoint first
        yield return StartCoroutine(TryFetchNextConfigFromEndpoint(apiEndpoint));

        // If main failed and we have a fallback, try it
        if (nextConfig == null && !string.IsNullOrEmpty(fallbackEndpoint))
        {
            Debug.Log("Main endpoint failed, trying fallback for next config...");
            yield return StartCoroutine(TryFetchNextConfigFromEndpoint(fallbackEndpoint));
        }

        // Mark as ready (even if failed - will use default)
        if (nextConfig == null)
        {
            Debug.LogWarning("Failed to fetch next config, will use default");
            nextConfig = CreateDefaultConfig();
        }

        nextConfigReady = true;
        Debug.Log($"Next level config ready! Remaining buffer time: {levelTimer:F1}s");
    }

    IEnumerator TryFetchNextConfigFromEndpoint(string endpoint)
    {
        // Create the request body using proper serializable class
        StartGameRequest requestData = new StartGameRequest(this.playerId);
        string requestBody = JsonUtility.ToJson(requestData);

        Debug.Log($"Request body before encoding: {requestBody}");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set timeout
            request.timeout = (int)requestTimeout;

            // Allow insecure connections if needed
            if (allowInsecureConnections)
            {
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
            }

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User-Agent", "Unity-Game-Client");

            Debug.Log($"Sending POST request for next config to: {endpoint}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Next config API Response: {jsonResponse}");

                    SessionResponse response = JsonUtility.FromJson<SessionResponse>(jsonResponse);

                    if (response.success)
                    {
                        nextConfig = response.config;
                        Debug.Log($"Next level config loaded successfully!");
                    }
                    else
                    {
                        Debug.LogError($"Next config API returned error: {response.message}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse next config JSON response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Next config API request failed: {request.error}");
            }
        }
    }

    void UseDefaultConfig()
    {
        Debug.Log("Using default configuration");

        currentConfig = CreateDefaultConfig();
        ApplyConfiguration();
        BeginLevel();
    }

    GameConfig CreateDefaultConfig()
    {
        return new GameConfig
        {
            terrain = new TerrainConfig
            {
                type = "smooth",
                movementModifier = 1f
            },
            boss = new BossConfig
            {
                speed = 75f,
                health = 180,
                damage = 18,
                shield = 25
            }
        };
    }

    // Public getters
    public string GetCurrentSessionId() => currentSessionId;
    public GameConfig GetCurrentConfig() => currentConfig;
    public float GetRemainingTime() => levelTimer;
    public float GetJudgmentTimeRemaining() => isInJudgmentPeriod ? levelTimer - bufferPeriod : 0f;
    public bool IsGameActive() => isGameActive;
    public bool IsInJudgmentPeriod() => isInJudgmentPeriod;
    public bool IsInBufferPeriod() => isInBufferPeriod;
    public bool IsNextConfigReady() => nextConfigReady;
    public float GetTotalDuration() => totalLevelDuration;
    public float GetJudgmentDuration() => judgmentPeriod;
    public float GetBufferDuration() => bufferPeriod;
}

// Certificate handler to allow insecure connections (development only)
public class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // Accept all certificates for development
    }
}