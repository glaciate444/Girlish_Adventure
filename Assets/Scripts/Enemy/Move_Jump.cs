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

        bool isGrounded = Physics2D.Raycast(
            pos + Vector2.down * groundCheckOffset,
            Vector2.down, 0.1f, groundLayer);

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