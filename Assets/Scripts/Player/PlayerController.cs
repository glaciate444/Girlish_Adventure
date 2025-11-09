/* =======================================
 * ファイル名 : PlayerController.cs
 * 概要 : プレイヤースクリプト
 * Created Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * 更新内容 : AddForce除外
 * ======================================= */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour{
    [Header("基本ステータス")]
    [SerializeField] private PlayerData playerData;
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
    [SerializeField, Range(0f, 80f)] private float maxSlopeAngle = 65f; // 登坂可能な最大角度
    [Header("攻撃関連")]
    [SerializeField] private SwordFlipHandler swordHandler;
    [SerializeField] private WeaponBase weaponBase;
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpawnForwardOffset = 0.2f; // 地面との食い込み防止（前方）
    [SerializeField] private float bulletSpawnUpOffsetGrounded = 0.06f; // 接地時に少しだけ上げる
    [SerializeField] private int specialCost = 1;
    [SerializeField] private float attackGroundDuration = 0.3f;
    [SerializeField] private float attackAirDuration = 0.6f;
    [Header("Ground Check")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private float dropThroughTime = 0.3f;
    [SerializeField] private float coyoteTime = 0.1f;
    [Header("梯子乗降速度")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float ladderMaxSnapWidth = 1.5f; // これより幅広い梯子コライダーではXスナップしない
    [SerializeField] private float ladderReleaseFrames = 30f;
    [Header("アニメーション / 無敵点滅")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HitEffectSpawner hitEffectSpawner;
    [SerializeField] private float damageTime = 3f;
    [SerializeField] private float flashTime = 0.2f;
    // プラットフォーム吸着・スナップ機構は完全撤廃
    [Header("一方通行床との衝突フィルタ")]
    [SerializeField] private bool usePlatformCollisionFilter = true;
    [SerializeField] private float platformTopTolerance = 0.02f;

    // 内部変数
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 velocity;
    private bool facingRight = true;
    private bool isGrounded;
    private bool isDropping;
    private bool isDead;
    private bool wasGrounded;
    private PlayerInput playerInput;
    private PlayerJumpHandler jumpHandler = new PlayerJumpHandler();
    private PlayerMotionHandler motionHandler = new PlayerMotionHandler();
    private PlayerEnvironmentHandler environmentHandler = new PlayerEnvironmentHandler();
    private PlayerAttackHandler attackHandler = new PlayerAttackHandler();
    private PlayerHealthHandler healthHandler = new PlayerHealthHandler();
    private CapsuleCollider2D capsuleCollider;
    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();
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
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnValidate(){
        if (!Application.isPlaying)
            return;

        ConfigureModules();
    }

    private void Start(){
        // PlayerDataが設定されていない場合は警告
        if (playerData == null){
            Debug.LogError("PlayerDataが設定されていません。InspectorでPlayerDataを設定してください。");
            return;
        }

        // PlayerDataで初期化（UI同期も含む）
        playerData.InitializeStatus();
        ConfigureModules();

        // UI がまだ未登録のことがあるため、登録を待ってから同期
        StartCoroutine(InitialSyncUIRoutine());
    }

    private void ConfigureModules(){
        jumpHandler.Configure(playerData, animator, jumpCutMultiplier, coyoteTime);
        motionHandler.Configure(playerData, groundCheck, airControlFactor, maxFallSpeed, groundAcceleration, groundDeceleration, airAcceleration, airDeceleration, forceAirStrafe, minAirStrafeSpeed, maxSlopeAngle);
        environmentHandler.Configure(animator, rb, transform, climbSpeed, ladderMaxSnapWidth, ladderReleaseFrames);
        attackHandler.Configure(this, gameObject, animator, weaponBase, playerBulletPrefab, firePoint, attackGroundDuration, attackAirDuration, specialCost, bulletSpawnForwardOffset, bulletSpawnUpOffsetGrounded);
        healthHandler.Configure(this, playerData, spriteRenderer, hitEffectSpawner, damageTime, flashTime, Die, () => OnDamage?.Invoke());
    }

    private IEnumerator InitialSyncUIRoutine(){
        // UIManager の準備を待機
        while (UIManager.Instance == null)
            yield return null;

        // PlayerData内でUI更新
        playerData.SyncUI();
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
		//LookMoveDirection();
        UpdateAnimation();
    }

    private void FixedUpdate(){
        if (isDead) return;

        environmentHandler.UpdateJumpPadTimer(Time.fixedDeltaTime);

        if (environmentHandler.IsClimbing){
            if (environmentHandler.UpdateClimb(moveInput, ref velocity, jumpHandler.HasJumpRequest))
                return;
        }

        if (usePlatformCollisionFilter)
            UpdatePlatformCollisionFilter();

        UpdateGroundState();
        motionHandler.UpdateMovement(moveInput, isGrounded, attackHandler.IsAttacking, ref velocity, Time.fixedDeltaTime);
        jumpHandler.ProcessJump(ref velocity, isGrounded, attackHandler.IsAttacking, moveInput, forceAirStrafe, minAirStrafeSpeed);
        motionHandler.ApplyGravity(isGrounded, ref velocity, Time.fixedDeltaTime);

        // 最終速度（坂投影 + 動く床加算）
        Vector2 finalVelocity = motionHandler.GetFinalVelocity(velocity, isGrounded);
        rb.linearVelocity = finalVelocity;
        // 内部状態は「自前の相対速度」のまま保持（動く床の速度は蓄積しない）
        if (isGrounded && groundCheck != null){
            Vector2 gv = groundCheck.GetGroundVelocity();
            velocity.y = finalVelocity.y - gv.y;
        }else{
            velocity.y = finalVelocity.y;
        }
    }

    private void UpdateGroundState(){
        bool groundedNow = groundCheck != null ? groundCheck.IsGrounded :
            Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        // ドロップ中は強制的に非接地扱いにする（重力を有効にして下に抜ける）
        isGrounded = isDropping ? false : groundedNow;
        bool justLanded = isGrounded && !wasGrounded;
        jumpHandler.UpdateGroundedState(isGrounded, Time.fixedDeltaTime);

        animator?.SetBool("IsGrounded", isGrounded);

        // スナップ処理は完全撤廃

        wasGrounded = isGrounded;
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
        if (environmentHandler.IsClimbing){
            // 梯子中はWalkアニメをオフにして固定
            animator?.SetBool("Walk", false);
            // 梯子にいる間は常にisClimbingをtrueに維持（静止時も登り状態を維持）
            animator?.SetBool("isClimbing", true);
            return;
        }

        animator?.SetBool("Walk", Mathf.Abs(moveInput.x) > 0.1f);
        // 攻撃中はJumpフラグを上書きしない（空中攻撃が吸い込まれるのを防止）
        // 梯子中はJumpアニメを無効化する（地上判定に依存しない）
        if (!attackHandler.IsAttacking)
            animator?.SetBool("Jump", !isGrounded);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Ladder"))
            environmentHandler.OnLadderTriggerEnter(other);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.CompareTag("Ladder"))
            environmentHandler.OnLadderTriggerExit(ref velocity);
    }


    // ===================== 攻撃処理 =====================
    public void OnAttack(InputAction.CallbackContext context){
        if (!context.started || isDead) return;
        attackHandler.TryStartAttack(isGrounded, moveInput, facingRight);
    }

    public void EndAttack(){
        if (isDead) return;
        attackHandler.EndAttackFromAnimation();
    }

    public void OnSpecialA(InputAction.CallbackContext context){
        if (!context.performed) return;
        attackHandler.TrySpecialAttack(playerData, facingRight, isGrounded);
    }

    // ===================== ダメージ処理 =====================
    private void OnCollisionEnter2D(Collision2D other){
        if (isDead) return;

        if (other.gameObject.CompareTag("Enemy"))
            healthHandler.HandleEnemyCollision(other.gameObject, transform, ref velocity);
    }

    public void TakeDamage(int dmg){
        healthHandler.ApplyDamage(dmg);
    }

    public void HealHP(int amount){
        healthHandler.HealHP(amount);
    }

    public void UseSpecial(int cost){
        healthHandler.UseSpecial(cost);
    }

    public void HealSP(int amount){
        healthHandler.HealSP(amount);
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
        // 梯子判定
        if (environmentHandler.IsOnLadder){
            if (!environmentHandler.IsClimbing){
                // 縦入力が明確なら登り開始
                if (Mathf.Abs(moveInput.y) > 0.2f){
                    environmentHandler.BeginClimb(ref velocity);
                }
                // 横入力が明確なら梯子を抜ける
                else if (Mathf.Abs(moveInput.x) > 0.3f){
                    environmentHandler.ExitLadder(ref velocity);
                }
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context){
        if (context.started){
            jumpHandler.OnJumpStarted();
        }else if (context.canceled){
            jumpHandler.OnJumpCanceled(ref velocity);
        }
    }

    private IEnumerator DropThroughPlatform(){
        isDropping = true;
        // 直ちに非接地扱いにして下方向の初速を与える
        isGrounded = false;
        wasGrounded = false;
        if (velocity.y > -2f) velocity.y = -2f;

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.transform.position, groundCheck.checkRadius, groundCheck.groundLayer | LayerMask.GetMask("Platform"));

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

    public int GetHP() { return playerData != null ? playerData.hp : 0; }
    public int GetSP() { return playerData != null ? playerData.sp : 0; }

    // === JumpPad から呼ばれる ===
    public void ActivateJumpPad(float bounceHeight, float duration){
        if (isDead) return;

        environmentHandler.ActivateJumpPad(ref velocity, bounceHeight, duration);
    }
    public bool IsOnJumpPad => environmentHandler.IsOnJumpPad;

    void OnDrawGizmosSelected(){
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheck.checkRadius);
    }

    private void UpdatePlatformCollisionFilter(){
        if (capsuleCollider == null) return;

        // 近傍の Platform コライダーを取得
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(groundCheck.transform.position, Mathf.Max(groundCheck.checkRadius, 0.25f), LayerMask.GetMask("Platform"));

        // 今回検出された集合を一時的にマーク
        HashSet<Collider2D> seen = new HashSet<Collider2D>();
        foreach (var col in overlaps){
            if (col == null) continue;
            seen.Add(col);

            float platformTop = col.bounds.max.y;
            // AABBでは広すぎる場合があるため、そのX位置の実際の天面をレイで取得
            {
                Bounds pb = capsuleCollider.bounds;
                Vector2 probeFrom = new Vector2(pb.center.x, pb.min.y + 2f);
                RaycastHit2D rh = Physics2D.Raycast(probeFrom, Vector2.down, 4f, LayerMask.GetMask("Platform"));
                if (rh.collider != null && rh.collider == col){
                    platformTop = rh.point.y;
                }
            }
            float playerBottom = capsuleCollider.bounds.min.y;

            bool belowTop = playerBottom < platformTop - platformTopTolerance;
            bool rising = velocity.y > 0f;
            bool shouldIgnore = belowTop || rising || isDropping;

            if (shouldIgnore){
                if (!ignoredPlatforms.Contains(col)){
                    Physics2D.IgnoreCollision(capsuleCollider, col, true);
                    ignoredPlatforms.Add(col);
                }
            }else{
                if (ignoredPlatforms.Contains(col)){
                    Physics2D.IgnoreCollision(capsuleCollider, col, false);
                    ignoredPlatforms.Remove(col);
                }
            }
        }

        // 見えなくなった（範囲外になった）コライダーは念のため再有効化
        if (ignoredPlatforms.Count > 0){
            var toEnable = new List<Collider2D>();
            foreach (var col in ignoredPlatforms){
                if (col == null || !seen.Contains(col)){
                    toEnable.Add(col);
                }
            }
            foreach (var col in toEnable){
                if (col != null)
                    Physics2D.IgnoreCollision(capsuleCollider, col, false);
                ignoredPlatforms.Remove(col);
            }
        }
    }

    // スナップ機能は完全撤廃したため、関連メソッドも削除しました
}
