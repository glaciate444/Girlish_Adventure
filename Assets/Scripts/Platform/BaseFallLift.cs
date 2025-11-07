using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class BaseFallLift : MonoBehaviour {
    [Header("落下までの待ち時間（秒）")]
    [SerializeField] protected float fallDelay = 1.0f;

    [Header("落下速度")]
    [SerializeField] protected float fallSpeed = 5f;

    [Header("落下後に消えるまでの時間（秒）")]
    [SerializeField] protected float afterFallDelay = 2.0f;

    protected Rigidbody2D rb;
    protected bool isFalling = false;
    protected Vector2 oldPos;
    protected Vector2 myVelocity;
    protected float fallTimer;

    protected virtual void Awake(){
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        oldPos = rb.position;
    }

    private void FixedUpdate(){
        if (isFalling){
            fallTimer += Time.fixedDeltaTime;
            Vector2 newPos = rb.position + Vector2.down * fallSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // 速度記録（プレイヤー追従補助用）
            myVelocity = (newPos - oldPos) / Time.fixedDeltaTime;
            oldPos = newPos;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (!isFalling && collision.collider.CompareTag("Player")){
            StartCoroutine(FallRoutine());
        }
    }

    protected virtual IEnumerator FallRoutine(){
        isFalling = true;
        yield return new WaitForSeconds(fallDelay);
        // 実際の落下は FixedUpdate に任せる
        yield return new WaitForSeconds(afterFallDelay);
        OnAfterFall();
    }

    protected abstract void OnAfterFall();
}
/*
 * Unityで新しい空のGameObjectを作成 → 「FallDownFloor.cs」をアタッチ。
 * Rigidbody2D と BoxCollider2D（または PolygonCollider2D）を追加。
 * Rigidbody2D は Body Type: Kinematic に設定（スクリプトでも固定）。
 * プレイヤーには Tag = "Player" を設定。
 * fallDelay や destroyDelay をインスペクターで調整。
*/