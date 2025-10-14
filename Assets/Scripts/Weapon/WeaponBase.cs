/* =======================================
 * スクリプト名：WeaponBase.cs
 * 武器の基底クラス
 * =======================================
 */
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour{
    [Header("共通設定")]
    public int damage = 1;
    public float attackDuration = 0.3f;
    public Vector2 attackDirection = Vector2.right;

    [Header("エフェクト関連")]
    public ParticleSystem attackEffectPrefab;  // 攻撃時に再生するパーティクル
    public Transform effectSpawnPoint;         // エフェクトの生成位置

    protected bool isAttacking = false;
    protected Collider2D hitbox;

    protected virtual void Awake(){
        hitbox = GetComponent<Collider2D>();
        if (hitbox != null)
            hitbox.enabled = false;
    }

    // 攻撃開始（プレイヤーから呼ばれる）
    public virtual void StartAttack(Vector2 dir){
        if (isAttacking) return;
        attackDirection = dir;
        isAttacking = true;

        // エフェクト再生
        PlayEffect();

        if (hitbox != null)
            hitbox.enabled = true;

        OnAttackStart();
        
        // 空中攻撃の場合は少し長めに設定
        float duration = attackDuration;
        if (dir.y > 0.1f) // 上方向への攻撃（空中攻撃）の場合
            duration = attackDuration * 1.5f;
            
        Invoke(nameof(EndAttack), duration);
    }

    protected abstract void OnAttackStart();

    public virtual void EndAttack(){
        isAttacking = false;
        if (hitbox != null)
            hitbox.enabled = false;
        OnAttackEnd();
    }

    protected virtual void OnAttackEnd() { }

    protected virtual void OnTriggerEnter2D(Collider2D other){
        if (!isAttacking) return;

        if (other.CompareTag("Enemy")){
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null){
                enemy.TakeDamage(damage);
                Debug.Log($"{name} → {enemy.name} に {damage} ダメージ!");
            }
        }
    }

    // 🔥 エフェクト再生
    protected virtual void PlayEffect(){
        if (attackEffectPrefab != null){
            ParticleSystem effect = Instantiate(
                attackEffectPrefab,
                effectSpawnPoint != null ? effectSpawnPoint.position : transform.position,
                Quaternion.identity
            );
            effect.Play();
            Destroy(effect.gameObject, 2f); // 2秒で自動破棄
        }
    }
}
