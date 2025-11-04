using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Thwomp")]
public class Move_Thwomp : MoveBehaviorSO
{
    [Header("ドッスン設定")]
    public float triggerRange = 3f;
    public float fallSpeed = 10f;
    public float riseSpeed = 2f;
    public float groundWaitTime = 1f;

    [Header("地面検知")]
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    [Tooltip("地面検知の位置（nullの場合は敵の中心から下方向0.5f）")]
    public Transform groundCheckTransform; // EnemyThwompのgroundCheckを参照可能に

    public override void Move(BaseEnemy enemy, MoveState state)
    {
        // 専用ステートにキャスト
        ThwompState s = state as ThwompState;
        if (s == null) return; // 念のためnullガード

        if (s.coroutineRunning) return;
        if (enemy.Player == null) return;

        float distX = Mathf.Abs(enemy.Player.position.x - enemy.transform.position.x);
        if (!s.isFalling && !s.isRising && distX <= triggerRange)
        {
            enemy.StartCoroutine(FallRoutine(enemy, s));
        }
    }

    private IEnumerator FallRoutine(BaseEnemy enemy, ThwompState state)
    {
        state.coroutineRunning = true;
        state.isFalling = true;

        Rigidbody2D rb = enemy.Rb;
        Vector2 startPos = enemy.transform.position;
        float startX = startPos.x; // X軸を固定するため初期位置を保存

        // 落下中はX軸を固定して横移動を防止
        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

        rb.isKinematic = false;
        rb.linearVelocity = Vector2.down * fallSpeed;

        yield return new WaitUntil(() => IsGrounded(enemy));

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        // X軸の固定を維持（上昇時も横移動を防止）
        
        // プレイヤーが下にいる場合、地面に到達しない可能性があるため
        // 一定時間待機してから上昇を開始
        yield return new WaitForSeconds(groundWaitTime);

        state.isFalling = false;
        state.isRising = true;

        // 上昇（X軸は固定されたまま）
        while (enemy.transform.position.y < startPos.y)
        {
            Vector2 currentPos = enemy.transform.position;
            // X軸は固定し、Y軸のみ移動
            enemy.transform.position = new Vector2(
                startX, // X軸を固定
                Mathf.MoveTowards(currentPos.y, startPos.y, riseSpeed * Time.deltaTime)
            );
            yield return null;
        }

        enemy.transform.position = startPos;
        
        // 制約を元に戻す
        rb.constraints = originalConstraints;
        state.isRising = false;
        state.coroutineRunning = false;
    }

    private bool IsGrounded(BaseEnemy enemy)
    {
        // groundCheckTransformが設定されている場合はそれを使用、なければ敵の中心から下方向
        Vector3 checkPos = groundCheckTransform != null 
            ? groundCheckTransform.position 
            : enemy.transform.position + Vector3.down * 0.5f;
            
        return Physics2D.OverlapCircle(
            checkPos,
            groundCheckRadius,
            groundLayer
        );
    }

    public override MoveState CreateState() => new ThwompState();

    // ==== ここが重要：専用ステート ====
    private class ThwompState : MoveState
    {
        public bool coroutineRunning;
        public bool isFalling;
        public bool isRising;
    }
}
/* =============================================
 * 🧱  設定手順（Unityエディタ）
 * Projectウィンドウで [Create > Enemy > MoveBehavior > Thwomp] を作成。
 * 生成した Move_Thwomp の Inspector で
 * triggerRange（落下反応距離）
 * fallSpeed, riseSpeed, groundWaitTime
 * groundLayer を設定。
 * 対象の敵プレハブのMoveBehavior にこのSOをセット。
 * isInvincible にチェックを入れれば倒せない敵に。
 * =============================================
*/