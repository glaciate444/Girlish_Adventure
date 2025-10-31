/* =======================================
 * ファイル名 : MapProgress.cs
 * 概要 : ステージ解放システムスクリプト
 * Created Date : 2025/10/30
 * Date : 2025/10/30
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class MapProgress : MonoBehaviour {
    public static void UnlockNode(MapNode node){
        node.Unlock();
        PlayerPrefs.SetInt(node.name, 1);
    }

    public static bool IsNodeUnlocked(string nodeName){
        return PlayerPrefs.GetInt(nodeName, 0) == 1;
    }
}
/*
 * StageSelectScene
 * ├── MapPlayer
 * ├── Nodes
 * │   ├── Node_1 (isStage = true, sceneName = "Course1")
 * │   ├── Node_2 (isStage = true, sceneName = "Course2")
 * │   └── Node_3 (分岐点)
 * └── MapCameraFollow（または固定カメラ）
 */