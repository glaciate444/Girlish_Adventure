/* =======================================
 * ファイル名 : CoursePoint.cs
 * 概要 : コースポイントスクリプト
 * Created Date : 2025/10/30
 * Date : 2025/10/30
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoursePoint : MonoBehaviour {
    [SerializeField] private string sceneName;
    [SerializeField] private KeyCode enterKey = KeyCode.Return;

    private bool isPlayerNear;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Player")) isPlayerNear = false;
    }

    private void Update(){
        if (isPlayerNear && Input.GetKeyDown(enterKey)){
            SceneManager.LoadScene(sceneName);
        }
    }
}
