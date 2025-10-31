/* =======================================
 * ファイル名 : Coin.cs
 * 概要 : コイン取得スクリプト
 * Created Date : 2025/10/14
 * Date : 2025/10/14
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
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