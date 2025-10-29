using UnityEngine;

public class CoinManager : MonoBehaviour {
    public static CoinManager Instance{ get; private set; }
    private int totalCoins;
    private void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも保持
        }else{
            Destroy(gameObject);
        }
    }
    public void AddCoins(int amount){
        totalCoins += amount;
        Debug.Log($"コインを{amount}枚取得、合計{totalCoins}枚");

        var ui = GameManager.Instance?.UI;
        if (ui != null)
            ui.UpdateCoinUI(totalCoins);
        else
            Debug.LogWarning("UIManagerが存在しないためUI更新できません。");
    }
    public int GetTotalCoins() => totalCoins;
}
