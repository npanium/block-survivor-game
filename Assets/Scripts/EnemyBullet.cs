using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float lifeTime = 8f;
    [SerializeField] private float trackingStrength = 0.3f; // How much it follows player (0-1)
    [SerializeField] private float trackingDuration = 2f; // How long it tracks before going straight
    [SerializeField] private float spinSpeed = 360f; // Degrees per second

    private Transform target;
    private Vector2 fixedDirection;
    private bool isTracking = true;
    private float trackingTimer;
    private PerformanceTracker performanceTracker;

    public void SetTarget(Transform _target)
    {
        target = _target;
        trackingTimer = trackingDuration;

        // Find performance tracker and log bullet fired
        if (performanceTracker == null)
            performanceTracker = FindObjectOfType<PerformanceTracker>();

        if (performanceTracker != null)
            performanceTracker.OnBulletFired();
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetSpeed(float speed)
    {
        bulletSpeed = speed;
    }

    public void SetTrackingStrength(float strength)
    {
        trackingStrength = Mathf.Clamp01(strength);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        // Set initial direction towards player
        if (target)
        {
            fixedDirection = (target.position - transform.position).normalized;
        }
        else
        {
            fixedDirection = Vector2.down; // Default direction
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction;

        if (isTracking && target && trackingTimer > 0)
        {
            // Blend between tracking player and going straight
            Vector2 towardsPlayer = (target.position - transform.position).normalized;
            direction = Vector2.Lerp(fixedDirection, towardsPlayer, trackingStrength).normalized;

            trackingTimer -= Time.fixedDeltaTime;
            if (trackingTimer <= 0)
            {
                isTracking = false;
                fixedDirection = direction; // Lock in final direction
            }
        }
        else
        {
            // Go in fixed direction (player can escape)
            direction = fixedDirection;
        }

        rb.velocity = direction * bulletSpeed;

        // Add spinning motion
        transform.Rotate(0, 0, spinSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(bulletDamage);
            }

            // Log bullet hit for performance tracking
            if (performanceTracker != null)
                performanceTracker.OnBulletHitPlayer();

            Destroy(gameObject);
        }

        // Hit wall/boundary
        // if (other.CompareTag("Wall"))
        // {
        //     Destroy(gameObject);
        // }
    }
}