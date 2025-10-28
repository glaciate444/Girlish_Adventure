﻿/* =======================================
 * ファイル名 : PlayerController.cs
 * 概要 : プレイヤースクリプト
 * Created Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * 更新内容 : AddForce除外
 * ======================================= */
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour{
    [Header("基本ステータス")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float gravity = -50f; // カスタム重力（GravityScaleは0）
    [Header("UIステータス")]
    public int hp;
    public int sp;
    public int maxHP = 10;
    public int maxSP = 6;
    [Header("物理演算ステータス")]
    [SerializeField] private float airControlFactor = 0.9f; // 空中でもほぼ地上と同等の最大速度
    [SerializeField] private float jumpCutMultiplier = 0.4f; // ジャンプカット倍率
    [SerializeField] private float maxFallSpeed = -25f; // 最大落下速度
    [SerializeField] private float groundFriction = 0.8f; // 地上での摩擦（無入力時のみ）
    [SerializeField] private float airFriction = 0.98f; // 空中での空気抵抗（無入力時に少しだけ）
    [SerializeField] private float groundAcceleration = 100f; // 地上の加速度
    [SerializeField] private float groundDeceleration = 100f; // 地上の減速度
    [SerializeField] private float airAcceleration = 140f; // 空中の加速度（強め）
    [SerializeField] private float airDeceleration = 80f; // 空中の減速度
    [SerializeField] private bool forceAirStrafe = true; // 空中時の強制横移動を有効化
    [SerializeField] private float minAirStrafeSpeed = 1.5f; // 空中での最低横速度
    [Header("攻撃関連")]
    [SerializeField] private SwordFlipHandler swordHandler;
    [SerializeField] private WeaponBase weaponBase;
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int specialCost = 1;
    [SerializeField] private float attackGroundDuration = 0.3f;
    [SerializeField] private float attackAirDuration = 0.6f;
    [Header("Ground Check")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private float dropThroughTime = 0.3f;
    [Header("アニメーション / 無敵点滅")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HitEffectSpawner hitEffectSpawner;
    [SerializeField] private float damageTime = 3f;
    [SerializeField] private float flashTime = 0.2f;

    // 内部変数
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 velocity;
    private bool facingRight = true;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private bool isDropping;
    private bool isAttacking;
    private bool isAirAttacking;
    private bool isDead;
    private bool isInvincible;
    private PlayerInput playerInput;
    public delegate void OnDamageDelegate();
    public event OnDamageDelegate OnDamage;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity; // AddForceではなく直接制御
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable(){
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerInput != null){
            // Inspector の UnityEvents が未設定でも動くよう、コードで配線
            playerInput.onActionTriggered += HandleActionTriggered;

            // 常に Player マップを有効化
            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != "Player")
                playerInput.SwitchCurrentActionMap("Player");

            if (!playerInput.actions.enabled)
                playerInput.actions.Enable();
        }
    }

    private void OnDisable(){
        if (playerInput != null)
            playerInput.onActionTriggered -= HandleActionTriggered;
    }

    private void Update(){
        if (isDead) return;

		// 入力・見た目更新のみ（物理はFixedUpdate）
		LookMoveDirection();
        UpdateAnimation();
    }

    private void FixedUpdate(){
        if (isDead) return;

        UpdateGroundState();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        ApplyVelocity();
    }

    private void UpdateGroundState(){
        bool groundedNow = groundCheck != null ? groundCheck.IsGrounded :
            Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        // ドロップ中は強制的に非接地扱いにする（重力を有効にして下に抜ける）
        isGrounded = isDropping ? false : groundedNow;

        animator?.SetBool("IsGrounded", isGrounded);
    }

    private void HandleMovement(){
        if (isAttacking) return;
        float moveX = moveInput.x;
        float targetSpeed = moveX * moveSpeed * (isGrounded ? 1f : airControlFactor);

        // 強制横移動救済: 空中で入力があるのに横が0に張り付く場合
        if (forceAirStrafe && !isGrounded){
            if (Mathf.Abs(moveX) > 0.05f && Mathf.Abs(velocity.x) < 0.01f){
                // 最低限の横速度を与える
                velocity.x = Mathf.Sign(moveX) * Mathf.Max(minAirStrafeSpeed, Mathf.Abs(velocity.x));
            }
        }

        // 物理ベースの加速度計算（左右対称）
        float accel = 0f;
        if (Mathf.Abs(moveX) > 0.05f){
            accel = isGrounded ? groundAcceleration : airAcceleration;
        }else{
            accel = isGrounded ? groundDeceleration : airDeceleration;
            targetSpeed = 0f;
        }

        // Mathf.MoveTowardsの代わりに物理ベースの計算を使用
        float speedDiff = targetSpeed - velocity.x;
        float accelForce = accel * Time.fixedDeltaTime;
        
        if (Mathf.Abs(speedDiff) <= accelForce){
            // 目標速度に到達または超える場合
            velocity.x = targetSpeed;
        }else{
            // 加速度を適用
            velocity.x += Mathf.Sign(speedDiff) * accelForce;
        }

        // 左右対称の最大速度でクランプ
        float maxHoriz = isGrounded ? moveSpeed : moveSpeed * airControlFactor;
        if (velocity.x > maxHoriz){
            velocity.x = maxHoriz;
        }else if (velocity.x < -maxHoriz){
            velocity.x = -maxHoriz;
        }

        // さらなる保険: 空中時に水平速度がゼロへ貼り付くことを抑止
        if (forceAirStrafe && !isGrounded && Mathf.Abs(moveX) > 0.05f){
            if (Mathf.Abs(velocity.x) < minAirStrafeSpeed)
                velocity.x = Mathf.Sign(moveX) * minAirStrafeSpeed;
        }

        // 無入力時に微小速度を丸める
        if (Mathf.Abs(moveX) <= 0.05f && Mathf.Abs(velocity.x) < 0.05f)
            velocity.x = 0f;
    }

    private void HandleJump(){
        if (jumpPressed && isGrounded && !isAttacking){
            velocity.y = jumpForce;
            jumpPressed = false;
            jumpCutApplied = false;
            animator?.SetTrigger("Jump");

            // ジャンプ直後の慣性殺し対策: 横入力があるなら初速を保証
            if (forceAirStrafe && Mathf.Abs(moveInput.x) > 0.05f){
                if (Mathf.Abs(velocity.x) < minAirStrafeSpeed)
                    velocity.x = Mathf.Sign(moveInput.x) * minAirStrafeSpeed;
            }
        }

        // ジャンプカット処理（より厳しい減速）
        if (!jumpHeld && !jumpCutApplied && velocity.y > 0f){
            velocity.y *= jumpCutMultiplier;
            jumpCutApplied = true;
        }
    }

    private void ApplyGravity(){
        if (!isGrounded){
            // カスタム重力適用
            velocity.y += gravity * Time.fixedDeltaTime;
            // 最大落下速度制限
            if (velocity.y < maxFallSpeed)
                velocity.y = maxFallSpeed;
        }else{
            // 接地時は垂直速度を即座にクリア
            if (velocity.y < 0f)
                velocity.y = 0f;
        }
    }

    private void ApplyVelocity(){
        // 動く床の速度を考慮（接地時のみプレイヤー速度に合算）
        Vector2 finalVelocity = velocity;
        if (groundCheck != null && isGrounded){
            Vector2 groundVelocity = groundCheck.GetGroundVelocity();
            finalVelocity += groundVelocity;
        }

        // AddForceは使わず、最終的な速度を直接適用
        rb.linearVelocity = finalVelocity;
    }

    private void HandleActionTriggered(InputAction.CallbackContext context){
        var mapName = context.action.actionMap != null ? context.action.actionMap.name : string.Empty;
        if (mapName != "Player") return;

        switch (context.action.name){
            case "Move":
                OnMove(context);
                break;
            case "Jump":
                OnJump(context);
                break;
            case "Attack":
                OnAttack(context);
                break;
            case "SpecialA":
                OnSpecialA(context);
                break;
        }
    }

    private void UpdateFacing(){
        if (moveInput.x > 0.1f) facingRight = true;
        else if (moveInput.x < -0.1f) facingRight = false;

        if (spriteRenderer != null)
            spriteRenderer.flipX = !facingRight;

        animator?.SetBool("FacingRight", facingRight);
    }

    private void UpdateAnimation(){
		animator?.SetBool("Walk", Mathf.Abs(moveInput.x) > 0.1f);
		// 攻撃中はJumpフラグを上書きしない（空中攻撃が吸い込まれるのを防止）
		if (!isAttacking)
			animator?.SetBool("Jump", !isGrounded);
    }

    // ===================== 攻撃処理 =====================
    public void OnAttack(InputAction.CallbackContext context){
        if (context.started && !isAttacking && !isDead){
            isAttacking = true;
            isAirAttacking = !isGrounded;
			animator?.SetBool("IsAttacking", true);

            if (animator != null){
                if (isAirAttacking){
                    animator.SetBool("Jump", false);
                    animator.Play(facingRight ? "AirAttack_Sword_Right" : "AirAttack_Sword_Left", 0, 0f);
                }else{
                    animator.ResetTrigger("Attack");
                    animator.SetTrigger("Attack");
                }
            }
            StartCoroutine(AttackRoutine());
            if (weaponBase != null)
                weaponBase.StartAttack(moveInput);
        }
    }

    private IEnumerator AttackRoutine(){
        float duration = isGrounded ? 0.3f : 0.6f;
        yield return new WaitForSeconds(duration);
        if (isAirAttacking) animator?.SetBool("Jump", true);
        isAttacking = false;
        isAirAttacking = false;
		animator?.SetBool("IsAttacking", false);
    }
    private void LookMoveDirection(){
        if (moveInput.x > 0.1f){
            facingRight = true;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }else if (moveInput.x < -0.1f){
            facingRight = false;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
        }

        if (spriteRenderer != null){
            if (!spriteRenderer.flipX && !facingRight){
                facingRight = true;
            }
            else if (spriteRenderer.flipX && facingRight){
                facingRight = false;
            }
        }
        swordHandler?.UpdateSwordDirection(facingRight);
        if (weaponBase != null){
            var swordWeapon = weaponBase.GetComponent<SwordWeapon>();
            if (swordWeapon != null){
                swordWeapon.SetFacingRight(facingRight);
            }
        }else{
            Debug.LogWarning("weaponBaseがnullです。InspectorでWeaponBaseを設定してください。");
        }
        if (animator != null){
            animator.SetBool("FacingRight", facingRight);
        }
    }

    // AnimationEvent から呼ばれる想定の受け先
    public void EndAttack(){
		if (isDead) return;
		if (isAirAttacking) animator?.SetBool("Jump", true);
		isAttacking = false;
		isAirAttacking = false;
		// 武器側の終了もリレー（イベントで終わる構成でも安全）
		if (weaponBase != null)
			weaponBase.EndAttack();
		animator?.SetBool("IsAttacking", false);
	}

    public void OnSpecialA(InputAction.CallbackContext context){
        if (!context.performed || sp < specialCost) return;
        UseSpecial(specialCost);
        var bulletObj = Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        var bullet = bulletObj.GetComponent<PlayerBullet>();
        if (bullet != null){
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            bullet.Setup(dir);
        }
    }

    // ===================== ダメージ処理 =====================
    private void OnCollisionEnter2D(Collision2D other){
        if (isInvincible || isDead) return;

        if (other.gameObject.CompareTag("Enemy")){
            HitEnemy(other.gameObject);
            hitEffectSpawner?.SpawnHitEffect(other.transform.position);
        }
    }

    private void HitEnemy(GameObject enemy){
        float playerY = transform.position.y;
        float enemyY = enemy.transform.position.y;

        if (playerY > enemyY + 0.4f){
            Destroy(enemy);
            velocity.y = jumpForce * 0.5f;
        }else{
            TakeDamage(1);
            StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash(){
        isInvincible = true;
        float elapsed = 0f;
        Color c = spriteRenderer.color;

        while (elapsed < damageTime){
            spriteRenderer.color = new Color(c.r, c.g, c.b, 0.2f);
            yield return new WaitForSeconds(flashTime);
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime * 2f;
        }
        spriteRenderer.color = c;
        isInvincible = false;
    }

    // ===================== ステータス制御 =====================
    public void TakeDamage(int dmg){
        if (isInvincible) return;
        hp = Mathf.Clamp(hp - dmg, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
        OnDamage?.Invoke();
        if (hp <= 0) Die();
    }

    public void HealHP(int amount){
        hp = Mathf.Clamp(hp + amount, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }

    public void UseSpecial(int cost){
        sp = Mathf.Clamp(sp - cost, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public void HealSP(int amount){
        sp = Mathf.Clamp(sp + amount, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    private void Die(){
        isDead = true;
        animator?.Play("Death");
        this.enabled = false;
    }

    // ===================== 入力 =====================
    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();

        Debug.Log($"moveInput.y -> {moveInput.y} | isGrounded -> {isGrounded} | isDropping -> {isDropping}");
        if (moveInput.y < -0.5f && isGrounded && !isDropping){
            Debug.Log("床抜けok");
            StartCoroutine(DropThroughPlatform());
        }
    }

    public void OnJump(InputAction.CallbackContext context){
        if (context.started){
            jumpPressed = true;
            jumpHeld = true;
        }else if (context.canceled){
            jumpHeld = false;
            // ジャンプボタンを離した時の即座の減速（より自然に）
            if (velocity.y > 0f && !jumpCutApplied){
                velocity.y *= jumpCutMultiplier;
                jumpCutApplied = true;
            }
        }
    }

    private IEnumerator DropThroughPlatform(){
        isDropping = true;
        // 直ちに非接地扱いにして下方向の初速を与える
        isGrounded = false;
        if (velocity.y > -2f) velocity.y = -2f;

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.transform.position, groundCheck.checkRadius, groundCheck.groundLayer);

        if (hit != null){
            // この床がプレイヤーの下抜けを許可しているか確認
            PlatformType platformType = hit.GetComponentInParent<PlatformType>();
            if (platformType == null || !platformType.allowDropThrough){
                isDropping = false;
                yield break;
            }

            PlatformEffector2D eff = hit.GetComponentInParent<PlatformEffector2D>();
            if (eff != null){
                float orig = eff.rotationalOffset;
                eff.rotationalOffset = 180f;
                yield return new WaitForSeconds(dropThroughTime);
                eff.rotationalOffset = orig;
            }
        }

        yield return new WaitForSeconds(0.05f);
        isDropping = false;
    }

    public int GetHP() { return hp; }
    public int GetSP() { return sp; }
    void OnDrawGizmosSelected(){
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheck.checkRadius);
    }
}
