using UnityEngine;
using System.Collections;

public class FallRespawnLift : BaseFallLift{
    [Header("復活までの時間（秒）")]
    [SerializeField] private float respawnDelay = 5f;

    protected override void OnAfterFall(){
        // 一度落ちた後、リスポーン処理
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine(){
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        yield return new WaitForSeconds(respawnDelay);

        //transform.position = initialPosition;
        //transform.rotation = initialRotation;

        rb.bodyType = RigidbodyType2D.Dynamic;
        isFalling = false;
    }
}
