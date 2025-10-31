/* =======================================
 * ファイル名 : CoinManager.cs
 * 概要 : コイン管理スクリプト
 * Created Date : 2025/10/14
 * Date : 2025/10/31
 * Version : 0.02
 * 更新内容 : UIManagerに対応
 * ======================================= */
using UnityEngine;

public class CoinManager : MonoBehaviour{
    public static CoinManager Instance { get; private set; }

    private int totalCoins;

    private void Awake(){
        // UIは親（UIManager）配下で管理する。ルート化やDontDestroyOnLoadは行わない。
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start(){
        // AwakeではUIManagerがまだ生成されていない可能性があるため
        // Startタイミングで初回UIを同期する
        TryUpdateUI();
    }

    public void AddCoins(int amount){
        totalCoins += amount;
        Debug.Log($"コインを{amount}枚取得、合計{totalCoins}枚");
        TryUpdateUI();
    }

    private void TryUpdateUI(){
        var ui = GameManager.Instance?.UI;
        if (ui != null)
            ui.UpdateCoinUI(totalCoins);
        else
            Debug.LogWarning("UIManagerが存在しないためUI更新できません。");
    }

    public int GetTotalCoins() => totalCoins;
}
