/*==========================================
 * ゴール処理
 * Ver1.00
 * Create Date : 2025/10/19
 *=========================================
 */
using UnityEngine;

public class GoalFlag : MonoBehaviour{
    private void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player")){
            Debug.Log("GOAL!");
        }
    }

}
