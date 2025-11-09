using UnityEngine;

[System.Serializable]
public class PlayerEnvironmentHandler
{
    private Animator animator;
    private Rigidbody2D rb;
    private Transform playerTransform;

    private float climbSpeed = 4f;
    private float ladderMaxSnapWidth = 1.5f;
    private float ladderReleaseFrames = 30f;

    private bool isClimbing;
    private bool isOnLadder;
    private bool isOnJumpPad;
    private float jumpPadTimer;

    private float horizontalHoldFrames;
    private Collider2D currentLadderCollider;
    private float ladderSnapCenterX;
    private bool lockXOnLadder;

    public void Configure(Animator animator, Rigidbody2D rb, Transform playerTransform, float climbSpeed, float ladderMaxSnapWidth, float ladderReleaseFrames)
    {
        this.animator = animator;
        this.rb = rb;
        this.playerTransform = playerTransform;
        this.climbSpeed = climbSpeed;
        this.ladderMaxSnapWidth = ladderMaxSnapWidth;
        this.ladderReleaseFrames = Mathf.Max(1f, ladderReleaseFrames);
    }

    public void UpdateJumpPadTimer(float deltaTime)
    {
        if (!isOnJumpPad)
            return;

        jumpPadTimer -= deltaTime;
        if (jumpPadTimer <= 0f)
            isOnJumpPad = false;
    }

    public bool UpdateClimb(Vector2 moveInput, ref Vector2 velocity, bool jumpRequested)
    {
        if (!isClimbing)
            return false;

        if (!isOnLadder)
        {
            ExitLadder(ref velocity);
            return false;
        }

        if (Mathf.Abs(moveInput.x) > 0.25f)
            horizontalHoldFrames++;
        else
            horizontalHoldFrames = 0f;

        Debug.Log($"Check800 - horizontalHoldTimeLadder > {horizontalHoldFrames} | {ladderReleaseFrames}");
        if (horizontalHoldFrames >= ladderReleaseFrames || jumpRequested)
        {
            Debug.Log("Check801 - ExitLadder(梯子から出ました)");
            ExitLadder(ref velocity);
            return false;
        }

        if (lockXOnLadder && currentLadderCollider != null && playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            pos.x = ladderSnapCenterX;
            playerTransform.position = pos;
        }

        float climbY = moveInput.y;
        velocity = new Vector2(0f, climbY * climbSpeed);
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            rb.gravityScale = 0f;
        }

        animator?.SetBool("isClimbing", true);
        animator?.SetFloat("ClimbSpeed", Mathf.Abs(climbY));

        return true;
    }

    public void BeginClimb(ref Vector2 velocity)
    {
        if (isClimbing)
            return;

        isClimbing = true;
        horizontalHoldFrames = 0f;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        velocity = Vector2.zero;

        if (currentLadderCollider != null && playerTransform != null)
        {
            Bounds bounds = currentLadderCollider.bounds;
            if (bounds.size.x <= ladderMaxSnapWidth)
            {
                ladderSnapCenterX = bounds.center.x;
                Vector3 pos = playerTransform.position;
                pos.x = ladderSnapCenterX;
                playerTransform.position = pos;
                lockXOnLadder = true;
            }
            else
            {
                ladderSnapCenterX = playerTransform.position.x;
                lockXOnLadder = false;
            }
        }
        else if (playerTransform != null)
        {
            ladderSnapCenterX = playerTransform.position.x;
            lockXOnLadder = false;
        }

        animator?.SetBool("isClimbing", true);
        animator?.SetFloat("ClimbSpeed", 0f);
    }

    public void ExitLadder(ref Vector2 velocity)
    {
        Debug.Log("Check899 - ExitLadder(梯子から出ました)");
        isClimbing = false;
        horizontalHoldFrames = 0f;
        lockXOnLadder = false;
        animator?.SetBool("isClimbing", false);
        animator?.SetFloat("ClimbSpeed", 0f);
        velocity.y = -0.1f;
    }

    public void OnLadderTriggerEnter(Collider2D ladderCollider)
    {
        isOnLadder = true;
        currentLadderCollider = ladderCollider;
    }

    public void OnLadderTriggerExit(ref Vector2 velocity)
    {
        isOnLadder = false;
        currentLadderCollider = null;
        Debug.Log("Ladder exited");
        if (isClimbing)
            ExitLadder(ref velocity);
    }

    public void ActivateJumpPad(ref Vector2 velocity, float bounceHeight, float duration)
    {
        isOnJumpPad = true;
        jumpPadTimer = duration;
        if (duration > 0f)
            velocity.y = bounceHeight / duration * 2f;
    }

    public bool IsClimbing => isClimbing;
    public bool IsOnLadder => isOnLadder;
    public bool IsOnJumpPad => isOnJumpPad;
}
