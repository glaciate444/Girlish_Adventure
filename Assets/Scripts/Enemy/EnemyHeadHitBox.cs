using UnityEngine;

public class EnemyHeadHitBox : MonoBehaviour {
    private BaseEnemy enemy;

    private void Start(){
        enemy = GetComponentInParent<BaseEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            // プレイヤーが上から来た場合のみ有効
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.y < 0){
                enemy.TakeDamage(enemy.maxHP);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f); // 跳ね返り
            }
        }
    }
}
