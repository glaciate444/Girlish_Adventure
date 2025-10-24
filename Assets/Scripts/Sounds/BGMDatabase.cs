/* =======================================
 * ファイル名 : BGMDatabase.cs
 * 概要 : BGMデータベーススクリプト
 * Date : 2025/10/24
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BGMEntry{
    public int id;          // 例: 3 = Stage1
    public string label;    // "Stage1"
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "BGMDatabase", menuName = "Sound/BGMDatabase")]
public class BGMDatabase : ScriptableObject{
    public List<BGMEntry> bgms;
    private Dictionary<int, AudioClip> dict;

    public void Init(){
        dict = new Dictionary<int, AudioClip>();
        foreach (var b in bgms)
            if (!dict.ContainsKey(b.id)) dict.Add(b.id, b.clip);
    }

    public AudioClip GetClip(int id){
        if (dict == null) Init();
        return dict.ContainsKey(id) ? dict[id] : null;
    }
}
