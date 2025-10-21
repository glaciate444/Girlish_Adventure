using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Shoot/3Way")]
public class Shoot_3Way : ShootBehaviorSO{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float spreadAngle = 20f;
    [SerializeField] private bool aimAtPlayer = true;

    private float timer;

    public override void Shoot(BaseEnemy enemy){
        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer = 0f;

        if (bulletPrefab == null) return;

        Vector2 baseDir = aimAtPlayer && enemy.Player != null
            ? (enemy.Player.position - enemy.transform.position).normalized
            : enemy.MoveDirection;

        for (int i = -1; i <= 1; i++){
            float angle = spreadAngle * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            var bullet = Instantiate(bulletPrefab, enemy.transform.position, Quaternion.identity);
            var rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * bulletSpeed;
        }
    }
}
