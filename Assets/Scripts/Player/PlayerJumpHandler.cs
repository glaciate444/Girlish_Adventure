using UnityEngine;

[System.Serializable]
public class PlayerJumpHandler
{
    private PlayerData playerData;
    private Animator animator;

    private float jumpCutMultiplier = 0.4f;
    private float coyoteTime = 0.1f;
    private float coyoteTimer;

    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpCutApplied;

    public void Configure(PlayerData data, Animator animator, float jumpCutMultiplier, float coyoteTime)
    {
        playerData = data;
        this.animator = animator;
        this.jumpCutMultiplier = jumpCutMultiplier;
        this.coyoteTime = Mathf.Max(0f, coyoteTime);
    }

    public void UpdateGroundedState(bool isGrounded, float deltaTime)
    {
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - deltaTime);
        }
    }

    public void OnJumpStarted()
    {
        jumpPressed = true;
        jumpHeld = true;
    }

    public void OnJumpCanceled(ref Vector2 velocity)
    {
        jumpHeld = false;
        ApplyJumpCut(ref velocity);
    }

    public void ProcessJump(ref Vector2 velocity, bool isGrounded, bool isAttacking, Vector2 moveInput, bool forceAirStrafe, float minAirStrafeSpeed)
    {
        if (playerData == null)
            return;

        if (jumpPressed && !isAttacking && (isGrounded || coyoteTimer > 0f))
        {
            velocity.y = playerData.jumpForce;
            jumpPressed = false;
            jumpCutApplied = false;
            coyoteTimer = 0f;

            animator?.SetTrigger("Jump");

            if (forceAirStrafe && Mathf.Abs(moveInput.x) > 0.05f && Mathf.Abs(velocity.x) < minAirStrafeSpeed)
                velocity.x = Mathf.Sign(moveInput.x) * minAirStrafeSpeed;
        }

        if (!jumpHeld)
            ApplyJumpCut(ref velocity);
    }

    private void ApplyJumpCut(ref Vector2 velocity)
    {
        if (jumpCutApplied)
            return;

        if (velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
            jumpCutApplied = true;
        }
    }

    public bool HasJumpRequest => jumpPressed;

    public void ForceJumpCutReset()
    {
        jumpCutApplied = false;
    }
}
