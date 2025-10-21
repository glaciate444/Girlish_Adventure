using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Shoot/Single")]
public class Shoot_Single : ShootBehaviorSO
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private bool aimAtPlayer = true;

    private float timer;

    public override void Shoot(BaseEnemy enemy)
    {
        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer = 0f;

        if (bulletPrefab == null) return;

        var bullet = Instantiate(bulletPrefab, enemy.transform.position, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = aimAtPlayer && enemy.Player != null
            ? (enemy.Player.position - enemy.transform.position).normalized
            : enemy.MoveDirection;

        rb.linearVelocity = dir * bulletSpeed;
    }
}
