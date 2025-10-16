using UnityEngine;

public class SwordAttack : MonoBehaviour{
    public int damage = 1;
    private void OnTriggerEnter2D(Collider2D other){
        // Enemyタグを持つ相手にのみ反応
        if(other.CompareTag("Enemy")){
            // BaseEnemyを取得してダメージ処理を呼ぶ
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if(enemy != null){
                enemy.TakeDamage(damage);
                Debug.Log($"Sword Hit! {enemy.name} に {damage} ダメージ！");
            }
        }        
    }

}
