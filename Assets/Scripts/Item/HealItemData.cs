using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HealEffect {
    public HealType type;
    public int amount;
}

public enum HealType { HP, SP } // ‚Ì‚¿‚ÉŠg’£‚·‚é

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