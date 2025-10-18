using UnityEngine;

public class Coin : MonoBehaviour {
    [SerializeField] private int addCoins = 1; // 数値はインスペクターで

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            // コイン取得を通知
            CoinManager.Instance.AddCoins(addCoins);

            // コインオブジェクトを削除
            Destroy(gameObject);
        }
    }
}