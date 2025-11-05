/* =======================================
 * ファイル名 : SoundManager.cs
 * 概要 : SoundManagerスクリプト
 * Date : 2025/10/24
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour{
    [SerializeField] private BGMDatabase bgmDB;
    [SerializeField] private SEDatabase seDB;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    private static SoundManager instance;
    public static SoundManager Instance => instance;
    
    private int currentBGMID = -1; // 現在再生中のBGM IDを保持
    private bool isEnsuringAudioSources = false; // 重複実行を防ぐフラグ

    void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (bgmDB == null){
                Debug.LogError("[SoundManager] BGMDatabaseが設定されていません！Inspectorで設定してください。");
            }else{
                Debug.Log($"[SoundManager] BGMDatabase設定確認: {bgmDB.name}");
                Debug.Log($"[SoundManager] BGMDatabase.bgms.Count: {(bgmDB.bgms != null ? bgmDB.bgms.Count : 0)}");
                bgmDB.Init();
                Debug.Log("[SoundManager] BGMDatabase.Init()完了");
            }
            
            if (seDB == null){
                Debug.LogError("[SoundManager] SEDatabaseが設定されていません！Inspectorで設定してください。");
            }else{
                seDB.Init();
            }
            
            if (bgmSource == null){
                Debug.LogError("[SoundManager] BGM AudioSourceが設定されていません！Inspectorで設定してください。");
            }
            
            if (seSource == null){
                Debug.LogError("[SoundManager] SE AudioSourceが設定されていません！Inspectorで設定してください。");
            }
            
            // AudioSourceがNoneの場合は自動検索
            EnsureAudioSources();
            
            Debug.Log("[SoundManager] SoundManagerが初期化されました。");
        }else{
            Debug.LogWarning("[SoundManager] 既にSoundManagerが存在するため、重複インスタンスを破棄します。");
            Destroy(gameObject);
        }
    }

    private void Start(){
        // StartでもAudioSourceを確認（シーンロード後に再設定）
        EnsureAudioSources();
        
        // 定期的にAudioSourceを監視
        StartCoroutine(MonitorAudioSources());
    }

    private void Update(){
        // 実行中にclipがNoneになった場合、再設定
        if (bgmSource != null && bgmSource.clip == null && currentBGMID >= 0){
            Debug.LogWarning($"[SoundManager] Update: bgmSource.clipがNoneになりました。BGM ID {currentBGMID} を再設定します。");
            PlayBGM(currentBGMID);
        }
        
        // AudioSource自体がNoneになった場合、再検索
        if (bgmSource == null || seSource == null){
            EnsureAudioSources();
        }
    }

    // AudioSourceがNoneの場合、自動的に検索して設定
    private void EnsureAudioSources(){
        if (isEnsuringAudioSources) return;
        isEnsuringAudioSources = true;
        
        Debug.Log("[SoundManager] EnsureAudioSources開始");
        Debug.Log($"[SoundManager] 現在のbgmSource: {(bgmSource != null ? bgmSource.name : "null")}");
        Debug.Log($"[SoundManager] 現在のseSource: {(seSource != null ? seSource.name : "null")}");
        
        bool foundAny = false;

        // BGM AudioSourceがNoneの場合、子オブジェクトから検索
        if (bgmSource == null){
            Debug.Log("[SoundManager] BGM AudioSourceがNoneのため、検索を開始");
            
            // まず子オブジェクトから検索
            bgmSource = GetComponentInChildren<AudioSource>();
            Debug.Log($"[SoundManager] GetComponentInChildren結果: {(bgmSource != null ? bgmSource.name : "null")}");
            
            if (bgmSource == null){
                // 子オブジェクトにない場合、シーン内の"BGMObj"を検索
                Debug.Log("[SoundManager] シーン内のBGMObjを検索");
                GameObject bgmObj = GameObject.Find("BGMObj");
                Debug.Log($"[SoundManager] GameObject.Find結果: {(bgmObj != null ? bgmObj.name : "null")}");
                
                if (bgmObj != null){
                    bgmSource = bgmObj.GetComponent<AudioSource>();
                    Debug.Log($"[SoundManager] BGMObjからAudioSource取得: {(bgmSource != null ? bgmSource.name : "null")}");
                }
            }
            
            // まだ見つからない場合、すべてのAudioSourceを検索
            if (bgmSource == null){
                Debug.Log("[SoundManager] すべてのAudioSourceを検索");
                AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
                Debug.Log($"[SoundManager] 見つかったAudioSource数: {allAudioSources.Length}");
                
                foreach (var source in allAudioSources){
                    Debug.Log($"[SoundManager]   - {source.name} (GameObject: {source.gameObject.name})");
                    if (source.name.Contains("BGM") || source.gameObject.name == "BGMObj" || source.gameObject.name.Contains("BGM")){
                        bgmSource = source;
                        Debug.Log($"[SoundManager] BGM AudioSourceとして選択: {source.name}");
                        break;
                    }
                }
            }
            
            if (bgmSource != null){
                Debug.Log($"[SoundManager] BGM AudioSourceを自動検出しました: {bgmSource.name}");
                foundAny = true;
            }else{
                Debug.LogWarning("[SoundManager] BGM AudioSourceが見つかりません。");
            }
        }

        // SE AudioSourceがNoneの場合、子オブジェクトから検索
        if (seSource == null){
            // BGMと別のAudioSourceを探す
            AudioSource[] allSources = GetComponentsInChildren<AudioSource>();
            foreach (var source in allSources){
                if (source != bgmSource){
                    seSource = source;
                    break;
                }
            }
            
            if (seSource == null){
                // 子オブジェクトにない場合、シーン内の"SEObj"を検索
                GameObject seObj = GameObject.Find("SEObj");
                if (seObj != null){
                    seSource = seObj.GetComponent<AudioSource>();
                }
            }
            
            if (seSource != null){
                Debug.Log($"[SoundManager] SE AudioSourceを自動検出しました: {seSource.name}");
                foundAny = true;
            }else{
                Debug.LogWarning("[SoundManager] SE AudioSourceが見つかりません。");
            }
        }

        if (foundAny && currentBGMID >= 0){
            // 以前のBGMが再生されていた場合、再設定
            Debug.Log($"[SoundManager] 以前のBGM ID {currentBGMID} を再設定します。");
            PlayBGM(currentBGMID);
        }
        
        isEnsuringAudioSources = false;
    }

    // AudioSourceを定期的に監視し、Noneになったら再設定（ループ）
    private IEnumerator MonitorAudioSources(){
        while (true){
            yield return new WaitForSeconds(0.5f); // 0.5秒ごとにチェック
            
            // ループで検索を続ける（Noneの場合は強制的に再設定）
            if (bgmSource == null || seSource == null){
                Debug.LogWarning("[SoundManager] AudioSourceがNoneになりました。再設定を試みます。");
                EnsureAudioSources();
                
                // 再設定後もNoneの場合、さらに検索を試みる
                if (bgmSource == null){
                    // すべてのAudioSourceを検索
                    AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true); // 非アクティブも含む
                    foreach (var source in allAudioSources){
                        if (source != null && (source.name.Contains("BGM") || source.gameObject.name == "BGMObj" || source.gameObject.name.Contains("BGM"))){
                            bgmSource = source;
                            // AudioSourceが非アクティブの場合はアクティブ化
                            if (!source.gameObject.activeSelf){
                                source.gameObject.SetActive(true);
                            }
                            Debug.Log($"[SoundManager] BGM AudioSourceを再検出しました: {source.name}");
                            if (currentBGMID >= 0){
                                PlayBGM(currentBGMID);
                            }
                            break;
                        }
                    }
                }
                
                if (seSource == null){
                    // すべてのAudioSourceを検索
                    AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true); // 非アクティブも含む
                    foreach (var source in allAudioSources){
                        if (source != null && (source.name.Contains("SE") || source.gameObject.name == "SEObj" || source.gameObject.name.Contains("SE")) && source != bgmSource){
                            seSource = source;
                            // AudioSourceが非アクティブの場合はアクティブ化
                            if (!source.gameObject.activeSelf){
                                source.gameObject.SetActive(true);
                            }
                            Debug.Log($"[SoundManager] SE AudioSourceを再検出しました: {source.name}");
                            break;
                        }
                    }
                }
            }
            
            // bgmSource.clipがNoneになった場合も再設定
            if (bgmSource != null && bgmSource.clip == null && currentBGMID >= 0){
                Debug.LogWarning($"[SoundManager] MonitorAudioSources: bgmSource.clipがNoneです。BGM ID {currentBGMID} を再設定します。");
                PlayBGM(currentBGMID);
            }
        }
    }

    private void OnDestroy(){
        // 破棄される場合は、instanceをnullに設定（ただし、自分自身がinstanceの場合のみ）
        if (instance == this){
            Debug.LogError("[SoundManager] ⚠️ SoundManagerが破棄されようとしています！破棄を阻止します。");
            
            // 破棄を阻止するため、GameObjectを再アクティブ化
            if (gameObject != null){
                gameObject.SetActive(true);
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SoundManager] SoundManagerを再アクティブ化し、DontDestroyOnLoadを再設定しました。");
            }
            
            // AudioSourceも保護
            if (bgmSource != null && bgmSource.gameObject != null){
                bgmSource.gameObject.SetActive(true);
            }
            if (seSource != null && seSource.gameObject != null){
                seSource.gameObject.SetActive(true);
            }
            
            // Instanceをクリアしない（他のスクリプトが参照している可能性がある）
            return; // 破棄を阻止するため、ここで処理を終了
        }
    }

    // 外部から破棄を防ぐためのメソッド
    public void PreventDestroy(){
        if (instance == this){
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceも保護
            if (bgmSource != null && bgmSource.gameObject != null){
                bgmSource.gameObject.SetActive(true);
            }
            if (seSource != null && seSource.gameObject != null){
                seSource.gameObject.SetActive(true);
            }
            
            Debug.Log("[SoundManager] PreventDestroy()が呼ばれました。SoundManagerを保護します。");
        }
    }

    public void PlayBGM(int id){
        Debug.Log($"[SoundManager] PlayBGM呼び出し: ID={id}");
        
        if (bgmDB == null){
            Debug.LogError("[SoundManager] BGMDatabaseが設定されていません！");
            return;
        }

        // AudioSourceがNoneの場合は自動検索
        if (bgmSource == null){
            Debug.LogWarning("[SoundManager] BGM AudioSourceがNoneです。自動検索を実行します。");
            EnsureAudioSources();
            
            // 再検索してもNoneの場合はエラー
            if (bgmSource == null){
                Debug.LogError("[SoundManager] BGM AudioSourceが見つかりません。");
                return;
            }
        }

        Debug.Log($"[SoundManager] bgmSource確認: {(bgmSource != null ? bgmSource.name : "null")}");

        var clip = bgmDB.GetClip(id);
        if (clip == null){
            Debug.LogWarning($"[SoundManager] BGM ID {id} のクリップが見つかりません。BGMDatabaseを確認してください。");
            Debug.Log($"[SoundManager] BGMDatabaseの状態: bgms.Count={(bgmDB.bgms != null ? bgmDB.bgms.Count : 0)}");
            if (bgmDB.bgms != null){
                foreach (var entry in bgmDB.bgms){
                    Debug.Log($"[SoundManager]   - ID: {entry.id}, Label: {entry.label}, Clip: {(entry.clip != null ? entry.clip.name : "null")}");
                }
            }
            return;
        }

        Debug.Log($"[SoundManager] クリップ取得成功: {clip.name}");

        // 同じ曲なら再生し直さない
        if (bgmSource.clip == clip && bgmSource.isPlaying){
            Debug.Log($"[SoundManager] BGM ID {id} は既に再生中です。");
            return;
        }

        // BGM IDを保存
        currentBGMID = id;

        Debug.Log($"[SoundManager] bgmSource.clipを設定: {clip.name}");
        bgmSource.clip = clip;
        
        Debug.Log($"[SoundManager] bgmSource.Play()を呼び出し");
        bgmSource.Play();
        
        Debug.Log($"[SoundManager] BGM ID {id} を再生しました: {clip.name}, 現在のclip: {(bgmSource.clip != null ? bgmSource.clip.name : "null")}, isPlaying: {bgmSource.isPlaying}");
    }

    public void StopBGM(){
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void PlaySE(int id){
        if (seDB == null){
            Debug.LogError("[SoundManager] SEDatabaseが設定されていません！");
            return;
        }

        // AudioSourceがNoneの場合は自動検索
        if (seSource == null){
            Debug.LogWarning("[SoundManager] SE AudioSourceがNoneです。自動検索を実行します。");
            EnsureAudioSources();
            
            // 再検索してもNoneの場合はエラー
            if (seSource == null){
                Debug.LogError("[SoundManager] SE AudioSourceが見つかりません。");
                return;
            }
        }

        var clip = seDB.GetClip(id);
        if (clip == null){
            Debug.LogWarning($"[SoundManager] SE ID {id} のクリップが見つかりません。SEDatabaseを確認してください。");
            return;
        }
        
        seSource.PlayOneShot(clip);
    }
}