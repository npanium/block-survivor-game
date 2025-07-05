using UnityEngine;

public class TimerBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSessionManager sessionManager;

    private Vector3 originalScale;
    private float originalScaleX;

    void Start()
    {
        originalScale = transform.localScale;
        originalScaleX = originalScale.x;

        if (sessionManager == null)
            sessionManager = FindObjectOfType<GameSessionManager>();
    }

    void Update()
    {
        if (sessionManager == null || !sessionManager.IsGameActive()) return;

        float progress = sessionManager.GetRemainingTime() / sessionManager.GetTotalDuration();
        progress = Mathf.Clamp01(progress);

        transform.localScale = new Vector3(
            originalScaleX * progress,
            originalScale.y,
            originalScale.z
        );
    }
}