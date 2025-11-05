/* =======================================
 * ファイル名 : DialogueManager.cs
 * 概要 : TextMeshPro制御スクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour {
    [SerializeField] private GameObject window;
    [SerializeField] private TextMeshProUGUI textMeshPro;

    public void ShowWindow(){
        window.SetActive(true);
    }

    public void HideWindow(){
        window.SetActive(false);
    }

    public void ClearText(){
        textMeshPro.text = "";
    }

    public void AppendText(string value){
        textMeshPro.text += value;
    }
}
