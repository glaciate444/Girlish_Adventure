/* =======================================
 * ファイル名 : Move_Jump.cs
 * 概要 : ジャンプ型移動スクリプト
 * Create Date : 2025/10/16
 * Date : 2025/10/21
 * Version : 0.02
 * 更新内容 : 
 * ======================================= */
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Jump")]
public class Move_Jump : MoveBehaviorSO{
    [Header("ジャンプ設定")]
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float forwardSpeed = 3f;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private bool wasGrounded;

    public override void Move(BaseEnemy enemy, MoveState state){
        Rigidbody2D rb = enemy.Rb;
        Vector2 dir = enemy.MoveDirection;
        Vector2 pos = enemy.transform.position;
        Collider2D col = enemy.GetComponent<Collider2D>();

        // ===== 地面チェック =====
        bool isGrounded = false;
        if (col != null){
            int mask = (groundLayer.value == 0) ? Physics2D.AllLayers : groundLayer.value;
            isGrounded = col.IsTouchingLayers(mask);

            // 足元レイチェック
            Vector2 footOrigin = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.02f);
            int rayMask = (groundLayer.value == 0) ? Physics2D.DefaultRaycastLayers : groundLayer.value;
            var hit = Physics2D.Raycast(footOrigin, Vector2.down, 0.05f, rayMask);
            if (!isGrounded) isGrounded = hit.collider != null;
            Debug.DrawRay(footOrigin, Vector2.down * 0.05f, hit ? Color.green : Color.red);
        }

        // ===== 壁チェック =====
        float wallCheckDistance = 0.1f;
        Vector2 rayOrigin = new Vector2(
            col.bounds.center.x + dir.x * (col.bounds.extents.x - 0.05f), // 側面ギリギリ
            col.bounds.center.y); // 垂直中央

        int wallMask = (groundLayer.value == 0) ? Physics2D.DefaultRaycastLayers : groundLayer.value;
        RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, dir, wallCheckDistance, wallMask);

        Debug.DrawRay(rayOrigin, dir * wallCheckDistance, wallHit ? Color.yellow : Color.cyan);

        // 壁に当たったときのみ反転（余計な反転防止）
        if (wallHit.collider != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f){
            dir.x *= -1;
            enemy.MoveDirection = dir;
        }

        state.timer += Time.deltaTime;

        if (isGrounded && !wasGrounded)
            state.timer = 0;

        if (isGrounded && state.timer >= jumpInterval){
            rb.linearVelocity = new Vector2(dir.x * forwardSpeed, jumpForce);
            state.timer = 0;
        }else{
            rb.linearVelocity = new Vector2(dir.x * forwardSpeed, rb.linearVelocity.y);
        }

        wasGrounded = isGrounded;
    }
}