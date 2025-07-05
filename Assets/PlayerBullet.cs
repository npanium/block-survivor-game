using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float lifeTime = 5f; // Auto-destroy after time

    private Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetSpeed(float speed)
    {
        bulletSpeed = speed;
    }

    private void Start()
    {
        // Destroy bullet after lifetime to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            // If no target, continue in current direction
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;

        // Rotate sprite to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage);
            }
            Destroy(gameObject);
        }

        // Hit wall/boundary
        // if (other.CompareTag("Wall"))
        // {
        //     Destroy(gameObject);
        // }
    }
}