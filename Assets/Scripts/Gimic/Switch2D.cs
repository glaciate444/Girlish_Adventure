using UnityEngine;
using UnityEngine.Events;

public class Switch2D : MonoBehaviour
{
    [SerializeField] private bool isToggle = true;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;

    private bool isOn;
    private SpriteRenderer sr;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    private void OnTriggerEnter2D(Collider2D collision){
        if (!collision.CompareTag("Player")) return;
        SetState(true);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (!collision.CompareTag("Player") || !isToggle) return;
        SetState(false);
    }

    private void SetState(bool state){
        if (isOn == state) return;
        isOn = state;
        sr.sprite = isOn ? onSprite : offSprite;
        if (isOn) onActivated?.Invoke();
        else onDeactivated?.Invoke();
    }
}
/*
 * UnityEventを使えば、スイッチに連動するギミックをインスペクターで簡単に設定可能です（例：ドア開閉、足場出現など）。
 */