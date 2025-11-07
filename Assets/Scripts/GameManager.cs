/* =======================================
 * ファイル名 : GameManager.cs
 * 概要 : ゲームメインスクリプト
 * Create Date : 2025/10/01
 * Date : 2025/11/07
 * Version : 0.05
 * 更新内容 : セーブデータ対応
 * ======================================= */
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    private UIManager uiManager;
    public int CurrentStage { get; private set; }
    private bool isEnsuringSoundManager = false; // 重複実行を防ぐフラグ
    private bool isEnsuringUIManager = false; // UIManager読み込み中のフラグ
    public PlayerData playerData; // ScriptableObject で現在の状態を持つ
    public int currentSlot = 1;
    public SoundManager SoundManager => SoundManager.Instance;
    public UIManager UI => uiManager;

    private void Awake(){
        Debug.Log($"[GameManager] Awake()呼び出し: Instance={(Instance != null ? Instance.name : "null")}, this={name}, active={gameObject.activeSelf}, enabled={enabled}");
        
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] ✅ GameManager初期化完了。DontDestroyOnLoad設定。Instance設定完了。");
            StartCoroutine(EnsurePersistentLoaded());
            StartCoroutine(MonitorSoundManager()); // 定期的な監視を開始
            StartCoroutine(MonitorUIManager()); // UIManagerの監視を開始
        }else if (Instance != this){
            Debug.LogWarning($"[GameManager] 既にGameManagerが存在するため、重複インスタンスを破棄します。既存: {Instance.name}, 新規: {name}");
            Destroy(gameObject);
        }else{
            Debug.LogWarning("[GameManager] 既に自分自身がInstanceとして設定されています。");
        }
    }

    private void OnDestroy(){
        // 自分自身がInstanceの場合のみ警告（重複インスタンスの破棄は正常）
        if (Instance == this){
            Debug.LogError("[GameManager] ⚠️ GameManagerが破棄されようとしています！破棄を阻止します。");
            
            // 破棄を阻止するため、GameObjectを再アクティブ化
            if (gameObject != null){
                gameObject.SetActive(true);
                DontDestroyOnLoad(gameObject);
                Debug.Log("[GameManager] GameManagerを再アクティブ化し、DontDestroyOnLoadを再設定しました。");
            }
            
            // 破棄を防ぐため、Instanceをクリアしない（他のスクリプトが参照している可能性がある）
            // 注意: OnDestroy内で破棄を完全に阻止することはできないため、
            // 呼び出し元で破棄を防ぐ必要があります
        }
    }

    // 外部から破棄を防ぐためのメソッド
    public void PreventDestroy(){
        if (Instance == this){
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] PreventDestroy()が呼ばれました。GameManagerを保護します。");
        }
    }

    // 外部からInstanceを強制的に設定するメソッド（緊急時のみ使用）
    public void ForceSetInstance(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] ⚠️ ForceSetInstance()が呼ばれました。Instanceを強制的に設定しました。");
        }else if (Instance != this){
            Debug.LogWarning($"[GameManager] ForceSetInstance()呼び出し: 既に別のInstanceが存在します。既存: {Instance.name}, 現在: {name}");
        }
    }

    private IEnumerator EnsurePersistentLoaded(){
        // 重複実行を防ぐ
        if (isEnsuringSoundManager) yield break;
        isEnsuringSoundManager = true;

        // PersistentがロードされていなければAdditiveロード
        if (!SceneManager.GetSceneByName("Persistent").isLoaded){
            Debug.Log("[GameManager] Persistentシーンが未ロードのため、強制ロードを実行します。");
            var async = SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
            while (!async.isDone) yield return null;
            Debug.Log("[GameManager] Persistentシーンのロード完了");
        }

        // SoundManagerが現れるまで探す（SoundManager.Instanceが設定されるまで待つ）
        float timeout = 5f;
        while (SoundManager.Instance == null && timeout > 0f){
            // FindObjectOfTypeで直接検索も試行
            SoundManager found = FindObjectOfType<SoundManager>();
            if (found != null && SoundManager.Instance == null){
                Debug.LogWarning("[GameManager] SoundManager.Instanceがnullですが、オブジェクトは見つかりました。再初期化を待機します。");
            }
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (SoundManager.Instance == null){
            Debug.LogError("❌ SoundManagerが見つかりません。Persistentシーンに正しく配置されているか確認してください。");
        }else{
            Debug.Log($"✅ SoundManagerを検出しました -> {SoundManager.Instance.name}");
        }

        isEnsuringSoundManager = false;
    }

    // SoundManagerを定期的に監視し、nullになったら強制読み込み
    private IEnumerator MonitorSoundManager(){
        while (true){
            yield return new WaitForSeconds(1f); // 1秒ごとにチェック
            
            if (SoundManager.Instance == null){
                Debug.LogWarning("[GameManager] SoundManagerがnullになりました。強制再読み込みを実行します。");
                StartCoroutine(EnsurePersistentLoaded());
                
                // 再読み込みが完了するまで待機
                float waitTimeout = 5f;
                while (SoundManager.Instance == null && waitTimeout > 0f){
                    waitTimeout -= Time.unscaledDeltaTime;
                    yield return null;
                }
                
                if (SoundManager.Instance != null){
                    Debug.Log("[GameManager] SoundManagerの再読み込みに成功しました。");
                    // 破棄を防ぐ
                    SoundManager.Instance.PreventDestroy();
                }
            }else{
                // SoundManagerが存在する場合、破棄を防ぐ
                SoundManager.Instance.PreventDestroy();
            }
        }
    }

    // UIManagerを定期的に監視し、破棄を防ぐ
    private IEnumerator MonitorUIManager(){
        while (true){
            yield return new WaitForSeconds(1f); // 1秒ごとにチェック
            
            // UIManager.Instanceを優先的に使用
            if (UIManager.Instance != null && UIManager.Instance.gameObject != null){
                // 既存のインスタンスを登録（まだ登録されていない場合）
                if (uiManager != UIManager.Instance){
                    RegisterUI(UIManager.Instance);
                }
                // 破棄を防ぐ
                UIManager.Instance.PreventDestroy();
            }else{
                // UIManager.Instanceが見つからない場合は警告のみ（新規生成は行わない）
                if (uiManager == null){
                    Debug.LogWarning("[GameManager] UIManagerが見つかりません。シーンに配置されているか確認してください。");
                }
            }
        }
    }

    // UIManagerを強制的に読み込む（既存のインスタンスのみ検索、新規生成は行わない）
    private IEnumerator ForceLoadUIManager(){
        if (isEnsuringUIManager) yield break;
        isEnsuringUIManager = true;
        
        float timeout = 5f;
        
        while ((uiManager == null || uiManager.gameObject == null) && timeout > 0f){
            timeout -= Time.unscaledDeltaTime;
            
            // UIManager.Instanceを確認
            if (UIManager.Instance != null && UIManager.Instance.gameObject != null){
                Debug.Log($"[GameManager] UIManager.Instanceを発見: {UIManager.Instance.name}");
                UIManager.Instance.gameObject.SetActive(true);
                UIManager.Instance.PreventDestroy();
                RegisterUI(UIManager.Instance);
                if (uiManager != null){
                    Debug.Log("[GameManager] UIManager.Instanceの再登録に成功しました。");
                    isEnsuringUIManager = false;
                    yield break;
                }
            }
            
            yield return null;
        }
        
        // 既存のインスタンスが見つからない場合は警告のみ（新規生成は行わない）
        if (uiManager == null){
            Debug.LogError("[GameManager] ⚠️ UIManagerが見つかりません。シーンに配置されているか確認してください。");
        }
        
        isEnsuringUIManager = false;
    }

    public void SetGamePaused(bool isPaused){
        // タイムスケールを止めて物理・アニメを停止
        Time.timeScale = isPaused ? 0f : 1f;

        // プレイヤー操作を禁止したい場合
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = !isPaused;

        Debug.Log($"Game Paused: {isPaused}");
    }

    // ===== UI関連 =====

    public void RegisterUI(UIManager ui){
        if (ui == null){
            Debug.LogWarning("[GameManager] RegisterUI: uiがnullです。");
            return;
        }
        
        // 破棄を防ぐ
        ui.PreventDestroy();
        
        // 既に別のUIManagerが登録されている場合は警告
        if (uiManager != null && uiManager != ui){
            Debug.LogWarning($"[GameManager] UIManagerが既に登録されています。古い: {uiManager.name}, 新しい: {ui.name}");
        }
        
        uiManager = ui;
        Debug.Log($"[GameManager] UIManagerを登録しました: {ui.name}");
    }

    // UIManagerは破棄されないため、登録解除は行わない
    // 必要に応じて、参照のみクリア（オブジェクトは破棄しない）
    public void UnregisterUI(){
        // 参照のみクリア（UIManagerオブジェクト自体は破棄しない）
        if (uiManager != null){
            Debug.Log($"[GameManager] UIManagerの参照をクリアしました: {uiManager.name}（オブジェクトは破棄しません）");
            uiManager = null;
        }
    }

    // ===== シーン遷移 =====

    public void LoadStage(int stageNumber){
        CurrentStage = stageNumber;
        SceneManager.LoadSceneAsync($"Stage_{stageNumber}");
    }

    public void ReturnToWorldMap(){
        SceneManager.LoadSceneAsync("WorldMap");
    }

    public void GoToTitle(){
        SceneManager.LoadSceneAsync("Title");
    }
    public void GoResult(string resultSceneName){
        SceneManager.LoadSceneAsync(resultSceneName);
    }



    // ===== サウンド操作 =====

    public void PlayBGM(int bgmID){
        // 破棄されないように保護
        if (Instance == this){
            PreventDestroy();
        }
        
        Debug.Log($"[GameManager] PlayBGM呼び出し: ID={bgmID}, Instance={(Instance != null ? Instance.name : "null")}, SoundManager.Instance={(SoundManager.Instance != null ? "存在" : "null")}");
        
        // Instanceがnullの場合は強制的に設定
        if (Instance == null && this != null){
            Debug.LogError("[GameManager] ⚠️ PlayBGM呼び出し時にInstanceがnullです。強制的に設定します。");
            ForceSetInstance();
        }
        
        if (SoundManager.Instance != null){
            SoundManager.Instance.PlayBGM(bgmID);
        }else{
            Debug.LogWarning("⚠️ SoundManager未設定。Persistentロードを強制実行します。");
            StartCoroutine(EnsureSoundManagerAndPlayBGM(bgmID));
        }
    }

    public void PlaySE(int seID){
        if (SoundManager.Instance != null){
            SoundManager.Instance.PlaySE(seID);
        }else{
            Debug.LogWarning("⚠️ SoundManager未設定。Persistentロードを強制実行します。");
            StartCoroutine(EnsureSoundManagerAndPlaySE(seID));
        }
    }

    public void StopBGM(){
        if (SoundManager.Instance != null){
            SoundManager.Instance.StopBGM();
        }else{
            Debug.LogWarning("⚠️ SoundManager未設定。StopBGMをスキップします。");
        }
    }

    // SoundManagerを確保してからBGMを再生
    private IEnumerator EnsureSoundManagerAndPlayBGM(int bgmID){
        yield return StartCoroutine(EnsurePersistentLoaded());
        
        if (SoundManager.Instance != null){
            SoundManager.Instance.PlayBGM(bgmID);
        }else{
            Debug.LogError("[GameManager] SoundManagerの確保に失敗しました。BGMを再生できません。");
        }
    }

    // SoundManagerを確保してからSEを再生
    private IEnumerator EnsureSoundManagerAndPlaySE(int seID){
        yield return StartCoroutine(EnsurePersistentLoaded());
        
        if (SoundManager.Instance != null){
            SoundManager.Instance.PlaySE(seID);
        }else{
            Debug.LogError("[GameManager] SoundManagerの確保に失敗しました。SEを再生できません。");
        }
    }

    // ====== セーブデータ ============

    public void SaveGame(){
        var slot = new SaveSlot{
            slotNumber = currentSlot,
            stageProgress = 0, // まだ進行度がない場合
            player = new PlayerSaveData{
                // levelがないので削除
                hp = playerData.hp,
                sp = playerData.sp,
                coin = playerData.coin,
                // inventoryがない場合、nullでもOK
                inventory = new string[0]
            },
            settings = new GameSettings()
        };

        SaveData.Save(currentSlot, slot);
    }

    public void LoadGame(){
        if (!SaveData.Exists(currentSlot)){
            Debug.LogWarning("No save data found.");
            return;
        }

        var slot = SaveData.Load(currentSlot);

        playerData.hp = slot.player.hp;
        playerData.sp = slot.player.sp;
        playerData.coin = slot.player.coin;

        // inventoryがまだないならスキップ
        playerData.SyncUI();
    }
}
