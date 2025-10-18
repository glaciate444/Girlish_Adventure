using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class BaseFallLift : MonoBehaviour{
    [Header("落下までの待ち時間（秒）")]
    [SerializeField] protected float fallDelay = 1.0f;

    [Header("落下後に消える/処理されるまでの時間（秒）")]
    [SerializeField] protected float afterFallDelay = 2.0f;

    protected Rigidbody2D rb;
    protected bool isFalling = false;
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    protected virtual void Awake(){
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (!isFalling && collision.collider.CompareTag("Player")){
            StartCoroutine(FallRoutine());
        }
    }

    protected virtual IEnumerator FallRoutine(){
        isFalling = true;
        yield return new WaitForSeconds(fallDelay);

        // 落下開始
        rb.isKinematic = false;
        rb.gravityScale = 2f;

        yield return new WaitForSeconds(afterFallDelay);

        // 派生クラスに挙動を委ねる
        OnAfterFall();
    }

    /// <summary>
    /// 落下後の挙動を派生クラスが定義
    /// </summary>
    protected abstract void OnAfterFall();
}
/*
 * Unityで新しい空のGameObjectを作成 → 「FallDownFloor.cs」をアタッチ。
 * Rigidbody2D と BoxCollider2D（または PolygonCollider2D）を追加。
 * Rigidbody2D は Body Type: Kinematic に設定（スクリプトでも固定）。
 * プレイヤーには Tag = "Player" を設定。
 * fallDelay や destroyDelay をインスペクターで調整。
*/