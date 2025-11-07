/* =======================================
 * ファイル名 : PlayerSaveData.cs
 * 概要 : セーブロードファイル処理スクリプト
 * Create Date : 2025/11/07
 * Date : 2025/11/07
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveSlot {
    public int slotNumber;
    public int stageProgress;
    public PlayerSaveData player;
    public GameSettings settings;
}

[System.Serializable]
public class PlayerSaveData {
    public int level;
    public int hp;
    public int sp;
    public int coin;
    public string[] inventory;
}

[System.Serializable]
public class GameSettings {
    public float bgmVolume = 1.0f;
    public float seVolume = 1.0f;
    public bool fullscreen = true;
}

public static class SaveData {
    private static string SavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static void Save(int slot, SaveSlot data){
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath(slot), json);
        Debug.Log($"Saved Slot {slot}: {SavePath(slot)}");
    }

    public static SaveSlot Load(int slot){
        string path = SavePath(slot);
        if (File.Exists(path)){
            string json = File.ReadAllText(path);
            SaveSlot data = JsonUtility.FromJson<SaveSlot>(json);
            Debug.Log($"Loaded Slot {slot}");
            return data;
        }else{
            Debug.LogWarning($"No save file found for Slot {slot}");
            return new SaveSlot { slotNumber = slot, player = new PlayerSaveData(), settings = new GameSettings() };
        }
    }

    public static bool Exists(int slot){
        return File.Exists(SavePath(slot));
    }
}
