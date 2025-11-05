/* =======================================
 * ファイル名 : DialogueManager.cs
 * 概要 : 実行・ページ制御スクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.02
 * 更新内容 : タイプ音付きバージョン
 * ======================================= */
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIcon;

    [Header("オプション")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();
    private bool isTyping = false;
    private GameManager gameManager;

    private Action onDialogueEnd; // 会話終了後のイベント用

    void Awake(){
        gameManager = FindObjectOfType<GameManager>();
        nextIcon.SetActive(false);
    }

    public void StartDialogue(DialogueData data, Action onEndEvent = null){
        // 会話イベントを登録
        onDialogueEnd = onEndEvent;

        if (gameManager != null)
            gameManager.SetGamePaused(true);

        dialogueLines.Clear();
        foreach (var line in data.lines)
            dialogueLines.Enqueue(line);

        DisplayNextLine();
    }

    public void DisplayNextLine(){
        if (isTyping) return;

        if (dialogueLines.Count == 0){
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();
        nameText.text = line.characterName;
        StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string line){
        isTyping = true;
        dialogueText.text = "";
        nextIcon.SetActive(false);

        foreach (char c in line.ToCharArray()){
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        nextIcon.SetActive(true);
    }

    private void EndDialogue(){
        dialogueText.text = "";
        nameText.text = "";
        nextIcon.SetActive(false);

        if (gameManager != null)
            gameManager.SetGamePaused(false);

        onDialogueEnd?.Invoke();
        onDialogueEnd = null;
    }
}
