/* =======================================
 * ファイル名 : DialogueWindow.cs
 * 概要 : ダイアログスクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DialogueWindow : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIcon;
    [SerializeField] private float typeSpeed = 0.02f;

    private DialogueData currentData;
    private int currentIndex;
    private bool isTyping;
    private bool waitingForNext;
    private int linesPerPage = 3; // ←★ 1ページ3行表示

    public void StartDialogue(DialogueData data){
        currentData = data;
        currentIndex = 0;
        ShowNextPage();
    }

    private void ShowNextPage(){
        if (currentData == null) return;

        // ページに含める行をまとめる
        string combinedText = "";
        int linesThisPage = 0;

        while (currentIndex < currentData.lines.Length && linesThisPage < linesPerPage){
            var line = currentData.lines[currentIndex];
            if (linesThisPage == 0)
                nameText.text = line.characterName; // 先頭のキャラ名をセット

            combinedText += line.text + "\n";
            currentIndex++;
            linesThisPage++;
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(combinedText.TrimEnd()));
    }

    private IEnumerator TypeText(string text){
        dialogueText.text = "";
        isTyping = true;
        waitingForNext = false;

        foreach (char c in text){
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        waitingForNext = true;
        nextIcon.SetActive(true);
    }

    private void Update(){
        if (waitingForNext && Input.GetButtonDown("Submit")){
            nextIcon.SetActive(false);
            waitingForNext = false;

            if (currentIndex >= currentData.lines.Length){
                EndDialogue();
            }else{
                ShowNextPage();
            }
        }
    }

    private void EndDialogue(){
        dialogueText.text = "";
        nameText.text = "";
        gameObject.SetActive(false);
    }
}
