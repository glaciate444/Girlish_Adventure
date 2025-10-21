using UnityEngine;

public class EnemyBullet : BaseBullet{
    protected override void HitTarget(Collider2D target){
        var player = target.GetComponent<PlayerController>();
        if (player != null)
            player.TakeDamage(damage);
    }
}
