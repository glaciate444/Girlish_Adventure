/* =======================================
 * ファイル名 : TitleCursorController.cs
 * 概要 : タイトル画面カーソルスクリプト
 * Created Date : 2025/10/28
 * Date : 2025/10/28
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TitleCursorController : MonoBehaviour {
    [SerializeField] private RectTransform cursor;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private Vector2 offset = new Vector2(-50f, 0f);

    private GameObject currentSelected;
    private CanvasGroup currentActiveGroup;

    public void SetActiveGroup(CanvasGroup group){
        currentActiveGroup = group;
        UpdateCursorInstant(EventSystem.current.currentSelectedGameObject);
    }

    private void Update(){
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected != currentSelected)
            Debug.Log($"[Cursor] Selected: {selected.name}");
        else if (selected == null)
            Debug.Log("[Cursor] Selected: null");

        if (currentActiveGroup == null){
            Debug.Log("[Cursor] currentActiveGroup is null!");
            return;
        }

        if (!currentActiveGroup.interactable){
            Debug.Log("[Cursor] currentActiveGroup not interactable!");
            return;
        }

        if (selected == null || selected == currentSelected) return;
        if (selected.transform.IsChildOf(currentActiveGroup.transform)){
            currentSelected = selected;
            MoveCursor(selected);
        }
    }

    private void MoveCursor(GameObject target){
        if (cursor == null || !target.TryGetComponent(out RectTransform targetRect)) return;

        var parentRect = cursor.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint)){
            Vector2 targetPos = localPoint + offset;
            DOTween.Kill(cursor);
            cursor.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
        }
    }

    private void UpdateCursorInstant(GameObject target){
        if (cursor == null || target == null || !target.TryGetComponent(out RectTransform targetRect)) return;
        var parentRect = cursor.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint)){
            cursor.anchoredPosition = localPoint + offset;
        }
    }
}