using UnityEngine;

public class ProjectileWeapon : WeaponBase{
    [Header("飛び道具設定")]
    public GameObject projectilePrefab;     // 弾や魔法のPrefab
    public float projectileSpeed = 10f;
    public Transform firePoint;             // 発射位置

    protected override void OnAttackStart(){
        if (projectilePrefab == null || firePoint == null){
            Debug.LogWarning($"{name}: projectilePrefab または firePoint が未設定です。");
            return;
        }

        // 弾丸生成
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null){
            projectile.Setup(attackDirection, damage, projectileSpeed);
        }

        // パーティクル再生（例えば魔法陣エフェクトなど）
        PlayEffect();
    }
}