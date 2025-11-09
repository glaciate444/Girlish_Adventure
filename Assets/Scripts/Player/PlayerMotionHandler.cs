using UnityEngine;

[System.Serializable]
public class PlayerMotionHandler
{
    private PlayerData playerData;
    private GroundCheck groundCheck;

    private float airControlFactor = 0.9f;
    private float maxFallSpeed = -25f;
    private float groundAcceleration = 100f;
    private float groundDeceleration = 100f;
    private float airAcceleration = 140f;
    private float airDeceleration = 80f;
    private bool forceAirStrafe = true;
    private float minAirStrafeSpeed = 1.5f;
    private float maxSlopeAngle = 50f;

    public void Configure(
        PlayerData data,
        GroundCheck groundCheck,
        float airControlFactor,
        float maxFallSpeed,
        float groundAcceleration,
        float groundDeceleration,
        float airAcceleration,
        float airDeceleration,
        bool forceAirStrafe,
        float minAirStrafeSpeed,
        float maxSlopeAngle)
    {
        playerData = data;
        this.groundCheck = groundCheck;
        this.airControlFactor = airControlFactor;
        this.maxFallSpeed = maxFallSpeed;
        this.groundAcceleration = groundAcceleration;
        this.groundDeceleration = groundDeceleration;
        this.airAcceleration = airAcceleration;
        this.airDeceleration = airDeceleration;
        this.forceAirStrafe = forceAirStrafe;
        this.minAirStrafeSpeed = minAirStrafeSpeed;
        this.maxSlopeAngle = Mathf.Max(0f, maxSlopeAngle);
    }

    public void UpdateMovement(Vector2 moveInput, bool isGrounded, bool isAttacking, ref Vector2 velocity, float deltaTime)
    {
        if (isAttacking)
            return;

        if (playerData == null)
            return;

        float moveX = moveInput.x;
        float targetSpeed = moveX * playerData.moveSpeed * (isGrounded ? 1f : airControlFactor);

        if (forceAirStrafe && !isGrounded)
        {
            if (Mathf.Abs(moveX) > 0.05f && Mathf.Abs(velocity.x) < 0.01f)
            {
                velocity.x = Mathf.Sign(moveX) * Mathf.Max(minAirStrafeSpeed, Mathf.Abs(velocity.x));
            }
        }

        float accel;
        if (Mathf.Abs(moveX) > 0.05f)
        {
            accel = isGrounded ? groundAcceleration : airAcceleration;
        }
        else
        {
            accel = isGrounded ? groundDeceleration : airDeceleration;
            targetSpeed = 0f;
        }

        float speedDiff = targetSpeed - velocity.x;
        float accelForce = accel * deltaTime;

        if (Mathf.Abs(speedDiff) <= accelForce)
        {
            velocity.x = targetSpeed;
        }
        else
        {
            velocity.x += Mathf.Sign(speedDiff) * accelForce;
        }

        float maxHoriz = isGrounded ? playerData.moveSpeed : playerData.moveSpeed * airControlFactor;
        if (velocity.x > maxHoriz)
        {
            velocity.x = maxHoriz;
        }
        else if (velocity.x < -maxHoriz)
        {
            velocity.x = -maxHoriz;
        }

        if (forceAirStrafe && !isGrounded && Mathf.Abs(moveX) > 0.05f)
        {
            if (Mathf.Abs(velocity.x) < minAirStrafeSpeed)
                velocity.x = Mathf.Sign(moveX) * minAirStrafeSpeed;
        }

        if (Mathf.Abs(moveX) <= 0.05f && Mathf.Abs(velocity.x) < 0.05f)
            velocity.x = 0f;
    }

    public void ApplyGravity(bool isGrounded, ref Vector2 velocity, float deltaTime)
    {
        if (playerData == null)
            return;

        if (!isGrounded)
        {
            velocity.y += playerData.gravity * deltaTime;
            if (velocity.y < maxFallSpeed)
                velocity.y = maxFallSpeed;
        }
        else
        {
            if (velocity.y < 0f)
                velocity.y = 0f;
        }
    }

    public Vector2 GetFinalVelocity(Vector2 velocity, bool isGrounded)
    {
        if (groundCheck != null && isGrounded)
        {
            if (velocity.y <= 0.0001f)
            {
                Vector2 groundNormal = groundCheck.GroundNormal;
                if (groundNormal.y > 0.0001f)
                {
                    float slopeAngle = Vector2.Angle(Vector2.up, groundNormal);
                    if (slopeAngle > 0.01f && slopeAngle <= maxSlopeAngle)
                    {
                        float horizontalSpeed = velocity.x;
                        Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x).normalized;
                        velocity = slopeDirection * horizontalSpeed;
                    }
                    else if (velocity.y > 0f)
                    {
                        velocity.y = 0f;
                    }
                }
            }

            Vector2 groundVelocity = groundCheck.GetGroundVelocity();
            velocity += groundVelocity;
        }

        return velocity;
    }
}
