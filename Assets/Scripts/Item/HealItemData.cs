/* =======================================
 * ファイル名 : HealItemData.cs (ScriptableObject型)
 * 概要 : 回復スクリプト
 * Created Date : 2025/10/14
 * Date : 2025/10/14
 * Version : 0.01
 * 更新内容 : 新規作成
 * 補足 : ScriptableObject
 * ======================================= */
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HealEffect {
    public HealType type;
    public int amount;
}

public enum HealType { HP, SP } // のちに拡張する

[CreateAssetMenu(menuName = "Items/Multi Heal Item")]
public class HealItemData : ScriptableObject {
    [SerializeField] private List<HealEffect> healEffects = new();

    public void Apply(PlayerController player){
        foreach (var effect in healEffects){
            switch (effect.type){
                case HealType.HP:
                    player.HealHP(effect.amount);
                    break;
                case HealType.SP:
                    player.HealSP(effect.amount);
                    break;
            }
        }
    }
}