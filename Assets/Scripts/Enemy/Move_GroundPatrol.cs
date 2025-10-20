using UnityEngine;

[System.Serializable]
public class Move_GroundPatrol : IMoveBehavior{
    public LayerMask groundLayer;
    public void Move(BaseEnemy enemy){
        Rigidbody2D rb = enemy.Rb;
        Vector2 dir = enemy.MoveDirection;

        rb.linearVelocity = new Vector2(dir.x * enemy.MoveSpeed, rb.linearVelocity.y);

        // ŠRƒ`ƒFƒbƒN
        Vector2 checkPos = (Vector2)enemy.transform.position + Vector2.right * dir.x * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 0.5f, groundLayer);
        if (!hit){
            enemy.MoveDirection = -dir; // ”½“]
        }
    }
}
