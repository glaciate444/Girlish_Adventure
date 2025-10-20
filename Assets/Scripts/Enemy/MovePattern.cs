/* =======================================
 * ファイル名 : MovePattern.cs
 * 概要 : 敵データ列挙体
 * Date : 2025/10/21
 * ======================================= */

public enum MovePattern{
    GroundPatrol, // 地上往復
    FlySin,       // 上下ふわふわ
    Chase,        // プレイヤー追尾
    Jump          // ジャンプ移動
}
