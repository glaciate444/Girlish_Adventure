using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerHealthHandler
{
    private MonoBehaviour coroutineRunner;
    private PlayerData playerData;
    private SpriteRenderer spriteRenderer;
    private HitEffectSpawner hitEffectSpawner;
    private float damageTime = 3f;
    private float flashTime = 0.2f;

    private bool isInvincible;
    private Coroutine damageRoutine;

    private System.Action onDeathCallback;
    private System.Action onDamageCallback;

    public void Configure(
        MonoBehaviour coroutineRunner,
        PlayerData playerData,
        SpriteRenderer spriteRenderer,
        HitEffectSpawner hitEffectSpawner,
        float damageTime,
        float flashTime,
        System.Action onDeathCallback,
        System.Action onDamageCallback)
    {
        this.coroutineRunner = coroutineRunner;
        this.playerData = playerData;
        this.spriteRenderer = spriteRenderer;
        this.hitEffectSpawner = hitEffectSpawner;
        this.damageTime = Mathf.Max(0f, damageTime);
        this.flashTime = Mathf.Max(0.01f, flashTime);
        this.onDeathCallback = onDeathCallback;
        this.onDamageCallback = onDamageCallback;
    }

    public void HandleEnemyCollision(GameObject enemy, Transform playerTransform, ref Vector2 velocity)
    {
        if (isInvincible || enemy == null || playerTransform == null)
            return;

        float playerY = playerTransform.position.y;
        float enemyY = enemy.transform.position.y;

        if (playerData != null && playerY > enemyY + 0.4f)
        {
            Object.Destroy(enemy);
            velocity.y = playerData.jumpForce * 0.5f;
            return;
        }

        ApplyDamage(1);
        hitEffectSpawner?.SpawnHitEffect(enemy.transform.position);
    }

    public void ApplyDamage(int damage)
    {
        if (isInvincible || playerData == null || damage <= 0)
            return;

        playerData.TakeDamage(damage);
        onDamageCallback?.Invoke();

        if (playerData.hp <= 0)
        {
            onDeathCallback?.Invoke();
            return;
        }

        StartDamageRoutine();
    }

    public void HealHP(int amount)
    {
        if (playerData == null || amount <= 0)
            return;

        playerData.HealHP(amount);
    }

    public bool UseSpecial(int cost)
    {
        if (playerData == null || cost <= 0)
            return false;

        if (playerData.sp < cost)
            return false;

        playerData.UseSpecial(cost);
        return true;
    }

    public void HealSP(int amount)
    {
        if (playerData == null || amount <= 0)
            return;

        playerData.HealSP(amount);
    }

    private void StartDamageRoutine()
    {
        if (coroutineRunner == null)
            return;

        if (damageRoutine != null)
            coroutineRunner.StopCoroutine(damageRoutine);

        damageRoutine = coroutineRunner.StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        isInvincible = true;

        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < damageTime)
            {
                spriteRenderer.color = new Color(original.r, original.g, original.b, 0.2f);
                yield return new WaitForSeconds(flashTime);
                spriteRenderer.color = new Color(original.r, original.g, original.b, 1f);
                yield return new WaitForSeconds(flashTime);
                elapsed += flashTime * 2f;
            }

            spriteRenderer.color = original;
        }
        else
        {
            yield return new WaitForSeconds(damageTime);
        }

        isInvincible = false;
        damageRoutine = null;
    }

    public bool IsInvincible => isInvincible;
}
