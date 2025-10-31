/* =======================================
 * ファイル名 : WorldMapInitializer.cs
 * 概要 : ゲームメインスクリプト
 * Create Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * 更新内容 : Presistent対応
 * ======================================= */
using UnityEngine;

public class WorldMapInitializer : MonoBehaviour {
    private void Start(){
        var fader = FindAnyObjectByType<SceneFader>();
        if (fader != null)
            StartCoroutine(fader.FadeIn());
    }
}