using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerData")]
public class PlayerData : ScriptableObject{
    [Header("ステータス")]
    public int maxHP = 10;
    public int hp = 10;
    public int maxSP = 6;
    public int sp = 6;
    public int coin = 0;

    [Header("基本能力")]
    public float moveSpeed = 7f;
    public float jumpForce = 13f;
    public float gravity = -50f;

    public delegate void OnStatusChanged();
    public event OnStatusChanged OnStatusUpdated;

    public void TakeDamage(int dmg){
        hp = Mathf.Clamp(hp - dmg, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
        OnStatusUpdated?.Invoke();
    }

    public void HealHP(int amount){
        hp = Mathf.Clamp(hp + amount, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }

    public void UseSP(int cost){
        sp = Mathf.Clamp(sp - cost, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public void UseSpecial(int cost){
        UseSP(cost);
    }

    public void HealSP(int amount){
        sp = Mathf.Clamp(sp + amount, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public void ResetStatus(){
        hp = maxHP;
        sp = maxSP;
        coin = 0;
    }

    public void InitializeStatus(){
        if (hp <= 0) hp = maxHP;
        if (sp <= 0) sp = maxSP;
        SyncUI();
    }

    public void SyncUI(){
        UIManager.Instance?.UpdateHP(hp, maxHP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }
}
