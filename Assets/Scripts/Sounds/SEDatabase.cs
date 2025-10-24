/* =======================================
 * ファイル名 : SEDatabase.cs
 * 概要 : 効果音データベーススクリプト
 * Date : 2025/10/24
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SEEntry{
    public int id;          // 例: 3 = CursorMove
    public string label;    // "CursorMove"
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "SEDatabase", menuName = "Sound/SEDatabase")]
public class SEDatabase : ScriptableObject{
    public List<SEEntry> ses;
    private Dictionary<int, AudioClip> dict;

    public void Init(){
        dict = new Dictionary<int, AudioClip>();
        foreach (var s in ses)
            if (!dict.ContainsKey(s.id)) dict.Add(s.id, s.clip);
    }

    public AudioClip GetClip(int id){
        if (dict == null) Init();
        return dict.ContainsKey(id) ? dict[id] : null;
    }
}