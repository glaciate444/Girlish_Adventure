using UnityEngine;

public class BackGrounds : MonoBehaviour{
    [SerializeField, Header("éãç∑åç∑"), Range(0, 1)]
    private float parallaxEffect;

    private GameObject cameraObj;
    private float length;
    private float startPosX;

    private void Start(){
        startPosX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        Debug.Log($"startPosX -> {startPosX} | length -> {length}");
        cameraObj = Camera.main.gameObject;
    }
    private void FixedUpdate(){
        Parallax();
    }

    private void Parallax(){
        float temp = cameraObj.transform.position.x * (1 - parallaxEffect);
        float dist = cameraObj.transform.position.x * parallaxEffect;

        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        if(temp > startPosX + length){
            startPosX += length;
        }else if (temp < startPosX -length){
            startPosX -= length;
        }
    }
}
