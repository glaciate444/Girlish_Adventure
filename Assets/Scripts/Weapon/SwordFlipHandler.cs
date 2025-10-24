using UnityEngine;

public class SwordFlipHandler : MonoBehaviour{
    [SerializeField] private Transform swordTransform;
    [SerializeField] private SwordWeapon swordWeapon;
    [SerializeField] private Vector3 rightLocalPosition = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 leftLocalPosition = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private SpriteRenderer swordSprite; // ★ 追加

    public void UpdateSwordDirection(bool facingRight){
        // 参照未設定なら WeaponManager から補完
        if (swordTransform == null){
            if (swordWeapon == null) swordWeapon = GetComponentInParent<SwordWeapon>();
        }

        if (swordTransform == null) return;

        // 位置を切り替え
        swordTransform.localPosition = facingRight ? rightLocalPosition : leftLocalPosition;

        // ★ スプライトを左右反転
        if (swordSprite == null)
            swordSprite = swordTransform.GetComponentInChildren<SpriteRenderer>();

        if (swordSprite != null)
            swordSprite.flipX = !facingRight;
    }
}