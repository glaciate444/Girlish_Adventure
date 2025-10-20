/* =======================================
 * スクリプト名：BaseEnemy.cs
 * 概要 : 敵スクリプトの基底クラス
 * Date : 2025/10/21
 * Version : 0.02
 * ======================================= */
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour{
    [Header("共通パラメータ")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int maxHP = 10;
    [SerializeField] protected int attackPower = 1;
    [SerializeField] protected MovePattern movePattern;

    protected int currentHP;
    protected Rigidbody2D rb;
    protected Vector2 moveDirection = Vector2.left;
    protected IMoveBehavior moveBehavior;

    public Transform Player { get; set; }  // 追尾用参照（自動設定でも可）
    public Rigidbody2D Rb => rb;
    public Vector2 MoveDirection { get => moveDirection; set => moveDirection = value; }
    public float MoveSpeed => moveSpeed;

    protected virtual void Start(){
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
        SetupMoveBehavior();
    }

    protected virtual void FixedUpdate(){
        moveBehavior?.Move(this);
    }

    protected void SetupMoveBehavior(){
        switch (movePattern){
            case MovePattern.GroundPatrol:
                moveBehavior = new Move_GroundPatrol();
                break;
            case MovePattern.FlySin:
                moveBehavior = new Move_FlySin();
                break;
            case MovePattern.Chase:
                moveBehavior = new Move_Chase();
                break;
            case MovePattern.Jump:
                moveBehavior = new Move_Jump();
                break;
        }
    }

    public virtual void TakeDamage(int amount){
        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    protected virtual void Die(){
        Destroy(gameObject);
    }

    public abstract void Attack(PlayerController player);
}
