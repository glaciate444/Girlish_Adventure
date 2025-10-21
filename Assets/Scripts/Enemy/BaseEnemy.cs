/* =======================================
 * スクリプト名：BaseEnemy.cs
 * 概要 : 敵スクリプトの基底クラス
 * Date : 2025/10/21
 * Version : 0.03
 * 更新内容 : ScriptableObjectに換装
 * ======================================= */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseEnemy : MonoBehaviour {
    [Header("共通パラメータ")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int maxHP = 10;
    [SerializeField] protected int attackPower = 1;
    [SerializeField] protected MoveBehaviorSO moveBehavior;
    [SerializeField] protected ShootBehaviorSO shootBehavior;

    protected int currentHP;
    protected Rigidbody2D rb;
    protected Vector2 moveDirection = Vector2.left;
    protected MoveState moveState;

    public Transform Player { get; set; }
    public Rigidbody2D Rb => rb;
    public Vector2 MoveDirection { get => moveDirection; set => moveDirection = value; }
    public float MoveSpeed => moveSpeed;

    protected virtual void Start(){
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;

        if (moveBehavior != null){
            // MoveState を生成（SO 側でカスタム初期化可能）
            moveState = moveBehavior.CreateState();
            // 必要なら SO 側で追加初期化
            moveBehavior.Initialize(this, moveState);
        }else{
            // 安全策：null じゃない空の状態を作る（必要に応じて）
            moveState = new MoveState();
        }
    }
    protected virtual void Update(){
        shootBehavior?.Shoot(this);
    }
    protected virtual void FixedUpdate(){
        moveBehavior?.Move(this, moveState);
    }

    // ====== ダメージ処理 ======
    public virtual void TakeDamage(int amount){
        currentHP -= amount;
        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die(){
        Destroy(gameObject);
    }

    // ====== 攻撃処理 ======
    public virtual void Attack(PlayerController player){
        if (player != null)
        {
            player.TakeDamage(attackPower);
        }
    }
}
