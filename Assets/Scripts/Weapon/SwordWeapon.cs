using UnityEngine;
using System.Collections;

public class SwordWeapon : WeaponBase {
    [Header("攻撃方向ごとのローカル回転")]
    [SerializeField] private float upAngle = 0f;
    [SerializeField] private float diagonalAngle = 45f;
    [SerializeField] private float forwardAngle = 90f;

    [Header("向き制御")]
    public bool facingRight = true;
    
    // 向きを設定するメソッド
    public void SetFacingRight(bool facing){
        facingRight = facing;
    }

    protected override void OnAttackStart(){
        Debug.Log($"SwordWeapon.OnAttackStart呼び出し - 現在のisAttacking: {isAttacking}");
        
        Vector2 dir = attackDirection.normalized;

        bool isUp = dir.y > 0.9f;
        bool isDiagUp = !isUp && (dir.y > 0.5f && Mathf.Abs(dir.x) > 0.5f);

        float angle = forwardAngle;
        if (isUp)
            angle = upAngle;
        else if (isDiagUp)
            angle = diagonalAngle;

        // ✅ 符号は攻撃方向からのみ判定する（facingRightを重複使用しない）
        float sign = Mathf.Sign(dir.x == 0 ? (facingRight ? 1f : -1f) : dir.x);
        
        // デバッグログを追加
        Debug.Log($"SwordWeapon - dir: {dir}, facingRight: {facingRight}, sign: {sign}, angle: {angle}");

        // 左右方向で角度反転
        transform.localRotation = Quaternion.Euler(0f, 0f, angle * sign);

        // 剣を表示
        gameObject.SetActive(true);
        
        // 攻撃判定の有効化をログ出力
        Debug.Log($"SwordWeapon攻撃開始 - 剣を表示、コライダー有効化、isAttacking: {isAttacking}");
    }

    protected override void OnAttackEnd(){
        transform.localRotation = Quaternion.identity;
        if (gameObject.activeInHierarchy)
            StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide(){
        yield return new WaitForSeconds(0.2f);
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }
}
