using UnityEngine;

[System.Serializable]
public class Move_Chase : IMoveBehavior{
    public float chaseRange = 5f;
    public void Move(BaseEnemy enemy){
        if (enemy.Player == null) return;

        float dist = Vector2.Distance(enemy.transform.position, enemy.Player.position);
        if (dist < chaseRange){
            Vector2 dir = (enemy.Player.position - enemy.transform.position).normalized;
            enemy.Rb.linearVelocity = dir * enemy.MoveSpeed;
        }
    }
}
