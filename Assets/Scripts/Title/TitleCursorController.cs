using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TitleCursorController : MonoBehaviour{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private Vector2 offset = new Vector2(-50f, 0f);

    private GameObject currentSelected;

    private void Update(){
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected != currentSelected)
        {
            currentSelected = selected;
            MoveCursor(selected);
        }
    }

    private void MoveCursor(GameObject target){
        if (cursor == null) return; // 未割り当て対策
        if (!target.TryGetComponent(out RectTransform targetRect)) return;

        var parentRect = cursor.parent as RectTransform;
        if (parentRect == null) return;

        // ワールド→親Rect基準へ変換して、座標ズレを防ぐ
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint)){
            Vector2 targetPos = localPoint + offset;
            DOTween.Kill(cursor);
            cursor.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
        }
    }
}
