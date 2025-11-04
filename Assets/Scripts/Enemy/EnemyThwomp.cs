using UnityEngine;

/// <summary>
/// ドッスン（Thwomp）タイプの敵
/// 移動ロジックは Move_Thwomp (MoveBehaviorSO) に委譲
/// </summary>
public class EnemyThwomp : BaseEnemy {
    [Header("衝突ダメージ設定")]
    [SerializeField] private float attackInterval = 0.5f; // 連続ダメージの間隔（秒）
    
    private float lastAttackTime = 0f;

    protected override void Start(){
        base.Start();
        // 初期状態をKinematicに設定（Move_Thwompが制御）
        rb.isKinematic = true;
    }

    /// <summary>
    /// プレイヤーとの衝突時（連続ダメージ対応）
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && Time.time - lastAttackTime >= attackInterval){
                Attack(player);
                lastAttackTime = Time.time;
            }
        }
    }
}
