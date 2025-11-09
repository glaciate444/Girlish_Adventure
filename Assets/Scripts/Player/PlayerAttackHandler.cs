using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerAttackHandler
{
    private MonoBehaviour coroutineRunner;
    private GameObject owner;
    private Animator animator;
    private WeaponBase weaponBase;
    private GameObject playerBulletPrefab;
    private Transform firePoint;

    private float attackGroundDuration = 0.3f;
    private float attackAirDuration = 0.6f;
    private int specialCost = 1;
    private float bulletSpawnForwardOffset = 0.2f;
    private float bulletSpawnUpOffsetGrounded = 0.06f;

    private bool isAttacking;
    private bool isAirAttacking;
    private Coroutine attackRoutine;

    public void Configure(
        MonoBehaviour coroutineRunner,
        GameObject owner,
        Animator animator,
        WeaponBase weaponBase,
        GameObject playerBulletPrefab,
        Transform firePoint,
        float attackGroundDuration,
        float attackAirDuration,
        int specialCost,
        float bulletSpawnForwardOffset,
        float bulletSpawnUpOffsetGrounded)
    {
        this.coroutineRunner = coroutineRunner;
        this.owner = owner;
        this.animator = animator;
        this.weaponBase = weaponBase;
        this.playerBulletPrefab = playerBulletPrefab;
        this.firePoint = firePoint;
        this.attackGroundDuration = Mathf.Max(0f, attackGroundDuration);
        this.attackAirDuration = Mathf.Max(0f, attackAirDuration);
        this.specialCost = Mathf.Max(0, specialCost);
        this.bulletSpawnForwardOffset = bulletSpawnForwardOffset;
        this.bulletSpawnUpOffsetGrounded = bulletSpawnUpOffsetGrounded;
    }

    public bool TryStartAttack(bool isGrounded, Vector2 moveInput, bool facingRight)
    {
        if (isAttacking)
            return false;

        isAttacking = true;
        isAirAttacking = !isGrounded;
        animator?.SetBool("IsAttacking", true);

        if (animator != null)
        {
            if (isAirAttacking)
            {
                animator.SetBool("Jump", false);
                animator.Play(facingRight ? "AirAttack_Sword_Right" : "AirAttack_Sword_Left", 0, 0f);
            }
            else
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }
        }

        weaponBase?.StartAttack(moveInput);

        if (coroutineRunner != null)
            attackRoutine = coroutineRunner.StartCoroutine(AttackDurationRoutine());

        return true;
    }

    public void EndAttackFromAnimation()
    {
        if (!isAttacking)
            return;

        StopAttackRoutine();
        CompleteAttack();
    }

    public void ResetForGroundedState()
    {
        if (isAirAttacking && animator != null)
            animator.SetBool("Jump", true);
    }

    public void TrySpecialAttack(PlayerData playerData, bool facingRight, bool isGrounded)
    {
        if (playerData == null)
            return;

        if (playerData.sp < specialCost)
            return;

        playerData.UseSpecial(specialCost);

        if (playerBulletPrefab == null || firePoint == null)
            return;

        float forward = facingRight ? bulletSpawnForwardOffset : -bulletSpawnForwardOffset;
        float up = isGrounded ? bulletSpawnUpOffsetGrounded : 0f;
        Vector3 spawnPos = firePoint.position + new Vector3(forward, up, 0f);
        var bulletObj = Object.Instantiate(playerBulletPrefab, spawnPos, Quaternion.identity);
        var bullet = bulletObj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            bullet.Setup(dir, owner);
        }
    }

    private IEnumerator AttackDurationRoutine()
    {
        float duration = isAirAttacking ? attackAirDuration : attackGroundDuration;
        if (duration > 0f)
            yield return new WaitForSeconds(duration);
        CompleteAttack();
        attackRoutine = null;
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private void CompleteAttack()
    {
        if (isAirAttacking && animator != null)
            animator.SetBool("Jump", true);

        isAttacking = false;
        isAirAttacking = false;
        weaponBase?.EndAttack();
        animator?.SetBool("IsAttacking", false);
    }

    public bool IsAttacking => isAttacking;
    public bool IsAirAttacking => isAirAttacking;
}
