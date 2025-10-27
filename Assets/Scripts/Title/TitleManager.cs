/* =======================================
 * ファイル名 : TitleManager.cs
 * 概要 : タイトル画面スクリプト
 * Date : 2025/10/24
 * Version : 0.01
 * ======================================= */
// TitleManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour{
    [SerializeField] private TitleMenuFadeController fadeController;
    [SerializeField] private InputActionReference anyKeyAction;

    private void OnEnable(){
        anyKeyAction.action.performed += OnAnyKey;
        anyKeyAction.action.Enable();
    }

    private void OnDisable(){
        anyKeyAction.action.performed -= OnAnyKey;
        anyKeyAction.action.Disable();
    }

    private void OnAnyKey(InputAction.CallbackContext ctx){
        fadeController.OpenMenu();
    }
}