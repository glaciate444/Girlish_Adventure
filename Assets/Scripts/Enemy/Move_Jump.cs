using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Jump")]
public class Move_Jump : MoveBehaviorSO{
    [Header("�W�����v�ݒ�")]
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

        // ===== �W�����v���� =====
        if (isGrounded && !wasGrounded){
            // ���n�����u�ԂɃ^�C�}�[���Z�b�g
            state.timer = 0;
        }

        if (isGrounded && state.timer >= jumpInterval){
            // �W�����v����
            Vector2 newVelocity = new Vector2(
                dir.x * forwardSpeed,
                jumpForce);

            rb.linearVelocity = newVelocity;
            state.timer = 0;
        }else{
            // �󒆒������������ێ��iAddForce�s�v�j
            rb.linearVelocity = new Vector2(
                dir.x * forwardSpeed,
                rb.linearVelocity.y);
        }
        wasGrounded = isGrounded;
    }
}