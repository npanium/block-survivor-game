using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint; // Empty GameObject as child of player
    [SerializeField] private float fireRate = 0.3f; // Time between shots
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Targeting")]
    [SerializeField] private float targetRange = 15f; // How far to look for enemies
    [SerializeField] private LayerMask enemyLayerMask = -1; // Which layers contain enemies

    private float nextFireTime = 0f;
    private Transform currentTarget;

    void Update()
    {
        FindClosestEnemy();
        ShootAtTarget();
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closestEnemy = null;
        float closestDistance = targetRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        currentTarget = closestEnemy;
    }

    void ShootAtTarget()
    {
        if (currentTarget != null && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(currentTarget);
            bulletScript.SetSpeed(bulletSpeed);
            bulletScript.SetDamage(bulletDamage);
        }
    }

    // Optional: Visualize targeting range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}