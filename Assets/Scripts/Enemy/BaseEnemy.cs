/* =======================================
 * スクリプト名：BaseEnemy.cs
 * 敵スクリプトの基底クラス
 * =======================================
 */
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour{ 
    [Header("共通パラメータ")]
    public float moveSpeed;
    public int maxHP;
    public int attackPower;

    protected int currentHP;
    protected Rigidbody2D rb;
    protected Vector2 moveDirection;

    public EnemyData enemyData;
    protected virtual void Start(){
        rb = GetComponent<Rigidbody2D>();
        moveDirection = Vector2.left;

        if (enemyData != null){
            moveSpeed = enemyData.moveSpeed;
            maxHP = enemyData.maxHP;
            attackPower = enemyData.attackPower;
        }
        currentHP = maxHP;
    }
    protected virtual void FixedUpdate(){
        Move();
    }
    protected virtual void Move(){
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
    }
    public virtual void TakeDamage(int amonut){
        currentHP -= amonut;
        if(currentHP < 0){
            Die(); // HPが0になった場合は敵を消滅
        }
    }
    protected virtual void Die(){
        Destroy(gameObject);
    }
    public abstract void Attack(PlayerController player);
}
