/* =======================================
 * ファイル名 : SoundManager.cs
 * 概要 : SoundManagerスクリプト
 * Date : 2025/10/24
 * Version : 0.01
 * ======================================= */
using UnityEngine;

public class SoundManager : MonoBehaviour{
    [SerializeField] private BGMDatabase bgmDB;
    [SerializeField] private SEDatabase seDB;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    private static SoundManager instance;
    public static SoundManager Instance => instance;

    void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
            bgmDB.Init();
            seDB.Init();
        }else{
            Destroy(gameObject);
        }
    }

    public void PlayBGM(int id){
        var clip = bgmDB.GetClip(id);
        if (clip == null) return;

        // 同じ曲なら再生し直さない
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void PlaySE(int id){
        var clip = seDB.GetClip(id);
        if (clip == null) return;
        seSource.PlayOneShot(clip);
    }
}