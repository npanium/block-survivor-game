using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint; // Empty GameObject as child of enemy
    [SerializeField] private float fireRate = 1.5f; // Time between shots (slower than player)
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Targeting")]
    [SerializeField] private float shootRange = 12f; // How far enemy can shoot
    [SerializeField] private float trackingStrength = 0.3f; // How much bullets follow player
    [SerializeField] private float trackingDuration = 2f; // How long bullets track

    private float nextFireTime = 0f;
    private Transform player;

    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null && CanShootAtPlayer())
        {
            ShootAtPlayer();
        }
    }

    bool CanShootAtPlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= shootRange && Time.time >= nextFireTime;
    }

    void ShootAtPlayer()
    {
        FireBullet();
        nextFireTime = Time.time + fireRate;
    }

    void FireBullet()
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(player);
            bulletScript.SetSpeed(bulletSpeed);
            bulletScript.SetDamage(bulletDamage);
            bulletScript.SetTrackingStrength(trackingStrength);
        }
    }

    // Optional: Visualize shoot range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= shootRange)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}