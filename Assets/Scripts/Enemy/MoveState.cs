using UnityEngine;

/// <summary>
/// 各敵のインスタンスごとに一時的な移動状態を保持する。
/// ScriptableObject共有による副作用を防ぐ。
/// </summary>
[System.Serializable]
public class MoveState{
    public float timer;          // 汎用タイマー
    public float timeOffset;     // Sin波などで使用するオフセット
    public Vector3 lastPosition; // 前フレーム位置など
}
