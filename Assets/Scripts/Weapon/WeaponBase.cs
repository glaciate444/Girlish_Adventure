/* =======================================
 * スクリプト名：WeaponBase.cs
 * 武器の基底クラス
 * Update : 2025/10/23
 * Version : ver0.02
 * =======================================
 */
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour{
    [Header("共通設定")]
    public int damage = 1;
    public float attackDuration = 0.3f;
    public Vector2 attackDirection = Vector2.right;

    [Header("エフェクト関連")]
    public GameObject hitAnimationPrefab;       // Spriteアニメ付き
    public ParticleSystem attackEffectPrefab;   // 攻撃時に再生するパーティクル
    public Transform effectSpawnPoint;          // エフェクトの生成位置

    protected bool isAttacking = false;
    protected Collider2D hitbox;

    protected virtual void Awake(){
        hitbox = GetComponent<Collider2D>();
        if (hitbox != null)
            hitbox.enabled = false;
    }

    // 攻撃開始（プレイヤーから呼ばれる）
    public virtual void StartAttack(Vector2 dir){
        Debug.Log($"WeaponBase.StartAttack呼び出し - 現在のisAttacking: {isAttacking}");
        
        if (isAttacking) {
            Debug.Log("既に攻撃中のため、StartAttackをスキップ");
            return;
        }
        
        attackDirection = dir;
        isAttacking = true;
        Debug.Log($"WeaponBase攻撃開始 - isAttackingをtrueに設定: {isAttacking}");

        if (hitbox != null){
            hitbox.enabled = true;
            Debug.Log($"WeaponBase攻撃開始 - コライダー有効化: {hitbox.name}, isTrigger: {hitbox.isTrigger}, enabled: {hitbox.enabled}");
        }
        else{
            Debug.LogError("WeaponBase - hitboxがnullです！");
        }

        OnAttackStart();
        
        // 空中攻撃の場合は少し長めに設定
        float duration = attackDuration;
        if (dir.y > 0.1f) // 上方向への攻撃（空中攻撃）の場合
            duration = attackDuration * 1.5f;
            
        Debug.Log($"WeaponBase攻撃開始 - 攻撃継続時間: {duration}秒");
        Invoke(nameof(EndAttack), duration);
    }

    protected abstract void OnAttackStart();

    public virtual void EndAttack(){
        Debug.Log($"WeaponBase攻撃終了 - isAttackingをfalseに設定: {isAttacking}");
        isAttacking = false;
        if (hitbox != null){
            hitbox.enabled = false;
            Debug.Log($"WeaponBase攻撃終了 - コライダー無効化: {hitbox.name}");
        }
        OnAttackEnd();
    }

    protected virtual void OnAttackEnd() { }

    protected virtual void OnTriggerEnter2D(Collider2D other){
        Debug.Log($"OnTriggerEnter2D呼び出し - isAttacking: {isAttacking}, other: {other.name}, tag: {other.tag}");
        
        if (!isAttacking) {
            Debug.Log("攻撃中ではないため、OnTriggerEnter2Dをスキップ");
            return;
        }

        if (other.CompareTag("Enemy")){
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null){
                enemy.TakeDamage(damage);
                Debug.Log($"{name} → {enemy.name} に {damage} ダメージ!");
                AnimeEffect();
            }else{
                Debug.LogWarning($"Enemyタグのオブジェクト {other.name} にBaseEnemyコンポーネントがありません");
            }
        }else{
            Debug.Log($"Enemyタグではないオブジェクト: {other.name} (tag: {other.tag})");
        }
    }
    
    protected virtual void OnTriggerStay2D(Collider2D other){
        Debug.Log($"OnTriggerStay2D呼び出し - isAttacking: {isAttacking}, other: {other.name}, tag: {other.tag}");
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other){
        Debug.Log($"OnTriggerExit2D呼び出し - isAttacking: {isAttacking}, other: {other.name}, tag: {other.tag}");
    }

    // 🔥 エフェクト再生
    protected virtual void PlayEffect(){
        // ベース位置（エフェクト生成位置 or 武器位置）
        Vector3 spawnPos = effectSpawnPoint != null
            ? effectSpawnPoint.position
            : transform.position;

        // ✅ X軸オフセットを追加（剣先方向へずらす）
        // 例: 正面方向に +0.5f 移動させる
        float xOffset = 0.5f;

        // facingRightは PlayerController などから伝達済み前提
        if (TryGetComponent(out SpriteRenderer sr)){
            // スプライトの反転状態を利用（右向きなら+、左向きなら-）
            float sign = sr.flipX ? -1f : 1f;
            spawnPos += new Vector3(xOffset * sign, 0f, 0f);
        }else{
            // fallback: transformの向きで判定
            float sign = transform.lossyScale.x >= 0 ? 1f : -1f;
            spawnPos += new Vector3(xOffset * sign, 0f, 0f);
        }
        if (attackEffectPrefab != null){
            // パーティクル生成
            var effect = Instantiate(attackEffectPrefab, spawnPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }
    }
    // アニメーション再生（Animator.Play方式）
    protected void AnimeEffect(){
        if (hitAnimationPrefab != null){
            // ベース位置（エフェクト生成位置 or 武器位置）
            Vector3 spawnPos = effectSpawnPoint != null
                ? effectSpawnPoint.position
                : transform.position;

            // ✅ X軸オフセットを追加（剣先方向へずらす）
            // 例: 正面方向に +0.5f 移動させる
            float xOffset = 0.5f;

            // facingRightは PlayerController などから伝達済み前提
            if (TryGetComponent(out SpriteRenderer sr)){
                // スプライトの反転状態を利用（右向きなら+、左向きなら-）
                float sign = sr.flipX ? -1f : 1f;
                spawnPos += new Vector3(xOffset * sign, 0f, 0f);
            }else{
                // fallback: transformの向きで判定
                float sign = transform.lossyScale.x >= 0 ? 1f : -1f;
                spawnPos += new Vector3(xOffset * sign, 0f, 0f);
            }

            var animObj = Instantiate(hitAnimationPrefab, spawnPos, Quaternion.identity);
            var animator = animObj.GetComponent<Animator>();
            if (animator != null)
                animator.Play("HitFlash", 0, 0f);
            Destroy(animObj, 0.5f); // アニメ終了後に破棄
        }
    }


}
