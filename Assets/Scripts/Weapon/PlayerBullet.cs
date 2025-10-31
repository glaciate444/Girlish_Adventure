/* =======================================
 * ファイル名 : PlayerBullet.cs
 * 概要 : プレイヤーの弾スクリプト
 * 継承スクリプト : BaseBullet.cs
 * Created Date : 2025/10/15
 * Date : 2025/10/15
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : BaseBullet {
    [SerializeField] private float speed = 10f;
    private Vector2 direction;
    private Rigidbody2D rb;

    private bool initialized = false;

    protected override void Start(){
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 弾が落下しないように
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    public void Setup(Vector2 dir){
        direction = dir.normalized;
        targetTag = "Enemy";
        initialized = true;

        // Rigidbodyがまだ取得されていない場合もあるため安全に呼ぶ
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Unity 6ではここで直接代入してもOK
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate(){
        // 念のため、発射後も速度を維持する
        if (initialized && rb.linearVelocity.sqrMagnitude < 0.01f){
            rb.linearVelocity = direction * speed;
        }
    }

    protected override void HitTarget(Collider2D target){
        var enemy = target.GetComponent<BaseEnemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }
}
