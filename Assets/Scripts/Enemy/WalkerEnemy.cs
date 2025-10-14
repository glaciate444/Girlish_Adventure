using UnityEngine;

public class WalkerEnemy : BaseEnemy {
    public float wallCheckDistance = 0.2f;
    public LayerMask groundLayer;
    protected override void FixedUpdate(){
        base.FixedUpdate();
        CheckWall();
    }

    private void CheckWall(){
        Vector2 dir = moveDirection;
        bool hitWall = false;
        
        // 方法1: 調整されたRaycast
        Vector2 origin = transform.position;
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null){
            // 移動方向に応じて、敵の端の位置を計算
            if (moveDirection.x < 0){ // 左に移動中
                origin.x -= capsule.size.x / 2.0f;
            }else{ // 右に移動中
                origin.x += capsule.size.x / 2.0f;
            }
        }
        
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, groundLayer);
        Debug.DrawRay(origin, dir * wallCheckDistance, Color.red, 0.1f);
        
        if (hit.collider != null && hit.distance <= wallCheckDistance){
            hitWall = true;
        }
        
        // 方法2: CapsuleCast（より確実な検出）
        if (!hitWall && capsule != null){
            Vector2 size = capsule.size;
            float angle = 0f;
            float distance = wallCheckDistance;
            
            RaycastHit2D capsuleHit = Physics2D.CapsuleCast(
                transform.position, size, capsule.direction, angle, dir, distance, groundLayer
            );
            
            if (capsuleHit.collider != null){
                hitWall = true;
            }
        }
        
        if (hitWall){
            Debug.Log($"壁を検出: 方向転換実行");
            moveDirection = -moveDirection;
            Flip();
        }
    }

    private void Flip(){
        transform.eulerAngles = new Vector3(0, moveDirection.x > 0 ? 180 : 0, 0);
    }

    public override void Attack(PlayerController player){
        player.Damage(attackPower);
    }
}
