using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PerformanceData
{
    public int apm;
    public float dodgeRatio;
    public int round;
}

public class PerformanceTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSessionManager sessionManager;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Debug Info")]
    [SerializeField] private bool enableConsoleLogging = false;

    // Performance tracking variables
    private int totalBulletsFired = 0;
    private int bulletsHitPlayer = 0;
    private int keyboardInputs = 0;
    private int currentRound = 1;
    private bool isTracking = false;
    private float trackingStartTime;

    void Start()
    {
        if (sessionManager == null)
            sessionManager = FindObjectOfType<GameSessionManager>();

        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        // Subscribe to session manager events
        if (sessionManager != null)
        {
            sessionManager.OnLevelStart.AddListener(StartTracking);
            sessionManager.OnJudgmentPeriodEnd.AddListener(EndTrackingAndSendData);
        }
    }

    void Update()
    {
        if (isTracking)
        {
            TrackKeyboardInputs();
        }

        // Optional console logging for debugging
        if (enableConsoleLogging && isTracking)
        {
            float elapsedTime = Time.time - trackingStartTime;
            float currentAPM = (keyboardInputs / elapsedTime) * 60f;
            float currentDodgeRatio = totalBulletsFired > 0 ? 1f - ((float)bulletsHitPlayer / totalBulletsFired) : 1f;

            Debug.Log($"Round {currentRound} | Time: {elapsedTime:F1}s | APM: {currentAPM:F0} | Dodge: {currentDodgeRatio:F2} | Bullets: {bulletsHitPlayer}/{totalBulletsFired}");
        }
    }

    void StartTracking()
    {
        ResetCounters();
        isTracking = true;
        trackingStartTime = Time.time;

        Debug.Log($"Started tracking performance for Round {currentRound}");
    }

    void EndTrackingAndSendData()
    {
        if (!isTracking) return;

        isTracking = false;
        float trackingDuration = Time.time - trackingStartTime;

        // Calculate final metrics
        int finalAPM = Mathf.RoundToInt((keyboardInputs / trackingDuration) * 60f);
        float finalDodgeRatio = totalBulletsFired > 0 ? 1f - ((float)bulletsHitPlayer / totalBulletsFired) : 1f;

        Debug.Log($"Round {currentRound} Complete - APM: {finalAPM}, Dodge Ratio: {finalDodgeRatio:F2}, Duration: {trackingDuration:F1}s");

        // Send data to API
        StartCoroutine(SendPerformanceData(finalAPM, finalDodgeRatio, currentRound));

        // Increment round for next level
        currentRound++;
    }

    void TrackKeyboardInputs()
    {
        // Track WASD and Arrow Keys
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            keyboardInputs++;
        }
    }

    void ResetCounters()
    {
        totalBulletsFired = 0;
        bulletsHitPlayer = 0;
        keyboardInputs = 0;
    }

    // Public methods for bullets to call
    public void OnBulletFired()
    {
        if (isTracking)
        {
            totalBulletsFired++;
        }
    }

    public void OnBulletHitPlayer()
    {
        if (isTracking)
        {
            bulletsHitPlayer++;
        }
    }

    IEnumerator SendPerformanceData(int apm, float dodgeRatio, int round)
    {
        string sessionId = sessionManager.GetCurrentSessionId();

        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogError("No session ID available for performance update");
            yield break;
        }

        string endpoint = $"https://places-burning-furnished-na.trycloudflare.com/api/game/{sessionId}/update";

        PerformanceData data = new PerformanceData
        {
            apm = apm,
            dodgeRatio = dodgeRatio,
            round = round
        };

        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User-Agent", "Unity-Game-Client");
            request.timeout = 10;

            Debug.Log($"Sending performance data to: {endpoint}");
            Debug.Log($"Data: {jsonData}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Performance data sent successfully for Round {round}");
                Debug.Log($"Server response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"Failed to send performance data: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                if (request.downloadHandler != null)
                {
                    Debug.LogError($"Error response: {request.downloadHandler.text}");
                }
            }
        }
    }

    // Public getters for debugging
    public int CurrentRound => currentRound;
    public int TotalBulletsFired => totalBulletsFired;
    public int BulletsHitPlayer => bulletsHitPlayer;
    public int KeyboardInputs => keyboardInputs;
    public bool IsTracking => isTracking;
    public float GetCurrentAPM()
    {
        if (!isTracking) return 0f;
        float elapsedTime = Time.time - trackingStartTime;
        return elapsedTime > 0 ? (keyboardInputs / elapsedTime) * 60f : 0f;
    }
    public float GetCurrentDodgeRatio()
    {
        return totalBulletsFired > 0 ? 1f - ((float)bulletsHitPlayer / totalBulletsFired) : 1f;
    }
}