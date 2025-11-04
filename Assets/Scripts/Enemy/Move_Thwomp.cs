using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/Thwomp")]
public class Move_Thwomp : MoveBehaviorSO {
    [Header("ドッスン設定")]
    public float triggerRange = 3f;
    public float fallSpeed = 10f;
    public float riseSpeed = 2f;
    public float groundWaitTime = 1f;

    [Header("地面検知")]
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public override void Move(BaseEnemy enemy, MoveState state){
    }

}
