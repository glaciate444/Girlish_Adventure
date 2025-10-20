using UnityEngine;

[RequireComponent(typeof(PlatformEffector2D))]
public class PlatformType : MonoBehaviour {
    [Tooltip("プレイヤーが↓+ジャンプで床を落下できるならON。falseで下キー通過不可（ただし一方通行は有効）")]
    public bool allowDropThrough = false;

    // エディタから切り替えたいだけなら Awakeで設定する必要は無いが、
    // ここでEffectorの初期設定を行うことも可能。
    void Awake(){
        var eff = GetComponent<PlatformEffector2D>();
        if (eff != null){
            // 通常はOneWayを有効にしておく（上からのみ接触）
            eff.useOneWay = true;
            eff.surfaceArc = 160f; // あなたが調整済みの値
        }
    }
    // 切り替えメソッド(外部で切り替える)
    public void SetDropThrough(bool enable){
        allowDropThrough = enable;
    }
    /* 拡張事項
     * 敵専用の一方通行制御：敵AIが Platform レイヤーを無視する設定をすれば、敵だけすり抜ける床も作れます。
     * 特殊エフェクトやアニメーション連動：すり抜け開始時に PlatformType にイベント通知して床の透明度を下げるなども可能。
     */
}
