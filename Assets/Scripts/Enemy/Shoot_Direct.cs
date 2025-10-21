using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Shoot/DirectShot")]
public class Shoot_Direct : ShootBehaviorSO{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float fireInterval = 2f;

    private float timer;

    public override void Shoot(BaseEnemy enemy){
        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer = 0f;

        var bullet = Instantiate(bulletPrefab, enemy.transform.position, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = (enemy.Player.position - enemy.transform.position).normalized;
        rb.linearVelocity = dir * bulletSpeed;
    }
}
