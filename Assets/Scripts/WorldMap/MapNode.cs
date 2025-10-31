/* =======================================
 * ファイル名 : MapNode.cs
 * 概要 : ノードの定義スクリプト
 * Created Date : 2025/10/30
 * Date : 2025/10/30
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class MapNode : MonoBehaviour {
    [SerializeField] private string sceneName;
    [SerializeField] private bool isStage;
    [SerializeField] private MapNode up;
    [SerializeField] private MapNode down;
    [SerializeField] private MapNode left;
    [SerializeField] private MapNode right;
    [SerializeField] private bool unlocked = true;

    public string SceneName => sceneName;
    public bool IsStage => isStage;
    public bool Unlocked => unlocked;

    public MapNode GetNeighbor(Vector2Int direction){
        return direction switch{
            Vector2Int v when v == Vector2Int.up => up,
            Vector2Int v when v == Vector2Int.down => down,
            Vector2Int v when v == Vector2Int.left => left,
            Vector2Int v when v == Vector2Int.right => right,
            _ => null
        };
    }

    public void Unlock() => unlocked = true;
}
/*===================
 * ノードは上下左右の接続先を設定できる。
 * isStage が true の場合、そのノードはコース入口。
 * unlocked によりルート解放を管理。
 * ================ */