using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderZone : MonoBehaviour{
    private void Reset(){
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }
}
