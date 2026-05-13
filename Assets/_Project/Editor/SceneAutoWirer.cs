using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;
using SwarmProtocol.UI;
using SwarmProtocol.Chest;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Stage;
using SwarmProtocol.Player;
using SwarmProtocol.Combat;
using SwarmProtocol.Drops;
using SwarmProtocol.Pickups;
using SwarmProtocol.Enemies;

/// <summary>
/// One-shot Editor tool that wires every serialized reference in the scene.
/// Run from: Tools → SwarmProtocol → Auto-Wire Scene
/// Then press Ctrl+S to save.
/// </summary>
public static class SceneAutoWirer
{
    // ─── Asset paths ────────────────────────────────────────────────────────────

    const string ConfigPath      = "Assets/_Project/ScriptableObjects/Instances/Config/GameConfig.asset";
    const string PowerUpCardPfb  = "Assets/_Project/Prefabs/UI/PowerUpCard.prefab";
    const string CharCardPfb     = "Assets/_Project/Prefabs/UI/CharacterCard.prefab";
    const string XPGemPfb        = "Assets/_Project/Prefabs/Pickups/XPGem.prefab";
    const string GoldCoinPfb     = "Assets/_Project/Prefabs/Pickups/GoldCoin.prefab";
    const string HealthOrbPfb    = "Assets/_Project/Prefabs/Pickups/HealthOrb.prefab";
    const string TreasureChestPfb= "Assets/_Project/Prefabs/Pickups/TreasureChest.prefab";
    const string CharDefaultPath = "Assets/_Project/ScriptableObjects/Instances/Characters/Character_Default.asset";
    const string StagesFolder    = "Assets/_Project/ScriptableObjects/Instances/Stages";
    const string PassivesFolder  = "Assets/_Project/ScriptableObjects/Instances/Passives";
    const string PowerUpsFolder  = "Assets/_Project/ScriptableObjects/Instances/PowerUps";
    const string WeaponsFolder   = "Assets/_Project/ScriptableObjects/Instances/Weapons";
    const string EnemiesFolder   = "Assets/_Project/ScriptableObjects/Instances/Enemies";

    // ─── Entry point ─────────────────────────────────────────────────────────────

