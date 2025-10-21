/* =======================================
 * ファイル名 : Move_Chase.cs
 * 概要 : 追跡型
 * Date : 2025/10/21
 * ======================================= */
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Chase")]
public class Move_Chase : MoveBehaviorSO {
    [SerializeField] private float chaseRange = 5f;

    public override void Move(BaseEnemy enemy, MoveState state){
        if (enemy.Player == null) return;

        float dist = Vector2.Distance(enemy.transform.position, enemy.Player.position);
        if (dist < chaseRange){
            Vector2 dir = (enemy.Player.position - enemy.transform.position).normalized;
            enemy.Rb.linearVelocity = dir * enemy.MoveSpeed;
        }else{
            enemy.Rb.linearVelocity = Vector2.zero;
        }
    }
}
