using UnityEngine;

public class SwordWeapon : WeaponBase{
    [Header("攻撃方向ごとのローカル回転")]
    public float upAngle = 0f;
    public float diagonalAngle = 45f;
    public float forwardAngle = 90f;

    // WeaponManagerなどから設定してもらう用
    public bool facingRight = true;

    protected override void OnAttackStart(){
        Vector2 dir = attackDirection.normalized;

        bool isUp = dir.y > 0.9f;
        bool isDiagUp = !isUp && (dir.y > 0.5f && Mathf.Abs(dir.x) > 0.5f);

        float angle = forwardAngle;
        if (isUp){
            angle = upAngle;
        }else if (isDiagUp){
            angle = diagonalAngle;
        }

        // 攻撃方向符号（右なら+、左なら-）
        float sign = isUp ? 1f : Mathf.Sign(dir.x == 0 ? 1f : dir.x);

        // 🔥 左右線対称：左向き時は角度を反転
        if (!facingRight)
            sign *= -1f;

        transform.localRotation = Quaternion.Euler(0, 0, angle * sign);
        
        // 剣の表示を確実にする
        gameObject.SetActive(true);
    }
    protected override void OnAttackEnd(){
        transform.localRotation = Quaternion.identity;
        // 空中攻撃の場合は剣を少し長く表示する
        if (gameObject.activeInHierarchy) {
            StartCoroutine(DelayedHide());
        }
    }
    
    private System.Collections.IEnumerator DelayedHide(){
        yield return new WaitForSeconds(0.2f); // 0.2秒後に非表示
        if (gameObject.activeInHierarchy) {
            gameObject.SetActive(false);
        }
    }
}