    [MenuItem("Tools/SwarmProtocol/Auto-Wire Scene")]
    static void WireScene()
    {
        int n = 0;
        n += FixDuplicateRerollButton();
        n += RenameTierText();
        n += EnsureBootstrapComponents();
        n += WireBootstrapper();
        n += WireHUDController();
        n += WireWeaponSlotHUD();
        n += WirePassiveSlotHUD();
        n += WirePowerUpShopUI();
        n += WireCharacterSelectUI();
        n += WireMetaProgressionManager();
        n += WireLevelUpUI();
        n += WireUpgradeCardUIs();
        n += WirePauseMenuUI();
        n += WireGameOverUI();
        n += WireMainMenuUI();
        n += WireChestOpenUI();
        n += WireSlotMachineColumns();
        n += WireUpgradeManager();
        n += WireStageDrivers();
        n += WireCurseScalingDriver();
        n += WireWeaponEvolutionSystem();
        n += WireOverchargeManager();
        n += WireDropHandlers();
        n += WirePlayerWeaponOverride();
        n += CreateMissingStages();          // before WireStageManager
        AssetDatabase.SaveAssets();
        n += WireStageManager();             // needs all 5 stage assets

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[AutoWirer] Done — {n} properties wired. Save the scene (Ctrl+S).");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    static T[] LoadAll<T>(string folder, string pattern = "*.asset") where T : Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
        var list  = new List<T>();
        foreach (var g in guids)
        {
            var obj = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g));
            if (obj != null) list.Add(obj);
        }
        return list.ToArray();
    }

    static int Set(Component comp, string field, Object val)
    {
        if (comp == null || val == null) return 0;
        var so = new SerializedObject(comp);
        var p  = so.FindProperty(field);
        if (p == null) { Debug.LogWarning($"[AutoWirer] Field '{field}' not found on {comp.GetType().Name}"); return 0; }
        if (p.objectReferenceValue == val) return 0;
        p.objectReferenceValue = val;
        so.ApplyModifiedPropertiesWithoutUndo();
        return 1;
    }

    static int SetList<T>(Component comp, string field, T[] items) where T : Object
    {
        if (comp == null || items == null || items.Length == 0) return 0;
        var so = new SerializedObject(comp);
        var p  = so.FindProperty(field);
        if (p == null) { Debug.LogWarning($"[AutoWirer] List field '{field}' not found on {comp.GetType().Name}"); return 0; }
        p.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        return 1;
    }

    static GameObject GO(string path) => GameObject.Find(path);

    // Finds a child by a slash-separated relative path, including inactive objects.
    static GameObject FindDeep(GameObject root, string relativePath)
    {
        if (root == null) return null;
        var parts = relativePath.Split('/');
        Transform current = root.transform;
        foreach (var part in parts)
        {
            current = current.Find(part);
            if (current == null) return null;
        }
        return current.gameObject;
    }

    // Like GO() but works on inactive objects under a known active root.
    static GameObject GOInactive(string path)
    {
        var active = GameObject.Find(path);
        if (active != null) return active;
        // Try walking from scene roots via Resources.FindObjectsOfTypeAll
        var parts = path.Split('/');
        foreach (var root in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!root.scene.IsValid()) continue;
            if (root.name != parts[0]) continue;
            if (parts.Length == 1) return root;
            var found = FindDeep(root, string.Join("/", parts, 1, parts.Length - 1));
            if (found != null) return found;
        }
        return null;
    }

    // Finds a GO by locating any scene object named `rootName` then walking `childPath` from it.
    // Works even when the root is inactive. Pass null childPath to return the root itself.
    static GameObject FindInactiveByPath(string rootName, string childPath)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid() || go.name != rootName) continue;
            if (childPath == null) return go;
            var child = go.transform.Find(childPath);
            if (child != null) return child.gameObject;
        }
        return null;
    }

    static T Comp<T>(string goPath) where T : Component
    {
        var go = GO(goPath);
        return go != null ? go.GetComponent<T>() : null;
    }

    // ─── Fix 1: Remove duplicate RerollButton ────────────────────────────────────

    static int FixDuplicateRerollButton()
    {
        var panel = GO("LevelUpPanel");
        if (panel == null) return 0;

        var toDelete = new List<Transform>();
        foreach (Transform child in panel.transform)
        {
            if (child.name == "RerollButton" && child.Find("RerollChargeText") == null)
                toDelete.Add(child);
        }

        foreach (var t in toDelete)
        {
            Debug.Log("[AutoWirer] Deleted duplicate RerollButton (no RerollChargeText child)");
            Object.DestroyImmediate(t.gameObject);
        }
        return toDelete.Count;
    }

    // ─── Fix 2: Rename ChestPanel's generic "Text (TMP)" to "TierText" ────────────

    static int RenameTierText()
    {
        var chestPanel = GO("ChestPanel");
        if (chestPanel == null) return 0;
        var t = chestPanel.transform.Find("Text (TMP)");
        if (t == null) return 0;
        t.name = "TierText";
        EditorUtility.SetDirty(t.gameObject);
        Debug.Log("[AutoWirer] Renamed ChestPanel/Text (TMP) → TierText");
        return 1;
    }

    // ─── Fix 3: Add missing Singleton components to Bootstrap ────────────────────

    static int EnsureBootstrapComponents()
    {
        var bootstrap = GO("Bootstrap");
        if (bootstrap == null) { Debug.LogWarning("[AutoWirer] Bootstrap GO not found"); return 0; }
        int n = 0;
        n += AddIfMissing<ActiveEnemyRegistry>(bootstrap);
        n += AddIfMissing<MetaProgressionManager>(bootstrap);
        n += AddIfMissing<CharacterSelectManager>(bootstrap);
        n += AddIfMissing<GoldService>(bootstrap);
        return n;
    }

    static int AddIfMissing<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return 0;
        go.AddComponent<T>();
        EditorUtility.SetDirty(go);
        Debug.Log($"[AutoWirer] Added {typeof(T).Name} to {go.name}");
        return 1;
    }

    // ─── Wire: Bootstrapper ──────────────────────────────────────────────────────

    static int WireBootstrapper()
    {
        var comp = Comp<Bootstrapper>("Bootstrap");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "gameConfig",   Load<GameConfigSO>(ConfigPath));
        n += Set(comp, "gameManager",  Comp<GameManager>("Managers/GameManager"));
        n += Set(comp, "poolManager",  Comp<ObjectPoolManager>("Managers/ObjectPoolManager"));
        n += Set(comp, "goldManager",  Comp<GoldManager>("Managers/GoldManager"));
        n += Set(comp, "enemyRegistry",GO("Bootstrap").GetComponent<ActiveEnemyRegistry>());
        return n;
    }

    // ─── Wire: HUDController ─────────────────────────────────────────────────────

    static int WireHUDController()
    {
        var hud = Comp<HUDController>("HUD");
        if (hud == null) { Debug.LogWarning("[AutoWirer] HUDController not found on 'HUD'"); return 0; }
        int n = 0;
        n += Set(hud, "healthBar",    GO("HUD/HUDRoot/HealthBar")?.GetComponent<Slider>());
        n += Set(hud, "healthText",   GO("HUD/HUDRoot/HealthText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "xpBar",        GO("HUD/HUDRoot/XPBar")?.GetComponent<Slider>());
        n += Set(hud, "levelText",    GO("HUD/HUDRoot/LevelText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "stageText",    GO("HUD/HUDRoot/StageText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "timerText",    GO("HUD/HUDRoot/TimerText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "killCountText",GO("HUD/HUDRoot/KillCountText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "goldText",     GO("HUD/HUDRoot/GoldText")?.GetComponent<TextMeshProUGUI>());
        n += Set(hud, "playerHealth", GO("Player")?.GetComponent<PlayerHealth>());
        n += Set(hud, "playerXP",     GO("Player")?.GetComponent<PlayerXP>());
        n += Set(hud, "stageManager", Comp<StageManager>("StageSystem/StageManager"));
        n += Set(hud, "hudRoot",      GO("HUD/HUDRoot"));
        return n;
    }

    // ─── Wire: WeaponSlotHUD ─────────────────────────────────────────────────────

    static int WireWeaponSlotHUD()
    {
        var comp = Comp<WeaponSlotHUD>("HUD/WeaponSlotHUD");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "weaponManager", GO("Player")?.GetComponent<WeaponManager>());

        var images = new List<Image>();
        var wshGO  = GO("HUD/WeaponSlotHUD");
        if (wshGO != null)
            foreach (Transform child in wshGO.transform)
            {
                var img = child.GetComponent<Image>();
                if (img != null) images.Add(img);
            }
        n += SetList(comp, "iconSlots", images.ToArray());
        return n;
    }

    // ─── Wire: PassiveSlotHUD ────────────────────────────────────────────────────

    static int WirePassiveSlotHUD()
    {
        var comp = Comp<PassiveSlotHUD>("HUD/PassiveSlotHUD");
        if (comp == null) return 0;

        var images = new List<Image>();
        var pshGO  = GO("HUD/PassiveSlotHUD");
        if (pshGO != null)
            foreach (Transform child in pshGO.transform)
            {
                var img = child.GetComponent<Image>();
                if (img != null) images.Add(img);
            }
        return SetList(comp, "iconSlots", images.ToArray());
    }

    // ─── Wire: PowerUpShopUI ────────────────────────────────────────────────────

    static int WirePowerUpShopUI()
    {
        var comp = Comp<PowerUpShopUI>("HUD/PowerUpShopUI");
        if (comp == null) return 0;

        var cardPrefabGO = Load<GameObject>(PowerUpCardPfb);
        var cardPrefab   = cardPrefabGO != null ? cardPrefabGO.GetComponent<PowerUpCardUI>() : null;

        // ShopPanel is inactive — use name-based search that works with inactive objects.
        var contentGO = FindInactiveByPath("ShopPanel", "CardContainer/Viewport/Content");
        var shopPanelGO = FindInactiveByPath("ShopPanel", null);
        var goldTextGO  = FindInactiveByPath("ShopPanel", "goldText");

        int n = 0;
        n += Set(comp, "shopPanel",     shopPanelGO);
        n += Set(comp, "goldText",      goldTextGO?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "cardPrefab",    cardPrefab);
        n += Set(comp, "cardContainer", contentGO?.GetComponent<Transform>());
        return n;
    }

    // ─── Wire: CharacterSelectUI ─────────────────────────────────────────────────

    static int WireCharacterSelectUI()
    {
        var comp = Comp<CharacterSelectUI>("HUD/CharacterSelectUI");
        if (comp == null) return 0;

        var charCard = Load<GameObject>(CharCardPfb);
        var content  = GO("HUD/CharacterSelectPanel/CharacterCardContainer/Viewport/Content");
        var charData = Load<CharacterDataSO>(CharDefaultPath);

        int n = 0;
        n += Set(comp, "selectPanel",        GO("HUD/CharacterSelectPanel"));
        n += Set(comp, "characterCardPrefab", charCard);
        n += Set(comp, "cardContainer",       content?.transform);
        if (charData != null) n += SetList(comp, "characters", new CharacterDataSO[] { charData });
        return n;
    }

    // ─── Wire: MetaProgressionManager ────────────────────────────────────────────

    static int WireMetaProgressionManager()
    {
        var comp = GO("Bootstrap")?.GetComponent<MetaProgressionManager>();
        if (comp == null) return 0;

        var allPUs = LoadAll<PowerUpDataSO>(PowerUpsFolder);
        var reroll = Load<PowerUpDataSO>($"{PowerUpsFolder}/PowerUp_Reroll.asset");
        var skip   = Load<PowerUpDataSO>($"{PowerUpsFolder}/PowerUp_Skip.asset");
        var banish = Load<PowerUpDataSO>($"{PowerUpsFolder}/PowerUp_Banish.asset");

        int n = 0;
        n += SetList(comp, "allPowerUps", allPUs);
        n += Set(comp, "rerollPowerUp", reroll);
        n += Set(comp, "skipPowerUp",   skip);
        n += Set(comp, "banishPowerUp", banish);
        return n;
    }

    // ─── Wire: LevelUpUI ────────────────────────────────────────────────────────

    static int WireLevelUpUI()
    {
        var comp = Comp<LevelUpUI>("UICanvas") ?? Object.FindFirstObjectByType<LevelUpUI>();
        if (comp == null) return 0;

        var panel = GO("LevelUpPanel");

        // Find the correct RerollButton (has RerollChargeText child)
        Button rerollBtn = null;
        TextMeshProUGUI rerollChargeText = null;
        foreach (Transform child in panel.transform)
        {
            if (child.name != "RerollButton") continue;
            var ct = child.Find("RerollChargeText");
            if (ct != null)
            {
                rerollBtn        = child.GetComponent<Button>();
                rerollChargeText = ct.GetComponent<TextMeshProUGUI>();
                break;
            }
        }

        var skipBtnGO    = GO("LevelUpPanel/SkipButton");
        var skipChargeGO = GO("LevelUpPanel/SkipButton/SkipChargeText");

        var cards = new List<UpgradeCardUI>();
        foreach (string name in new[] { "Card_1", "Card_2", "Card_3" })
        {
            var cardGO = panel.transform.Find("CardRow/" + name) ?? panel.transform.Find(name);
            if (cardGO != null)
            {
                var c = cardGO.GetComponent<UpgradeCardUI>();
                if (c != null) cards.Add(c);
            }
        }

        int n = 0;
        n += Set(comp, "levelUpPanel",    panel);
        n += SetList(comp, "cards",       cards.ToArray());
        n += Set(comp, "rerollButton",    rerollBtn);
        n += Set(comp, "skipButton",      skipBtnGO?.GetComponent<Button>());
        n += Set(comp, "rerollChargeText",rerollChargeText);
        n += Set(comp, "skipChargeText",  skipChargeGO?.GetComponent<TextMeshProUGUI>());
        return n;
    }

    // ─── Wire: UpgradeCardUI (all 3 cards) ───────────────────────────────────────

    static int WireUpgradeCardUIs()
    {
        var panel = GO("LevelUpPanel");
        if (panel == null) return 0;
        int n = 0;
        foreach (string name in new[] { "Card_1", "Card_2", "Card_3" })
        {
            var cardGO = panel.transform.Find("CardRow/" + name) ?? panel.transform.Find(name);
            if (cardGO == null) continue;
            var comp   = cardGO.GetComponent<UpgradeCardUI>();
            if (comp   == null) continue;

            n += Set(comp, "button",          cardGO.Find("Button")?.GetComponent<Button>());
            n += Set(comp, "banishButton",     cardGO.Find("BanishButton")?.GetComponent<Button>());
            n += Set(comp, "iconImage",        cardGO.Find("Icon")?.GetComponent<Image>());
            n += Set(comp, "nameText",         cardGO.Find("NameText")?.GetComponent<TextMeshProUGUI>());
            n += Set(comp, "descriptionText",  cardGO.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>());
            n += Set(comp, "levelText",        cardGO.Find("LevelText")?.GetComponent<TextMeshProUGUI>());
            n += Set(comp, "rarityBackground", cardGO.Find("RarityBackground")?.GetComponent<Image>());
        }
        return n;
    }

    // ─── Wire: PauseMenuUI ───────────────────────────────────────────────────────

    static int WirePauseMenuUI()
    {
        var comp = Comp<PauseMenuUI>("PausePanel");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "pausePanel",      GO("PausePanel"));
        n += Set(comp, "resumeButton",    GO("PausePanel/ResumeButton")?.GetComponent<Button>());
        n += Set(comp, "quitToMenuButton",GO("PausePanel/QuitToMenuButton")?.GetComponent<Button>());
        return n;
    }

    // ─── Wire: GameOverUI ────────────────────────────────────────────────────────

    static int WireGameOverUI()
    {
        var comp = Comp<GameOverUI>("GameOverPanel");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "gameOverPanel",  GO("GameOverPanel"));
        n += Set(comp, "stageText",      GO("GameOverPanel/StageText")?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "killCountText",  GO("GameOverPanel/KillCountText")?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "goldText",       GO("GameOverPanel/GoldText")?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "restartButton",  GO("GameOverPanel/RestartButton")?.GetComponent<Button>());
        n += Set(comp, "menuButton",     GO("GameOverPanel/MenuButton")?.GetComponent<Button>());
        n += Set(comp, "stageManager",   Comp<StageManager>("StageSystem/StageManager"));
        return n;
    }

    // ─── Wire: MainMenuUI ────────────────────────────────────────────────────────

    static int WireMainMenuUI()
    {
        var comp = Comp<MainMenuUI>("MenuPanel");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "menuPanel",           GO("MenuPanel"));
        n += Set(comp, "startButton",         GO("MenuPanel/StartButton")?.GetComponent<Button>());
        n += Set(comp, "quitButton",          GO("MenuPanel/QuitButton")?.GetComponent<Button>());
        n += Set(comp, "shopButton",          GO("MenuPanel/ShopButton")?.GetComponent<Button>());
        n += Set(comp, "characterSelectButton",GO("MenuPanel/CharacterButton")?.GetComponent<Button>());
        n += Set(comp, "shopUI",              Comp<PowerUpShopUI>("HUD/PowerUpShopUI"));
        n += Set(comp, "characterSelectUI",   Comp<CharacterSelectUI>("HUD/CharacterSelectUI"));
        return n;
    }

    // ─── Wire: ChestOpenUI ───────────────────────────────────────────────────────

    static int WireChestOpenUI()
    {
        var comp = Comp<ChestOpenUI>("ChestOpenUI");
        if (comp == null) return 0;

        var chestPanel = GO("ChestPanel");
        if (chestPanel == null) return 0;

        // Collect column components and GOs
        var colContainer = chestPanel.transform.Find("ColumnContainer");
        var columns      = new List<SlotMachineColumn>();
        var columnGOs    = new List<GameObject>();
        if (colContainer != null)
        {
            for (int i = 1; i <= 5; i++)
            {
                var col = colContainer.Find($"Column{i}");
                if (col != null)
                {
                    columnGOs.Add(col.gameObject);
                    var smc = col.GetComponent<SlotMachineColumn>();
                    if (smc != null) columns.Add(smc);
                }
            }
        }

        int n = 0;
        n += Set(comp, "chestPanel",       chestPanel);
        n += Set(comp, "tierText",         chestPanel.transform.Find("TierText")?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "jackpotBannerText",chestPanel.transform.Find("JackpotBanner")?.GetComponent<TextMeshProUGUI>());
        n += Set(comp, "collectButton",    chestPanel.transform.Find("CollectButton")?.GetComponent<Button>());
        n += SetList(comp, "columns",      columns.ToArray());
        n += SetList(comp, "columnObjects",columnGOs.ToArray());
        n += Set(comp, "config",           Load<GameConfigSO>(ConfigPath));
        return n;
    }

    // ─── Wire: SlotMachineColumn children ────────────────────────────────────────

    static int WireSlotMachineColumns()
    {
        var chestPanel   = GO("ChestPanel");
        if (chestPanel == null) return 0;
        var colContainer = chestPanel.transform.Find("ColumnContainer");
        if (colContainer == null) return 0;

        int n = 0;
        for (int i = 1; i <= 5; i++)
        {
            var col = colContainer.Find($"Column{i}");
            if (col == null) continue;
            var smc = col.GetComponent<SlotMachineColumn>();
            if (smc == null) continue;

            n += Set(smc, "iconImage", col.Find("Image")?.GetComponent<Image>());
            n += Set(smc, "nameText",  col.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>());
        }
        return n;
    }

    // ─── Wire: UpgradeManager ────────────────────────────────────────────────────

    static int WireUpgradeManager()
    {
        var comp = Comp<UpgradeManager>("Managers/UpgradeManager");
        if (comp == null) return 0;

        var passives = LoadAll<PassiveItemSO>(PassivesFolder);

        // Only the 3 starter weapons — exclude evolved and special pickup weapons
        var starterWeapons = new WeaponDataSO[]
        {
            Load<WeaponDataSO>($"{WeaponsFolder}/WeaponData_PulseRifle.asset"),
            Load<WeaponDataSO>($"{WeaponsFolder}/WeaponData_Scattergun.asset"),
            Load<WeaponDataSO>($"{WeaponsFolder}/WeaponData_PowerFist.asset"),
        };

        int n = 0;
        n += Set(comp, "weaponManager", GO("Player")?.GetComponent<WeaponManager>());
        n += Set(comp, "playerHealth",  GO("Player")?.GetComponent<PlayerHealth>());
        n += Set(comp, "playerXP",      GO("Player")?.GetComponent<PlayerXP>());
        n += Set(comp, "config",        Load<GameConfigSO>(ConfigPath));
        n += Set(comp, "stageTimer",    Comp<StageTimer>("StageSystem/StageTimer"));
        n += SetList(comp, "availableWeapons",  starterWeapons);
        n += SetList(comp, "availablePassives", passives);
        return n;
    }

    // ─── Wire: StageManager ──────────────────────────────────────────────────────

    static int WireStageManager()
    {
        var comp = Comp<StageManager>("StageSystem/StageManager");
        if (comp == null) return 0;

        var stages = LoadAll<StageDataSO>(StagesFolder);
        // Sort by stage number
        System.Array.Sort(stages, (a, b) => a.stageNumber.CompareTo(b.stageNumber));

        int n = 0;
        n += SetList(comp, "stages",     stages);
        n += Set(comp,     "stageTimer", Comp<StageTimer>("StageSystem/StageTimer"));
        return n;
    }

    // ─── Wire: Stage drivers ─────────────────────────────────────────────────────

    static int WireStageDrivers()
    {
        var spawner   = Comp<EnemySpawner>("StageSystem/EnemySpawner");
        var timer     = Comp<StageTimer>("StageSystem/StageTimer");
        int n = 0;

        var spawnDrv  = Comp<StageSpawnDriver>("StageSystem/StageSpawnDriver");
        if (spawnDrv != null)
        {
            n += Set(spawnDrv, "enemySpawner", spawner);
            n += Set(spawnDrv, "stageTimer",   timer);
        }

        var bossDrv   = Comp<BossSpawnDriver>("StageSystem/BossSpawnDriver");
        if (bossDrv != null)
        {
            n += Set(bossDrv, "enemySpawner", spawner);
            n += Set(bossDrv, "stageTimer",   timer);
        }

        var eliteDrv  = Comp<EliteBurstDriver>("StageSystem/EliteBurstDriver");
        if (eliteDrv != null)
        {
            n += Set(eliteDrv, "enemySpawner", spawner);
            n += Set(eliteDrv, "stageTimer",   timer);
        }

        return n;
    }

    // ─── Wire: CurseScalingDriver ────────────────────────────────────────────────

    static int WireCurseScalingDriver()
    {
        var comp = Comp<CurseScalingDriver>("CurseScalingDriver");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "spawnDriver", Comp<StageSpawnDriver>("StageSystem/StageSpawnDriver"));
        n += Set(comp, "config",      Load<GameConfigSO>(ConfigPath));
        return n;
    }

    // ─── Wire: WeaponEvolutionSystem ─────────────────────────────────────────────

    static int WireWeaponEvolutionSystem()
    {
        var comp = Comp<WeaponEvolutionSystem>("WeaponEvolutionSystem");
        if (comp == null) return 0;
        return Set(comp, "weaponManager", GO("Player")?.GetComponent<WeaponManager>());
    }

    // ─── Wire: OverchargeManager ─────────────────────────────────────────────────

    static int WireOverchargeManager()
    {
        var comp = Comp<OverchargeManager>("OverchargeManager");
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "weaponManager", GO("Player")?.GetComponent<WeaponManager>());
        n += Set(comp, "config",        Load<GameConfigSO>(ConfigPath));
        return n;
    }

    // ─── Wire: DropHandlers ──────────────────────────────────────────────────────

    static int WireDropHandlers()
    {
        var dropGO = GO("DropHandlers");
        if (dropGO == null) return 0;
        int n = 0;

        var xpDrp = dropGO.GetComponent<XPDropHandler>();
        if (xpDrp != null)
            n += Set(xpDrp, "xpGemPrefab", Load<GameObject>(XPGemPfb)?.GetComponent<XPGem>());

        var goldDrp = dropGO.GetComponent<GoldDropHandler>();
        if (goldDrp != null)
            n += Set(goldDrp, "goldCoinPrefab", Load<GameObject>(GoldCoinPfb)?.GetComponent<GoldCoin>());

        var healthDrp = dropGO.GetComponent<HealthOrbDropHandler>();
        if (healthDrp != null)
            n += Set(healthDrp, "healthOrbPrefab", Load<GameObject>(HealthOrbPfb)?.GetComponent<HealthOrb>());

        var chestDrp = dropGO.GetComponent<ChestDropHandler>();
        if (chestDrp != null)
        {
            n += Set(chestDrp, "chestPrefab", Load<GameObject>(TreasureChestPfb)?.GetComponent<TreasureChest>());
            n += Set(chestDrp, "stageTimer",  Comp<StageTimer>("StageSystem/StageTimer"));
        }

        return n;
    }

    // ─── Wire: PlayerWeaponOverride ──────────────────────────────────────────────

    static int WirePlayerWeaponOverride()
    {
        var comp = GO("Player")?.GetComponent<PlayerWeaponOverride>();
        if (comp == null) return 0;
        int n = 0;
        n += Set(comp, "weaponManager", GO("Player")?.GetComponent<WeaponManager>());
        n += Set(comp, "playerStats",   GO("Player")?.GetComponent<PlayerStats>());
        // NdujaWeapon is a child of Player/WeaponContainer — find its StrategyWeapon component
        var ndujaGO = GO("Player/WeaponContainer/NdujaWeapon");
        n += Set(comp, "ndujaWeapon",   ndujaGO?.GetComponent<StrategyWeapon>());
        return n;
    }

    // ─── Create missing Stage SOs (Stage3, Stage4, Stage5) ───────────────────────

    static int CreateMissingStages()
    {
        int n = 0;
        var swarmer  = Load<EnemyDataSO>($"{EnemiesFolder}/EnemyData_Swarmer.asset");
        var rangedBD = Load<EnemyDataSO>($"{EnemiesFolder}/EnemyData_RangedBD.asset");
        var tank     = Load<EnemyDataSO>($"{EnemiesFolder}/EnemyData_Tank.asset");
        var eliteBD  = Load<EnemyDataSO>($"{EnemiesFolder}/EnemyData_EliteBD.asset");

        n += CreateStageIfMissing("Stage3", 3, 240f, 1.7f, 60f, 20, 4, eliteBD,
            new[] {
                new BracketDef(swarmer,  1.5f, 3, 0),
                new BracketDef(rangedBD, 3f,   1, 30),
                new BracketDef(tank,     5f,   1, 90),
            },
            new[] { new BossDef(eliteBD, 150f) });

        n += CreateStageIfMissing("Stage4", 4, 270f, 2.0f, 60f, 25, 6, eliteBD,
            new[] {
                new BracketDef(swarmer,  1.2f, 3, 0),
                new BracketDef(rangedBD, 3f,   2, 30),
                new BracketDef(tank,     4f,   1, 90),
            },
            new[] { new BossDef(eliteBD, 120f) });

        n += CreateStageIfMissing("Stage5", 5, 300f, 2.5f, 45f, 30, 8, eliteBD,
            new[] {
                new BracketDef(swarmer,  1.0f, 4, 0),
                new BracketDef(rangedBD, 2.5f, 2, 30),
                new BracketDef(tank,     3f,   2, 60),
                new BracketDef(eliteBD,  8f,   1, 120),
            },
            new[] { new BossDef(tank, 90f), new BossDef(eliteBD, 210f) });

        return n;
    }

    struct BracketDef { public EnemyDataSO enemy; public float interval; public int count; public float from;
        public BracketDef(EnemyDataSO e, float i, int c, float f) { enemy=e; interval=i; count=c; from=f; } }
    struct BossDef    { public EnemyDataSO enemy; public float atTime;
        public BossDef(EnemyDataSO e, float t) { enemy=e; atTime=t; } }

    static int CreateStageIfMissing(string name, int num, float dur, float diff,
        float evolutionTime, float eliteBurstBeforeEnd, int eliteBurstCount,
        EnemyDataSO eliteEnemy, BracketDef[] brackets, BossDef[] bosses)
    {
        string path = $"{StagesFolder}/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<StageDataSO>(path) != null) return 0;

        var so = ScriptableObject.CreateInstance<StageDataSO>();
        so.stageName              = $"Stage {num}";
        so.stageNumber            = num;
        so.stageDuration          = dur;
        so.difficultyMultiplier   = diff;
        so.evolutionAllowedAfterTime = evolutionTime;
        so.eliteBurstTimeBeforeEnd   = eliteBurstBeforeEnd;
        so.eliteBurstCount           = eliteBurstCount;
        so.eliteEnemyData            = eliteEnemy;

        foreach (var b in brackets)
            so.spawnBrackets.Add(new StageDataSO.SpawnBracket
                { fromTime=b.from, enemyData=b.enemy, spawnInterval=b.interval, spawnCount=b.count });

        foreach (var b in bosses)
            so.bossSpawns.Add(new StageDataSO.BossSpawn { atTime=b.atTime, bossEnemy=b.enemy });

        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[AutoWirer] Created {path}");
        return 1;
    }
}
