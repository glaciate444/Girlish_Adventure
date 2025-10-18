using UnityEngine;

public class FallOnceLift : BaseFallLift{
    protected override void OnAfterFall(){
        // 完全に消える
        Destroy(gameObject);
    }
}
