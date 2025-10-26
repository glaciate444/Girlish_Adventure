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

        bool isGrounded;
        Collider2D col = enemy.GetComponent<Collider2D>();
        if (col != null){
            int mask = (groundLayer.value == 0) ? Physics2D.AllLayers : groundLayer.value;
            isGrounded = col.IsTouchingLayers(mask);

            // 足元（bounds.min）から短距離レイでも確認（保険）
            Vector2 footOrigin = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.02f);
            int rayMask = (groundLayer.value == 0) ? Physics2D.DefaultRaycastLayers : groundLayer.value;
            var hit = Physics2D.Raycast(footOrigin, Vector2.down, 0.05f, rayMask);
            Debug.DrawRay(footOrigin, Vector2.down * 0.05f, hit ? Color.green : Color.red);
            if (!isGrounded) isGrounded = hit.collider != null;
        }else{
            // コライダーが無い場合のフォールバック：Transform基準で下向きレイ
            Vector2 origin = pos + Vector2.down * groundCheckOffset;
            int rayMask = (groundLayer.value == 0) ? Physics2D.DefaultRaycastLayers : groundLayer.value;
            isGrounded = Physics2D.Raycast(origin, Vector2.down, 0.2f, rayMask);
            Debug.DrawRay(origin, Vector2.down * 0.2f, isGrounded ? Color.green : Color.red);
        }

        state.timer += Time.deltaTime;

        // ===== ジャンプ条件 =====
        if (isGrounded && !wasGrounded){
            // 着地した瞬間にタイマーリセット
            state.timer = 0;
        }

        if (isGrounded && state.timer >= jumpInterval){
            // ジャンプ発動
            Vector2 newVelocity = new Vector2(
                dir.x * forwardSpeed,
                jumpForce);

            rb.linearVelocity = newVelocity;
            state.timer = 0;
        }else{
            // 空中中も横方向を維持（AddForce不要）
            rb.linearVelocity = new Vector2(
                dir.x * forwardSpeed,
                rb.linearVelocity.y);
        }
        wasGrounded = isGrounded;
    }
}