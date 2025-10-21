/* =======================================
 * ファイル名 : MoveBehaviorSO.cs
 * 概要 : 移動用ScriptableObject
 * Date : 2025/10/21
 * ======================================= */
using UnityEngine;

/// <summary>
/// 移動ロジックの ScriptableObject 基底クラス
/// CreateState を持たせて、敵ごとの動的状態（MoveState）を生成できるようにする
/// </summary>
public abstract class MoveBehaviorSO : ScriptableObject {
    /// <summary>
    /// デフォルトの状態生成。必要なら派生クラスでオーバーライドして初期値を入れる。
    /// </summary>
    public virtual MoveState CreateState(){
        return new MoveState();
    }

    /// <summary>
    /// 初期化（敵生成時に 1 回呼ばれる）
    /// </summary>
    public virtual void Initialize(BaseEnemy enemy, MoveState state) { }

    /// <summary>
    /// 毎FixedUpdateで呼ばれる移動処理（必須オーバーライド）
    /// </summary>
    public abstract void Move(BaseEnemy enemy, MoveState state);
}