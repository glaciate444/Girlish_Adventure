using UnityEngine;

[System.Serializable]
public class Move_Jump : IMoveBehavior{
    private float jumpTimer;
    public float jumpInterval = 2f;
    public float jumpForce = 5f;

    public void Move(BaseEnemy enemy){
        jumpTimer += Time.deltaTime;
        if (jumpTimer >= jumpInterval)
        {
            enemy.Rb.AddForce(new Vector2(enemy.MoveDirection.x * enemy.MoveSpeed, jumpForce), ForceMode2D.Impulse);
            jumpTimer = 0;
        }
    }
}
