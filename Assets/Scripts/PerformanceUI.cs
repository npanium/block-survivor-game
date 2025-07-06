using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerformanceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI bossHealthText; // Changed from apmText
    [SerializeField] private TextMeshProUGUI dodgeRatioText;
    [SerializeField] private TextMeshProUGUI bulletsText;
    [SerializeField] private GameObject performancePanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("End Screen References")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI endScreenTitle;
    [SerializeField] private TextMeshProUGUI endScreenMessage;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton; // For boss defeated scenario

    [Header("Boss Health Bar")]
    [SerializeField] private Slider bossHealthBar;
    [SerializeField] private Image bossHealthFill;
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;

    [Header("References")]
    [SerializeField] private PerformanceTracker performanceTracker;
    [SerializeField] private GameSessionManager sessionManager;
    [SerializeField] private Enemy boss;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Display Settings")]
    [SerializeField] private bool showOnlyDuringJudgment = true;
    [SerializeField] private bool showBulletStats = true;

    private bool gameEnded = false;

    void Start()
    {
        // Find references if not assigned
        if (performanceTracker == null)
            performanceTracker = FindObjectOfType<PerformanceTracker>();

        if (sessionManager == null)
            sessionManager = FindObjectOfType<GameSessionManager>();

        if (boss == null)
            boss = FindObjectOfType<Enemy>();

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        // Hide panels initially
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        // Subscribe to events
        if (sessionManager != null)
        {
            sessionManager.OnLevelStart.AddListener(OnLevelStart);
            sessionManager.OnJudgmentPeriodEnd.AddListener(ShowLoadingUI);
            sessionManager.OnBufferPeriodStart.AddListener(ShowLoadingUI);
            sessionManager.OnLevelEnd.AddListener(HideAllUI);
        }

        // Subscribe to health events
        if (boss != null)
        {
            boss.OnDeath.AddListener(OnBossDefeated);
            boss.OnHealthChanged.AddListener(OnBossHealthChanged);
        }

        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(OnPlayerDefeated);
        }

        // Setup buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToNextLevel);

        gameEnded = false;
    }

    void Update()
    {
        if (!gameEnded && performanceTracker != null && ShouldShowUI())
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

        // Update boss health (replacing APM)
        if (bossHealthText != null && boss != null)
        {
            int currentHealth = boss.GetCurrentHealth();
            int maxHealth = boss.GetMaxHealth();
            bossHealthText.text = $"BOSS HP\n{currentHealth}/{maxHealth}";
        }

        // Update boss health bar
        UpdateBossHealthBar();

        // Update dodge ratio
        if (dodgeRatioText != null)
        {
            float dodgeRatio = performanceTracker.GetCurrentDodgeRatio();
            dodgeRatioText.text = $"DODGE\n{dodgeRatio:P0}";
        }

        // Update bullet stats (optional)
        if (bulletsText != null && showBulletStats)
        {
            int hit = performanceTracker.BulletsHitPlayer;
            int total = performanceTracker.TotalBulletsFired;
            bulletsText.text = $"HITS\n{hit}/{total}";
        }
    }

    void UpdateBossHealthBar()
    {
        if (bossHealthBar != null && boss != null)
        {
            float healthPercentage = boss.GetHealthPercentage();
            bossHealthBar.value = healthPercentage;

            // Change color based on health
            if (bossHealthFill != null)
            {
                bossHealthFill.color = Color.Lerp(lowHealthColor, healthColor, healthPercentage);
            }
        }
    }

    void OnLevelStart()
    {
        gameEnded = false;
        ShowPerformanceUI();
    }

    void ShowPerformanceUI()
    {
        if (performancePanel != null)
            performancePanel.SetActive(true);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
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

        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    void HideAllUI()
    {
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Don't hide end screen here - let it stay visible
    }

    void UpdateLoadingText()
    {
        if (loadingText != null && performanceTracker != null)
        {
            int nextRound = performanceTracker.CurrentRound + 1;
            loadingText.text = $"NEW ROUND\nLOADING...\n\nROUND {nextRound}";
        }
    }

    // End game events
    void OnBossDefeated()
    {
        if (gameEnded) return;
        gameEnded = true;
        ShowEndScreen(true);
    }

    void OnPlayerDefeated()
    {
        if (gameEnded) return;
        gameEnded = true;
        ShowEndScreen(false);
    }

    void OnBossHealthChanged(int newHealth)
    {
        // Health bar will update in UpdateBossHealthBar()
        // Could add additional effects here if needed
    }

    void ShowEndScreen(bool playerWon)
    {
        // Hide other panels
        if (performancePanel != null)
            performancePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Show end screen
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);

        // Set appropriate text
        if (playerWon)
        {
            if (endScreenTitle != null)
                endScreenTitle.text = "BUG SQUASHED";

            if (endScreenMessage != null)
            {
                int round = performanceTracker != null ? performanceTracker.CurrentRound : 1;
                float dodgeRatio = performanceTracker != null ? performanceTracker.GetCurrentDodgeRatio() : 0f;

                string[] victoryMessages = {
                    "Null pointer eliminated. Reality stabilized.",
                    "Exception handled. The AI retreats.",
                    "Stack overflow prevented. Code secured.",
                    "Bug terminated. System restored.",
                    "Memory leak patched. Crisis averted."
                };

                string message = victoryMessages[Random.Range(0, victoryMessages.Length)];
                endScreenMessage.text = $"{message}\n\nSession #{round} | Evasion: {dodgeRatio:P0}\n\n// Next debugging iteration ready";
            }

            // Show continue button for next level
            if (continueButton != null)
                continueButton.gameObject.SetActive(true);
        }
        else
        {
            if (endScreenTitle != null)
                endScreenTitle.text = "SYSTEM CRASHED";

            if (endScreenMessage != null)
            {
                int round = performanceTracker != null ? performanceTracker.CurrentRound : 1;
                float dodgeRatio = performanceTracker != null ? performanceTracker.GetCurrentDodgeRatio() : 0f;

                string[] defeatMessages = {
                    "The bug evolved faster than your reflexes.",
                    "Null pointer exception: Developer not found.",
                    "Stack overflow. Your debugging session terminated.",
                    "Memory corrupted. The AI adapted too quickly.",
                    "Fatal error: Caffeine levels insufficient."
                };

                string message = defeatMessages[Random.Range(0, defeatMessages.Length)];
                endScreenMessage.text = $"{message}\n\nSession #{round} | Evasion: {dodgeRatio:P0}\n\n// Restart debugging session?";
            }

            // Hide continue button on defeat
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }

        // Always show restart button
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        // Pause or stop game systems
        PauseGame();
    }

    void PauseGame()
    {
        // Stop time scale for dramatic effect (optional)
        // Time.timeScale = 0f;

        // Or disable game components
        if (boss != null)
        {
            var enemyMovement = boss.GetComponent<EnemyMovement>();
            if (enemyMovement != null) enemyMovement.enabled = false;

            var enemyShooting = boss.GetComponent<EnemyShooting>();
            if (enemyShooting != null) enemyShooting.enabled = false;
        }

        if (playerHealth != null && playerHealth.IsAlive())
        {
            var playerMovement = playerHealth.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = false;

            var playerShooting = playerHealth.GetComponent<PlayerShooting>();
            if (playerShooting != null) playerShooting.enabled = false;
        }
    }

    void RestartGame()
    {
        // Reset time scale if paused
        Time.timeScale = 1f;

        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void ContinueToNextLevel()
    {
        // Reset time scale if paused
        Time.timeScale = 1f;

        gameEnded = false;

        // Hide end screen
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        // Trigger next level through session manager
        if (sessionManager != null)
        {
            // You might want to add a method to SessionManager to force next level
            sessionManager.StartNewLevel();
        }
        else
        {
            // Fallback: restart scene
            RestartGame();
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

    public void ForceEndScreen(bool playerWon)
    {
        OnBossDefeated();
    }
}