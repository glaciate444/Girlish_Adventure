using UnityEngine;

public class WeaponManager : MonoBehaviour {
    [Header("現在装備中の武器")]
    public WeaponBase currentWeapon;

    [Header("プレイヤーの向き")]
    public bool facingRight = true;

    public void Attack(Vector2 inputDir){
        if (currentWeapon == null) return;

        Vector2 dir = Vector2.zero;

        if (inputDir.y > 0.5f){
            if (Mathf.Abs(inputDir.x) > 0.5f)
                dir = new Vector2(Mathf.Sign(inputDir.x), 1f).normalized;
            else
                dir = Vector2.up;
        }else{
            dir = facingRight ? Vector2.right : Vector2.left;
        }

        // 🔥 武器に向きを伝える
        if (currentWeapon is SwordWeapon sword)
            sword.facingRight = facingRight;

        currentWeapon.StartAttack(dir);
    }

    public void Flip(bool right){
        facingRight = right;
        // スケール反転はしない。位置/向きは SwordFlipHandler と攻撃方向で制御する。
    }
}
