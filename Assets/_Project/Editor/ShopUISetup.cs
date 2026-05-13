using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.UI;

/// <summary>
/// Builds the PowerUpShop panel layout from scratch.
/// Run: Tools → SwarmProtocol → Setup Shop UI
/// </summary>
public static class ShopUISetup
{
    [MenuItem("Tools/SwarmProtocol/Setup Shop UI")]
    static void Setup()
    {
        GameObject panel = FindInactive("ShopPanel");
        if (panel == null) { Debug.LogError("[ShopSetup] ShopPanel not found"); return; }

        // ── 1. Panel: full-screen dark overlay ───────────────────────────────
        SetStretch(panel);
        var bg = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.92f);

        // ── 2. Title ──────────────────────────────────────────────────────────
        var title = GetOrCreate(panel, "TitleText");
        SetAnchored(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 70f));
        EnsureTMP(title, "POWER-UP SHOP", 42, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);

        // ── 3. Gold text (top-right) ──────────────────────────────────────────
        var goldGO = panel.transform.Find("goldText")?.gameObject ?? GetOrCreate(panel, "goldText");
        SetAnchored(goldGO, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -55f), new Vector2(200f, 50f));
        EnsureTMP(goldGO, "Gold: 0", 22, FontStyles.Bold, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right);

        // ── 4. Close button (top-left) ────────────────────────────────────────
        var closeGO = GetOrCreate(panel, "CloseButton");
        SetAnchored(closeGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -55f), new Vector2(120f, 50f));
        var closeImg = closeGO.GetComponent<Image>() ?? closeGO.AddComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f);
        if (closeGO.GetComponent<Button>() == null) closeGO.AddComponent<Button>();
        var closeTxtGO = GetOrCreate(closeGO, "Text");
        SetStretch(closeTxtGO);
        EnsureTMP(closeTxtGO, "Close", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);

        // ── 5. CardContainer ScrollRect ───────────────────────────────────────
        var containerGO = panel.transform.Find("CardContainer")?.gameObject;
        if (containerGO == null) { Debug.LogError("[ShopSetup] CardContainer not found"); return; }

        // Anchor: stretch from 5%-95% width, 8%-88% height (leaves room for title+gold+close)
        var containerRT = containerGO.GetComponent<RectTransform>();
        containerRT.anchorMin       = new Vector2(0.04f, 0.08f);
        containerRT.anchorMax       = new Vector2(0.96f, 0.88f);
        containerRT.sizeDelta       = Vector2.zero;
        containerRT.anchoredPosition = Vector2.zero;

        var sr = containerGO.GetComponent<ScrollRect>() ?? containerGO.AddComponent<ScrollRect>();
        sr.horizontal   = false;
        sr.vertical     = true;
        sr.movementType = ScrollRect.MovementType.Elastic;

        // Viewport
        var viewportGO = containerGO.transform.Find("Viewport")?.gameObject;
        if (viewportGO != null)
        {
            SetStretch(viewportGO);
            var mask = viewportGO.GetComponent<Mask>() ?? viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            var vImg = viewportGO.GetComponent<Image>() ?? viewportGO.AddComponent<Image>();
            vImg.color = new Color(1f, 1f, 1f, 0.01f);
            sr.viewport = viewportGO.GetComponent<RectTransform>();

            // Content
            var contentGO = viewportGO.transform.Find("Content")?.gameObject;
            if (contentGO != null)
            {
                var crt = contentGO.GetComponent<RectTransform>();
                crt.anchorMin        = new Vector2(0f, 1f);
                crt.anchorMax        = new Vector2(1f, 1f);
                crt.pivot            = new Vector2(0.5f, 1f);
                crt.sizeDelta        = new Vector2(0f, 0f);
                crt.anchoredPosition = Vector2.zero;

                // Remove old VerticalLayoutGroup if any (we want GridLayout)
                var oldVLG = contentGO.GetComponent<VerticalLayoutGroup>();
                if (oldVLG != null) Object.DestroyImmediate(oldVLG);

                var grid = contentGO.GetComponent<GridLayoutGroup>() ?? contentGO.AddComponent<GridLayoutGroup>();
                grid.cellSize        = new Vector2(260f, 320f);
                grid.spacing         = new Vector2(20f, 20f);
                grid.padding         = new RectOffset(20, 20, 20, 20);
                grid.startCorner     = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis       = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment  = TextAnchor.UpperCenter;
                grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;

                var csf = contentGO.GetComponent<ContentSizeFitter>() ?? contentGO.AddComponent<ContentSizeFitter>();
                csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                sr.content = crt;
            }
        }

        // Hide scrollbars
        var hScrollbar = containerGO.transform.Find("Scrollbar Horizontal");
        var vScrollbar = containerGO.transform.Find("Scrollbar Vertical");
        if (hScrollbar != null) hScrollbar.gameObject.SetActive(false);
        if (vScrollbar != null) vScrollbar.gameObject.SetActive(false);

        // ── 6. Fix card template layout ───────────────────────────────────────
        SetupCardTemplate(panel);

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[ShopSetup] Done. Save the scene (Ctrl+S).");
    }

    static void SetupCardTemplate(GameObject panel)
    {
        var shopUI = Object.FindFirstObjectByType<PowerUpShopUI>();
        if (shopUI == null) return;
        var so = new SerializedObject(shopUI);
        var cardPrefabGO = (so.FindProperty("cardPrefab")?.objectReferenceValue as PowerUpCardUI)?.gameObject;
        if (cardPrefabGO == null)
        {
            Debug.LogWarning("[ShopSetup] cardPrefab not wired — skipping card template setup.");
            return;
        }

        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cardPrefabGO);
        if (string.IsNullOrEmpty(prefabPath))
        {
            // It's a scene object, edit directly
            ApplyCardLayout(cardPrefabGO);
            EditorUtility.SetDirty(cardPrefabGO);
        }
        else
        {
            // It's a prefab asset — must use EditPrefabContentsScope
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                ApplyCardLayout(scope.prefabContentsRoot);
            }
        }

        Debug.Log("[ShopSetup] Card template layout applied.");
        Debug.Log("[ShopSetup] Card template layout applied to: " + cardPrefabGO.name);
    }

    static void ApplyCardLayout(GameObject root)
    {
        var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

        var vlg = root.GetComponent<VerticalLayoutGroup>() ?? root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 6f;
        vlg.childAlignment        = TextAnchor.UpperCenter;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        StyleCardChild(root, "iconImage",        60f, true);
        StyleCardChild(root, "nameText",          28f, false, 18, FontStyles.Bold,   Color.white,                  TextAlignmentOptions.Center);
        StyleCardChild(root, "descriptionText",   44f, false, 13, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center, wrap: true);
        StyleCardChild(root, "rankText",          24f, false, 14, FontStyles.Normal, new Color(0.4f, 0.85f, 1f),  TextAlignmentOptions.Center);
        StyleCardChild(root, "costText",          22f, false, 14, FontStyles.Bold,   new Color(1f, 0.85f, 0.3f),  TextAlignmentOptions.Center);

        // Spacer
        var spacer = GetOrCreate(root, "Spacer");
        var spacerRT = spacer.GetComponent<RectTransform>();
        if (spacerRT != null) spacerRT.sizeDelta = new Vector2(spacerRT.sizeDelta.x, 0f);
        var sle = spacer.GetComponent<LayoutElement>() ?? spacer.AddComponent<LayoutElement>();
        sle.flexibleHeight = 1f; sle.preferredHeight = 0f; sle.minHeight = 0f;

        // Buy button
        var buyGO = FindChildNamed(root, "buyButton") ?? FindChildNamed(root, "BuyButton");
        if (buyGO != null)
        {
            var le = buyGO.GetComponent<LayoutElement>() ?? buyGO.AddComponent<LayoutElement>();
            le.preferredHeight = 40f; le.minHeight = 40f; le.flexibleWidth = 1f;
            var btnImg = buyGO.GetComponent<Image>() ?? buyGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f);
            var btnTxt = buyGO.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null)
            {
                btnTxt.text = "Buy"; btnTxt.fontSize = 15;
                btnTxt.fontStyle = FontStyles.Bold;
                btnTxt.color = Color.white;
                btnTxt.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    static void StyleCardChild(GameObject card, string childName, float height, bool isImage,
        float fontSize = 14, FontStyles style = FontStyles.Normal, Color color = default,
        TextAlignmentOptions align = TextAlignmentOptions.Center, bool wrap = false)
    {
        var child = FindChildNamed(card, childName);
        if (child == null) return;
        var le = child.GetComponent<LayoutElement>() ?? child.AddComponent<LayoutElement>();
        le.preferredHeight = height; le.minHeight = isImage ? height : 0f; le.flexibleWidth = 1f;
        if (!isImage)
        {
            var tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize  = fontSize;
                tmp.fontStyle = style;
                tmp.color     = color == default ? Color.white : color;
                tmp.alignment = align;
                if (wrap) tmp.textWrappingMode = TextWrappingModes.Normal;
            }
        }
    }

    static GameObject FindChildNamed(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        foreach (Transform child in parent.transform)
            if (child.name.ToLower() == name.ToLower()) return child.gameObject;
        return null;
    }

    // ── Generic helpers ────────────────────────────────────────────────────────

    static void SetStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void SetAnchored(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    static void EnsureTMP(GameObject go, string text, float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style; tmp.color = color; tmp.alignment = align;
    }

    static GameObject GetOrCreate(GameObject parent, string name)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) return existing.gameObject;
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static GameObject FindInactive(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.IsValid()) return go;
        return null;
    }
}
