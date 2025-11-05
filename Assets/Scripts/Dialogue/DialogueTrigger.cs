/* =======================================
 * ファイル名 : DialogueTrigger.cs
 * 概要 : ダイアログトリガースクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private MonoBehaviour eventScript; // 会話後に実行したいスクリプト
    [SerializeField] private string eventMethodName = "OnDialogueEnd"; // 実行メソッド名

    [SerializeField] private DialogueData dialogue;
    [SerializeField] private MonoBehaviour onEndScript; // 終了後に実行したいスクリプト

    private DialogueManager dialogueManager;

    void Start(){
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    public void TriggerDialogue(){
        if (dialogueManager != null){
            dialogueManager.StartDialogue(dialogue, OnDialogueEnd);
        }
    }

    private void OnDialogueEnd(){
        if (onEndScript != null){
            var method = onEndScript.GetType().GetMethod("OnDialogueComplete");
            if (method != null)
                method.Invoke(onEndScript, null);
        }
    }
}
