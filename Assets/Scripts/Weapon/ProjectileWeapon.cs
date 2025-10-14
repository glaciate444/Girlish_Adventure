using UnityEngine;

public class ProjectileWeapon : WeaponBase{
    [Header("��ѓ���ݒ�")]
    public GameObject projectilePrefab;     // �e�▂�@��Prefab
    public float projectileSpeed = 10f;
    public Transform firePoint;             // ���ˈʒu

    protected override void OnAttackStart(){
        if (projectilePrefab == null || firePoint == null){
            Debug.LogWarning($"{name}: projectilePrefab �܂��� firePoint �����ݒ�ł��B");
            return;
        }

        // �e�ې���
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null){
            projectile.Setup(attackDirection, damage, projectileSpeed);
        }

        // �p�[�e�B�N���Đ��i�Ⴆ�Ζ��@�w�G�t�F�N�g�Ȃǁj
        PlayEffect();
    }
}