/* =======================================
 * ファイル名 : HealItem.cs
 * 概要 : 回復アイテム取得スクリプト
 * Created Date : 2025/10/14
 * Date : 2025/10/14
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class HealItem : MonoBehaviour {
    [SerializeField] private HealItemData healItemData;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            var player = other.GetComponent<PlayerController>();
            if (player != null){
                healItemData.Apply(player);
                Destroy(gameObject);
            }
        }
    }
}