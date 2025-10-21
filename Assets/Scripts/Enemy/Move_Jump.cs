using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Jump")]
public class Move_Jump : MoveBehaviorSO {
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float jumpForce = 5f;

    public override void Move(BaseEnemy enemy, MoveState state){
        state.timer += Time.deltaTime;
        if (state.timer >= jumpInterval){
            enemy.Rb.AddForce(
                new Vector2(enemy.MoveDirection.x * enemy.MoveSpeed, jumpForce),
                ForceMode2D.Impulse);
            state.timer = 0;
        }
    }
}
