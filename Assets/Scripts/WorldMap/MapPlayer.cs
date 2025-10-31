/* =======================================
 * ファイル名 : MapPlayer.cs
 * 概要 : プレイヤーのノード間移動スクリプト
 * Created Date : 2025/10/30
 * Date : 2025/10/30
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class MapPlayer : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private MapNode startNode;

    private MapNode currentNode;
    private MapControls controls;
    private bool isMoving;

    private void Awake(){
        controls = new MapControls();
    }

    private void OnEnable(){
        controls.Enable();
        controls.Map.Submit.performed += OnSubmit;
    }

    private void OnDisable(){
        controls.Map.Submit.performed -= OnSubmit;
        controls.Disable();
    }

    private void Start(){
        currentNode = startNode;
        transform.position = currentNode.transform.position;
    }

    private void Update(){
        if (isMoving) return;

        Vector2 moveInput = controls.Map.Move.ReadValue<Vector2>();
        Vector2Int inputDir = Vector2Int.zero;

        if (moveInput.y > 0.5f) inputDir = Vector2Int.up;
        else if (moveInput.y < -0.5f) inputDir = Vector2Int.down;
        else if (moveInput.x < -0.5f) inputDir = Vector2Int.left;
        else if (moveInput.x > 0.5f) inputDir = Vector2Int.right;

        if (inputDir != Vector2Int.zero){
            MapNode next = currentNode.GetNeighbor(inputDir);
            if (next != null && next.Unlocked)
                StartCoroutine(MoveToNode(next));
        }
    }

    private void OnSubmit(InputAction.CallbackContext ctx){
        if (currentNode.IsStage)
            SceneManager.LoadScene(currentNode.SceneName);
    }

    private IEnumerator MoveToNode(MapNode target){
        isMoving = true;
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;
        float t = 0f;

        while (t < 1f){
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        currentNode = target;
        isMoving = false;
    }
}

/* ==================================
 * グリッド（ノード）間を線形補間でスムーズに移動。
 * 各ノードが接続する方向だけに進める。
 * ノードがロック中なら進入不可。
 * ==================================
 */