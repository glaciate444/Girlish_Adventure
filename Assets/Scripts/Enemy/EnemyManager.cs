using UnityEngine;

public class EnemyManager : MonoBehaviour{
    [Header("移動速度")]
    public float moveSpeed;
    [Header("敵の攻撃力")]
    public int attackPower;
    [Header("壁検出距離")]
    public float wallDetectionDistance = 0.3f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isTouchingWall = false;
    private void Start(){
        rb = GetComponent<Rigidbody2D>();
        moveDirection = Vector2.left;
    }

    private void FixedUpdate(){
        Move();
        ChangeMoveDirection();
        LookMoveDirection();
    }

    private void Move(){
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
    }
    // 詳細なデバッグ情報付きの壁検出
    private void ChangeMoveDirection(){
        int layerMask = LayerMask.GetMask("GroundLayer");
        bool hitWall = false;
        
        // 方法1: 調整されたRaycast
        Vector2 rayStart = transform.position;
        if(moveDirection.x < 0) {
            rayStart.x -= GetComponent<CapsuleCollider2D>().size.x / 2.0f;
        } else {
            rayStart.x += GetComponent<CapsuleCollider2D>().size.x / 2.0f;
        }
        RaycastHit2D ray = Physics2D.Raycast(rayStart, moveDirection, 0.2f, layerMask);
        if(ray.transform != null && ray.distance <= 0.2f){
            hitWall = true;
        }
        // 方法2: CapsuleCast（より確実）
        if(!hitWall) {
            CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
            Vector2 size = capsule.size;
            float angle = 0f;
            Vector2 direction = moveDirection;
            float distance = 0.1f;
            
            Debug.Log($"CapsuleCast - 位置: {transform.position}, サイズ: {size}, 方向: {direction}");
            
            RaycastHit2D capsuleHit = Physics2D.CapsuleCast(
                transform.position, size, capsule.direction, angle, direction, distance, layerMask
            );
            
            Debug.Log($"CapsuleCast結果 - hit: {capsuleHit.transform != null}, distance: {capsuleHit.distance}");
            
            if(capsuleHit.transform != null) {
                hitWall = true;
            }
        }
        
        // 方法3: より広範囲のRaycast（デバッグ用）
        if(!hitWall) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, moveDirection, 1.0f, layerMask);
            for(int i = 0; i < hits.Length; i++) {
                if(hits[i].distance <= 0.5f) {
                    hitWall = true;
                    break;
                }
            }
        }
        
        if(hitWall) {
            Debug.Log("🔄 壁を検出 - 方向転換実行");
            Debug.Log($"方向転換前: {moveDirection}");
            moveDirection = -moveDirection;
            Debug.Log($"方向転換後: {moveDirection}");
        } else {
            Debug.Log("❌ 壁を検出できませんでした");
        }
        Debug.Log($"=== 壁検出チェック終了 ===\n");
    }
    private void LookMoveDirection(){
        if(moveDirection.x < 0.0f){
            transform.eulerAngles = Vector3.zero;
        }else if(moveDirection.x > 0.0f){
            transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
    
    }


    public void PlayerDamage(PlayerController player){
        player.Damage(attackPower);
    }
    
    // Colliderの衝突検出（OnTrigger系の代替）
    private void OnCollisionEnter2D(Collision2D collision){
        Debug.Log($"衝突検出: {collision.gameObject.name}, タグ: {collision.gameObject.tag}, レイヤー: {LayerMask.LayerToName(collision.gameObject.layer)}");
        if(collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("GroundLayer")){
            // 壁との衝突を検出
            Vector2 contactNormal = collision.contacts[0].normal;
            Debug.Log($"接触法線: {contactNormal}");
            
            // 左右の壁との衝突をチェック
            if(Mathf.Abs(contactNormal.x) > 0.5f){
                Debug.Log($"🔄 壁との衝突を検出 - 方向転換");
                Debug.Log($"衝突前の移動方向: {moveDirection}");
                moveDirection = -moveDirection;
                Debug.Log($"衝突後の移動方向: {moveDirection}");
                isTouchingWall = true;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision){
        if(collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("GroundLayer")){
            isTouchingWall = false;
        }
    }
}
