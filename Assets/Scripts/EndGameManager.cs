using UnityEngine;
using UnityEngine.Events;

public class EndGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy boss;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PerformanceTracker performanceTracker;
    [SerializeField] private GameSessionManager sessionManager;

    [Header("Events")]
    public UnityEvent<bool> OnGameEnd; // bool: true = player won, false = player lost

    private bool gameEnded = false;

    void Start()
    {
        // Find references if not assigned
        if (boss == null)
            boss = FindObjectOfType<Enemy>();

        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (performanceTracker == null)
            performanceTracker = FindObjectOfType<PerformanceTracker>();

        if (sessionManager == null)
            sessionManager = FindObjectOfType<GameSessionManager>();

        // Subscribe to death events
        if (boss != null)
            boss.OnDeath.AddListener(OnBossDefeated);

        if (playerHealth != null)
            playerHealth.OnDeath.AddListener(OnPlayerDefeated);

        // Reset game state
        gameEnded = false;
    }

    void OnBossDefeated()
    {
        if (gameEnded) return;

        Debug.Log("Boss defeated! Player wins!");
        EndGame(true);
    }

    void OnPlayerDefeated()
    {
        if (gameEnded) return;

        Debug.Log("Player defeated! Game over!");
        EndGame(false);
    }

    void EndGame(bool playerWon)
    {
        gameEnded = true;

        // Stop performance tracking
        if (performanceTracker != null && performanceTracker.IsTracking)
        {
            // Force end tracking and send final data
            performanceTracker.SendImmediatePerformanceData(playerWon);
        }

        // Disable game systems
        DisableGameSystems();

        // Trigger end game event
        OnGameEnd?.Invoke(playerWon);

        Debug.Log($"Game ended. Player won: {playerWon}");
    }

    void DisableGameSystems()
    {
        // Disable boss
        if (boss != null)
        {
            var enemyMovement = boss.GetComponent<EnemyMovement>();
            if (enemyMovement != null) enemyMovement.enabled = false;

            var enemyShooting = boss.GetComponent<EnemyShooting>();
            if (enemyShooting != null) enemyShooting.enabled = false;
        }

        // Disable player if still alive
        if (playerHealth != null && playerHealth.IsAlive())
        {
            var playerMovement = playerHealth.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = false;

            var playerShooting = playerHealth.GetComponent<PlayerShooting>();
            if (playerShooting != null) playerShooting.enabled = false;
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void StartNextLevel()
    {
        Time.timeScale = 1f;
        gameEnded = false;

        if (sessionManager != null)
        {
            sessionManager.StartNewLevel();
        }
        else
        {
            RestartLevel();
        }
    }

    // Public getters
    public bool HasGameEnded() => gameEnded;
}