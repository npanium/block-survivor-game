using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerformanceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI apmText;
    [SerializeField] private TextMeshProUGUI dodgeRatioText;
    [SerializeField] private TextMeshProUGUI bulletsText;
    [SerializeField] private GameObject performancePanel;
    [SerializeField] private GameObject loadingPanel; // New loading panel
    [SerializeField] private TextMeshProUGUI loadingText; // Loading message text

    [Header("References")]
    [SerializeField] private PerformanceTracker performanceTracker;
    [SerializeField] private GameSessionManager sessionManager;

    [Header("Display Settings")]
    [SerializeField] private bool showOnlyDuringJudgment = true;
    [SerializeField] private bool showBulletStats = true;

    void Start()
    {
        if (performanceTracker == null)
            performanceTracker = FindObjectOfType<PerformanceTracker>();

        if (sessionManager == null)
            sessionManager = FindObjectOfType<GameSessionManager>();

        // Hide panels initially
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Subscribe to events
        if (sessionManager != null)
        {
            sessionManager.OnLevelStart.AddListener(ShowPerformanceUI);
            sessionManager.OnJudgmentPeriodEnd.AddListener(ShowLoadingUI);
            sessionManager.OnBufferPeriodStart.AddListener(ShowLoadingUI);
            sessionManager.OnLevelEnd.AddListener(HideAllUI);
        }
    }

    void Update()
    {
        if (performanceTracker != null && ShouldShowUI())
        {
            UpdatePerformanceDisplay();
        }
    }

    bool ShouldShowUI()
    {
        if (showOnlyDuringJudgment)
        {
            return sessionManager != null && sessionManager.IsInJudgmentPeriod();
        }
        return performanceTracker.IsTracking;
    }

    void UpdatePerformanceDisplay()
    {
        // Update round
        if (roundText != null)
        {
            roundText.text = $"ROUND\n{performanceTracker.CurrentRound}";
        }

        // Update APM
        if (apmText != null)
        {
            int currentAPM = Mathf.RoundToInt(performanceTracker.GetCurrentAPM());
            apmText.text = $"APM\n{currentAPM}";
        }

        // Update dodge ratio
        if (dodgeRatioText != null)
        {
            float dodgeRatio = performanceTracker.GetCurrentDodgeRatio();
            dodgeRatioText.text = $"DODGE\n{dodgeRatio:P0}"; // P0 = percentage with 0 decimals
        }

        // Update bullet stats (optional)
        if (bulletsText != null && showBulletStats)
        {
            int hit = performanceTracker.BulletsHitPlayer;
            int total = performanceTracker.TotalBulletsFired;
            bulletsText.text = $"HITS\n{hit}/{total}";
        }
    }

    void ShowPerformanceUI()
    {
        if (performancePanel != null)
            performancePanel.SetActive(true);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    void ShowLoadingUI()
    {
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            UpdateLoadingText();
        }
    }

    void HideAllUI()
    {
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    void UpdateLoadingText()
    {
        if (loadingText != null && performanceTracker != null)
        {
            int nextRound = performanceTracker.CurrentRound + 1;
            loadingText.text = $"NEW ROUND\nLOADING...\n\nROUND {nextRound}";
        }
    }

    // Manual control methods
    public void TogglePerformanceDisplay()
    {
        if (performancePanel != null)
        {
            bool newState = !performancePanel.activeSelf;
            performancePanel.SetActive(newState);

            if (loadingPanel != null && newState)
                loadingPanel.SetActive(false);
        }
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            ShowPerformanceUI();
        else
            HideAllUI();
    }
}