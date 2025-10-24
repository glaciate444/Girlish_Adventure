using UnityEngine;

public class AirAttackStateBehaviour : StateMachineBehaviour
{
    [Header("空中攻撃の設定")]
    public float attackDuration = 0.6f;
    
    private PlayerController playerController;
    private float attackTimer;
    private bool isAttacking;

    // ステート開始時
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerController = animator.GetComponent<PlayerController>();
        attackTimer = attackDuration;
        isAttacking = true;
        
        Debug.Log($"空中攻撃開始: {stateInfo.shortNameHash} - アニメーション強制再生");
    }

    // ステート更新中
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            
            // 攻撃時間が終了したら
            if (attackTimer <= 0f)
            {
                isAttacking = false;
                // Attackトリガーをリセット
                animator.ResetTrigger("Attack");
                Debug.Log("空中攻撃終了 - Attackトリガーをリセット");
            }
        }
    }

    // ステート終了時
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isAttacking = false;
        attackTimer = 0f;
        // ステート終了時にもAttackトリガーをリセット
        animator.ResetTrigger("Attack");
        
        // 空中攻撃終了後はJumpパラメータをtrueに戻す
        if (playerController != null && !playerController.GetComponent<GroundCheck>().IsGrounded)
        {
            animator.SetBool("Jump", true);
            Debug.Log("空中攻撃終了 - Jumpパラメータをtrueに戻す");
        }
        
        Debug.Log($"空中攻撃終了: {stateInfo.shortNameHash} - Attackトリガーをリセット");
    }
}
