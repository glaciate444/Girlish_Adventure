/* =======================================
 * ファイル名 : Move_GroundPatrol.cs
 * 概要 : 地上移動型
 * Date : 2025/10/21
 * Version : 0.00
 * ======================================= */
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/GroundPatrol")]
public class Move_GroundPatrol : MoveBehaviorSO {
    [Header("移動・検出設定")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private float rayOffsetY = 0.1f;
    [SerializeField] private float wallRayHeight = 0.2f;

    [Header("安定化パラメータ")]
    [SerializeField] private float reverseCooldown = 0.5f;

    public override void Move(BaseEnemy enemy, MoveState state){
        Rigidbody2D rb = enemy.Rb;
        Vector2 dir = enemy.MoveDirection;

        // 通常移動
        rb.linearVelocity = new Vector2(dir.x * enemy.MoveSpeed, rb.linearVelocity.y);

        // クールダウン処理
        state.timer -= Time.deltaTime;
        if (state.timer > 0f) return;

        bool shouldReverse = false;

        // ===== 崖チェック =====
        Vector2 edgeCheckPos = (Vector2)enemy.transform.position
                             + Vector2.right * dir.x * 0.5f
                             + Vector2.down * rayOffsetY;

        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheckPos, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(edgeCheckPos, Vector2.down * edgeCheckDistance, edgeHit ? Color.green : Color.red);

        if (!edgeHit){
            shouldReverse = true;
        }

        // ===== 壁チェック =====
        Vector2 wallCheckPos = (Vector2)enemy.transform.position
                             + Vector2.up * wallRayHeight
                             + Vector2.right * dir.x * 0.1f; // 少し前にオフセット

        Vector2 wallCheckDirection = Vector2.right * dir.x;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPos, wallCheckDirection, wallCheckDistance, groundLayer);
        Debug.DrawRay(wallCheckPos, wallCheckDirection * wallCheckDistance, wallHit ? Color.yellow : Color.cyan);

        if (wallHit){
            Debug.Log($"壁を検出: {wallHit.collider.name} at distance {wallHit.distance}");
            shouldReverse = true;
        }

        // ===== 方向反転 =====
        if (shouldReverse){
            Debug.Log($"方向反転: {dir} -> {-dir}");
            enemy.MoveDirection = -dir;
            enemy.transform.localScale = new Vector3(Mathf.Sign(enemy.MoveDirection.x), 1f, 1f);
            state.timer = reverseCooldown;
        }
    }
}
