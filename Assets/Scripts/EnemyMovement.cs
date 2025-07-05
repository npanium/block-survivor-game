using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float changeDirectionInterval = 2f;
    [SerializeField] private float movementRange = 3f; // How far from spawn point

    [Header("Boundary Settings")]
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private float boundaryPadding = 1f;

    private Vector3 spawnPosition;
    private Vector3 targetPosition;
    private Vector3 movementDirection;
    private float nextDirectionChangeTime;
    private Camera mainCamera;
    private Vector2 cameraBounds;

    void Start()
    {
        spawnPosition = transform.position;
        mainCamera = Camera.main;

        if (useCameraBounds)
            CalculateCameraBounds();

        ChooseNewTarget();
    }

    void Update()
    {
        MoveTowardsTarget();

        // Change direction periodically
        if (Time.time >= nextDirectionChangeTime)
        {
            ChooseNewTarget();
        }

        // Ensure enemy stays within bounds
        EnforceBoundaries();
    }

    void CalculateCameraBounds()
    {
        if (mainCamera != null)
        {
            float camHeight = mainCamera.orthographicSize - boundaryPadding;
            float camWidth = (camHeight * mainCamera.aspect) - boundaryPadding;
            cameraBounds = new Vector2(camWidth, camHeight);
        }
    }

    void ChooseNewTarget()
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(0.5f, movementRange);

        if (useCameraBounds)
        {
            // Choose target within camera bounds
            targetPosition = GetRandomPositionInCameraBounds();
        }
        else
        {
            // Choose target within movement range of spawn point
            targetPosition = spawnPosition + (randomDirection * randomDistance);
        }

        movementDirection = (targetPosition - transform.position).normalized;
        nextDirectionChangeTime = Time.time + changeDirectionInterval + Random.Range(-0.5f, 0.5f);
    }

    Vector3 GetRandomPositionInCameraBounds()
    {
        float randomX = Random.Range(-cameraBounds.x, cameraBounds.x);
        float randomY = Random.Range(-cameraBounds.y, cameraBounds.y);
        return new Vector3(randomX, randomY, transform.position.z);
    }

    void MoveTowardsTarget()
    {
        // Move towards target
        transform.position += movementDirection * moveSpeed * Time.deltaTime;

        // Check if reached target (within a small threshold)
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            ChooseNewTarget();
        }
    }

    void EnforceBoundaries()
    {
        Vector3 currentPos = transform.position;
        bool needsNewTarget = false;

        if (useCameraBounds && mainCamera != null)
        {
            // Clamp to camera bounds
            float clampedX = Mathf.Clamp(currentPos.x, -cameraBounds.x, cameraBounds.x);
            float clampedY = Mathf.Clamp(currentPos.y, -cameraBounds.y, cameraBounds.y);

            if (currentPos.x != clampedX || currentPos.y != clampedY)
            {
                transform.position = new Vector3(clampedX, clampedY, currentPos.z);
                needsNewTarget = true;
            }
        }
        else
        {
            // Keep within movement range of spawn point
            float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);
            if (distanceFromSpawn > movementRange)
            {
                Vector3 directionToSpawn = (spawnPosition - transform.position).normalized;
                transform.position = spawnPosition - (directionToSpawn * movementRange);
                needsNewTarget = true;
            }
        }

        // Choose new target if we hit a boundary
        if (needsNewTarget)
        {
            ChooseNewTarget();
        }
    }

    // Public methods for external control
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void SetMovementRange(float range)
    {
        movementRange = range;
    }

    public void PauseMovement()
    {
        enabled = false;
    }

    public void ResumeMovement()
    {
        enabled = true;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw movement range around spawn point
        if (!useCameraBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
            Gizmos.DrawWireSphere(center, movementRange);
        }

        // Draw target position
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);

            // Draw line to target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
        }

        // Draw camera bounds
        if (useCameraBounds && mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(cameraBounds.x * 2, cameraBounds.y * 2, 0));
        }
    }
}