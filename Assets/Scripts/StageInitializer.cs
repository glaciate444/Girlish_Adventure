/* =======================================
 * ファイル名 : StageInitializer.cs
 * 概要 : ステージ初期設定スクリプト
 * Date : 2025/10/29
 * Version : 0.01
 * ======================================= */
using UnityEngine;

public class StageInitializer : MonoBehaviour{
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private int bgmID = -1;

    // GameManagerからUIManager Prefabを取得するためのメソッド
    public GameObject GetUIManagerPrefab() => uiManagerPrefab;

    // Resourcesから有効なUIManagerプレハブを探索（UIManager付き & 子が存在）。候補をログし、子数が最大のものを採用
    private GameObject FindValidUIManagerPrefabInResources(){
        // まず既定のパスを優先
        GameObject candidate = Resources.Load<GameObject>("Prefab/UIManager");
        if (candidate != null){
            var comp = candidate.GetComponent<UIManager>();
            Debug.Log($"[StageInitializer] Load Prefab/UIManager -> hasUIManager={(comp!=null)}, childCount={candidate.transform.childCount}");
            if (comp != null && candidate.transform.childCount > 0)
                return candidate;
        }

        // 総当たりで最良候補を選択
        var all = Resources.LoadAll<GameObject>("");
        GameObject best = null;
        int bestChildren = -1;
        foreach (var go in all){
            if (go == null) continue;
            var comp = go.GetComponent<UIManager>();
            int cc = go.transform != null ? go.transform.childCount : 0;
            Debug.Log($"[StageInitializer] Resource candidate: {go.name}, hasUIManager={(comp!=null)}, childCount={cc}");
            if (comp != null && cc > bestChildren){
                best = go;
                bestChildren = cc;
            }
        }
        if (best != null && bestChildren > 0){
            Debug.Log($"[StageInitializer] Use candidate: {best.name} (childCount={bestChildren})");
            return best;
        }
        return null;
    }

    private void LogHierarchy(Transform root){
        if (root == null){ Debug.Log("[StageInitializer] LogHierarchy: root=null"); return; }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[StageInitializer] Children of {root.name} (count={root.childCount})\n");
        for (int i=0;i<root.childCount;i++){
            var c = root.GetChild(i);
            sb.Append("- ").Append(c.name).Append("\n");
        }
        Debug.Log(sb.ToString());
    }

    // UIの子が存在しない場合はCanvas系を補構築し、参照を自動割り当て
    // 注意: UIManager自体は破棄しない。子オブジェクトのみ再生成する。
    private void EnsureUIChildren(UIManager ui){
        if (ui == null || ui.gameObject == null) return;
        
        try{
            // 子オブジェクトが存在しない場合のみ再生成
            if (ui.transform.childCount == 0){
                Debug.LogWarning("[StageInitializer] UIManagerの子オブジェクトが存在しません。子オブジェクトのみ再生成します。");
                var canvasPrefab = Resources.Load<GameObject>("Prefab/Canvas");
                if (canvasPrefab != null){
                    var canvasInst = Instantiate(canvasPrefab, ui.transform);
                    canvasInst.name = "Canvas"; // 安定名
                    Debug.Log("[StageInitializer] ✅ Canvasプレハブを子として再構築しました。");
                }else{
                    Debug.LogError("[StageInitializer] Resources/Prefab/Canvas が見つかりません。UIの再構築に失敗します。");
                }
            }
            
            // 参照の自動割り当てと保護
            //ui.AutoAssign();
            ui.PreventDestroy();
        }catch (System.Exception e){
            Debug.LogError($"[StageInitializer] EnsureUIChildren中にエラー: {e.Message}");
        }
    }

    private void Start(){
        StartCoroutine(InitRoutine());
    }

