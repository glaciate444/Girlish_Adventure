/* =======================================
 * ファイル名 : BootLoader.cs
 * 概要 : ゲーム起動時にPersistentをロードして常駐させるスクリプト
 * Date : 2025/10/29
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour {
    private void Start(){
        // 既にロード済みならスキップ
        if (!SceneManager.GetSceneByName("Persistent").isLoaded){
            SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
        }
    }
}