    private System.Collections.IEnumerator InitRoutine(){
        if (uiManagerPrefab == null){
            Debug.LogError("UIManager Prefab が設定されていません。");
            yield break;
        }

        // 既存のUIManagerを優先的に使用し、無ければ生成
        GameObject instance = null;
        UIManager ui = null;

        // まずResourcesから有効なUIManagerプレハブを優先的に取得（Inspector設定ミスを無視）
        GameObject preferredPrefab = FindValidUIManagerPrefabInResources();

        // 既存のUIManager.Instanceを最優先で使用（新規生成は行わない）
        if (UIManager.Instance != null && UIManager.Instance.gameObject != null){
            Debug.Log($"[StageInitializer] ✅ 既にUIManager.Instanceが存在します: {UIManager.Instance.name}。既存インスタンスを使用します。");
            try{
                if (!UIManager.Instance.gameObject.activeSelf){
                    UIManager.Instance.gameObject.SetActive(true);
                }
                UIManager.Instance.PreventDestroy();
                instance = UIManager.Instance.gameObject;
                ui = UIManager.Instance;
            }catch (System.Exception e){
                Debug.LogError($"[StageInitializer] 既存UIManagerのアクティブ化に失敗: {e.Message}");
                // エラーが発生した場合は、新規生成を試みる（最後の手段）
            }
        }
        
        // 既存のインスタンスが見つからない場合のみ新規生成（通常は発生しない）
        if (ui == null || instance == null){
            Debug.LogWarning("[StageInitializer] ⚠️ UIManager.Instanceが見つかりません。新規生成を試みます（通常は発生しません）。");
            // 親指定なしで完全なルートとして生成
            var sourcePrefab = preferredPrefab != null ? preferredPrefab : uiManagerPrefab;
            if (sourcePrefab == null){
                Debug.LogError("[StageInitializer] UIManagerの生成元プレハブが見つかりません（ResourcesにもInspectorにもありません）。");
                yield break;
            }
            instance = Instantiate(sourcePrefab);
            ui = instance.GetComponent<UIManager>();
            Debug.Log($"[StageInitializer] Instantiate: {sourcePrefab.name}, instChildCount={instance.transform.childCount}");
            if (instance.transform.childCount == 0) LogHierarchy(instance.transform);

            // PrefabにUIManagerが無い、または子が無い場合は誤参照の可能性 → Resourcesから復旧
            if (ui == null || instance.transform.childCount == 0){
                Debug.LogError("[StageInitializer] 指定されたUIManager Prefabが不正です（UIManagerなし/子なし）。Resourcesから正しいPrefabを読み込みます。");
                if (instance != null) Destroy(instance);

                GameObject resourcePrefab = FindValidUIManagerPrefabInResources();
                if (resourcePrefab != null){
                    instance = Instantiate(resourcePrefab);
                    ui = instance.GetComponent<UIManager>();
                    Debug.Log($"[StageInitializer] Fallback Instantiate: {resourcePrefab.name}, instChildCount={instance.transform.childCount}");
                    if (instance.transform.childCount == 0) LogHierarchy(instance.transform);
                }
            }

            if (ui == null){
                Debug.LogError("[StageInitializer] 正しいUIManager Prefabを読み込めませんでした。Resources/Prefab/UIManager を確認してください。");
                yield break;
            }

            // 生成直後に破棄を防ぐ
            EnsureUIChildren(ui);
            
            // 生成したインスタンスをGameManagerに登録
            if (GameManager.Instance != null){
                GameManager.Instance.RegisterUI(ui);
            }
        }else{
            // 既存インスタンスを使用する場合も、子オブジェクトを確認
            EnsureUIChildren(ui);
        }

        // インスタンス名を保存（破棄される前に）
        string instanceName = instance != null ? instance.name : "UIManager (Clone)";

        // Persistentシーンがロードされるまで待つ（GameManagerがPersistentシーンにある場合）
        if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("Persistent").isLoaded){
            Debug.Log("[StageInitializer] Persistentシーンが未ロードのため、待機します。");
            float timeout = 5f;
            while (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("Persistent").isLoaded && timeout > 0f){
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // GameManager が用意できるまで待つ（シーン初期化順の差異に対応）
        float gmTimeout = 5f;
        while (GameManager.Instance == null && gmTimeout > 0f){
            gmTimeout -= Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (GameManager.Instance == null){
            Debug.LogError("[StageInitializer] GameManager.Instanceが見つかりません。PersistentシーンにGameManagerが配置されているか確認してください。");
        }

        // UIManagerのStart()で自動的にGameManagerに登録されるため、ここでの登録は不要
        // 念のため、UIManagerのStart()が呼ばれるまで少し待つ
        yield return null;
        
        // instanceが破棄されている可能性があるため、保存した名前を使用
        Debug.Log($"✅ {instanceName} 生成完了");

        // uiが破棄されている可能性があるため、nullチェック
        if (ui == null || ui.gameObject == null){
            Debug.LogWarning("[StageInitializer] UIManagerが破棄されました。UIManager.Instanceを確認します。");
            
            // UIManager.Instanceを優先的に使用（再生成は行わない）
            if (UIManager.Instance != null && UIManager.Instance.gameObject != null){
                ui = UIManager.Instance;
                instance = UIManager.Instance.gameObject;
                Debug.Log("[StageInitializer] ✅ 既存のUIManager.Instanceを使用します。");
                try{
                    if (!ui.gameObject.activeSelf){
                        ui.gameObject.SetActive(true);
                    }
                    ui.PreventDestroy();
                    EnsureUIChildren(ui);
                }catch (System.Exception e){
                    Debug.LogError($"[StageInitializer] UIManager.Instanceの復旧に失敗: {e.Message}");
                }
            }else{
                Debug.LogError("[StageInitializer] ⚠️ UIManager.Instanceもnullです。UIManagerは破棄されません。次のシーンで再生成されるまで待機します。");
                // 新規生成は行わない（UIManagerは破棄されないべき）
            }
        }
        
        // uiが有効な場合のみ処理を続行
        if (ui != null && ui.gameObject != null){
            // 破棄防止と参照再割り当て
            EnsureUIChildren(ui);
            // 既存の値をUIへ初期同期
            var coinMgr = CoinManager.Instance;
            if (coinMgr != null)
                ui.UpdateCoinUI(coinMgr.GetTotalCoins());

            // プレイヤーの HP/SP も初期同期
            var player = FindObjectOfType<PlayerData>();
            if (player != null){
                ui.UpdateHP(player.hp, player.maxHP);
                ui.UpdateSP(player.sp, player.maxSP);
            }
        }

        // UIManagerが破棄されてもBGMは再生する
        // GameManager.Instanceがnullになった場合、強制読み込み
        if (GameManager.Instance == null){
            Debug.LogWarning("[StageInitializer] GameManager.Instanceがnullです。強制読み込みを開始します。");
            yield return StartCoroutine(ForceLoadGameManager());
        }
        
        if (GameManager.Instance == null){
            Debug.LogError("[StageInitializer] GameManager.Instanceがnullです。強制読み込み後も見つかりませんでした。");
            yield break;
        }
        
        int id = bgmID >= 0 ? bgmID : GameManager.Instance.CurrentStage;
        Debug.Log($"[StageInitializer] BGM再生開始: bgmID={bgmID}, 最終ID={id}, GameManager.Instance={(GameManager.Instance != null ? "存在" : "null")}");
        
        GameManager.Instance.PlayBGM(id);
    }

    // GameManagerを強制的に読み込む（タイムアウトベース）
    private System.Collections.IEnumerator ForceLoadGameManager(){
        float timeout = 5f; // 5秒でタイムアウト
        int attemptCount = 0;
        
        while (GameManager.Instance == null && timeout > 0f){
            attemptCount++;
            timeout -= Time.unscaledDeltaTime;
            
            if (attemptCount % 10 == 0){ // 10回ごとにログ出力
                Debug.Log($"[StageInitializer] GameManager強制読み込み試行中... (残り時間: {timeout:F1}秒)");
            }
            
            // 1. Persistentシーンを強制ロード
            if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("Persistent").isLoaded){
                Debug.Log("[StageInitializer] Persistentシーンを強制ロードします。");
                var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Persistent", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                while (!async.isDone){
                    yield return null;
                }
                Debug.Log("[StageInitializer] Persistentシーンのロード完了");
            }
            
            // 2. GameManagerを検索（非アクティブも含む）
            GameManager foundGM = FindObjectOfType<GameManager>(true); // true = 非アクティブも検索
            if (foundGM == null){
                // すべてのGameObjectを検索
                GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
                foreach (var obj in allObjects){
                    if (obj.name.Contains("GameManager")){
                        foundGM = obj.GetComponent<GameManager>();
                        if (foundGM != null) break;
                    }
                }
            }
            
            if (foundGM != null){
                Debug.Log($"[StageInitializer] GameManagerオブジェクトを発見: {foundGM.name}, Active: {foundGM.gameObject.activeSelf}, Enabled: {foundGM.enabled}");
                
                // GameManagerを確実にアクティブ化
                if (!foundGM.gameObject.activeSelf){
                    Debug.Log("[StageInitializer] GameManagerが非アクティブのため、アクティブ化します。");
                    foundGM.gameObject.SetActive(true);
                }
                
                if (!foundGM.enabled){
                    Debug.Log("[StageInitializer] GameManagerスクリプトが無効のため、有効化します。");
                    foundGM.enabled = true;
                }
                
                // Instanceが設定されていない場合、手動で設定
                if (GameManager.Instance == null){
                    Debug.LogWarning("[StageInitializer] GameManager.Instanceがnullです。手動で設定を試みます。");
                    // Awakeが呼ばれるまで待つ（最大2秒）
                    float waitTime = 0f;
                    while (GameManager.Instance == null && waitTime < 2f){
                        waitTime += Time.unscaledDeltaTime;
                        yield return null;
                    }
                    
                    // まだnullの場合は、強制的にInstanceを設定（最後の手段）
                    if (GameManager.Instance == null && foundGM != null){
                        Debug.LogError("[StageInitializer] ⚠️ GameManager.Instanceが設定されません。強制的に設定します。");
                        foundGM.ForceSetInstance();
                        Debug.Log("[StageInitializer] GameManager.ForceSetInstance()を呼び出しました。");
                    }
                }
            }else{
                Debug.LogWarning("[StageInitializer] GameManagerオブジェクトが見つかりません。");
            }
            
            // 3. まだnullの場合は、もう一度待機
            if (GameManager.Instance == null){
                yield return null; // 1フレーム待機
            }
            
            // 4. 破棄されようとしているGameManagerを保護
            if (foundGM != null && foundGM.gameObject != null){
                foundGM.gameObject.SetActive(true);
                UnityEngine.Object.DontDestroyOnLoad(foundGM.gameObject);
                foundGM.PreventDestroy(); // 追加の保護
                Debug.Log("[StageInitializer] GameManagerにDontDestroyOnLoadを再設定し、保護しました。");
            }
        }
        
        if (GameManager.Instance != null){
            Debug.Log($"[StageInitializer] ✅ GameManager強制読み込み成功！試行回数: {attemptCount}");
        }else{
            Debug.LogError($"[StageInitializer] ❌ GameManager強制読み込み失敗。{attemptCount}回試行しましたが見つかりませんでした。");
        }
    }
}
