using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the LevelUp panel layout from scratch.
/// Run: Tools → SwarmProtocol → Setup LevelUp UI
/// </summary>
public static class LevelUpUISetup
{
    [MenuItem("Tools/SwarmProtocol/Setup LevelUp UI")]
    static void Setup()
    {
        var panel = GameObject.Find("LevelUpPanel");
        if (panel == null) { Debug.LogError("[LevelUpSetup] LevelUpPanel not found"); return; }

        // ── 1. LevelUpPanel: full-screen dark overlay ─────────────────────────
        SetStretch(panel);
        EnsureImage(panel, new Color(0f, 0f, 0f, 0.88f));

        // ── 2. Title text ─────────────────────────────────────────────────────
        var title = GetOrCreate(panel, "TitleText");
        SetAnchored(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(600f, 80f));
        EnsureTMP(title, "LEVEL UP!", 48, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);

        // ── 3. Card row container ─────────────────────────────────────────────
        var row = GetOrCreate(panel, "CardRow");
        SetAnchored(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(960f, 420f));
        EnsureHorizontalLayout(row, 20f);

        // Move the 3 cards under the row
        foreach (var name in new[] { "Card_1", "Card_2", "Card_3" })
        {
            var card = panel.transform.Find(name);
            if (card == null) continue;
            card.SetParent(row.transform, false);
            SetupCard(card.gameObject);
        }

        // ── 4. Reroll + Skip buttons ──────────────────────────────────────────
        var rerollBtn = FindChildByName(panel, "RerollButton");
        if (rerollBtn != null)
            SetAnchored(rerollBtn.gameObject, new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(0f, 50f), new Vector2(200f, 60f));

        var skipBtn = panel.transform.Find("SkipButton");
        if (skipBtn != null)
            SetAnchored(skipBtn.gameObject, new Vector2(0.65f, 0f), new Vector2(0.65f, 0f), new Vector2(0f, 50f), new Vector2(200f, 60f));

        // Style reroll/skip buttons
        if (rerollBtn != null) StyleButton(rerollBtn.gameObject, "Reroll (1)", new Color(0.2f, 0.5f, 0.9f));
        if (skipBtn   != null) StyleButton(skipBtn.gameObject,   "Skip (1)",   new Color(0.4f, 0.4f, 0.4f));

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[LevelUpSetup] Done. Save the scene (Ctrl+S).");
    }

    // ── Card setup ────────────────────────────────────────────────────────────

    static void SetupCard(GameObject card)
    {
        // Card size driven by layout group (ContentSizeFitter not needed — fixed row height)
        var le = card.GetComponent<LayoutElement>() ?? card.AddComponent<LayoutElement>();
        le.preferredWidth  = 280f;
        le.preferredHeight = 420f;
        le.flexibleWidth   = 1f;

        // Card background
        var img = card.GetComponent<Image>() ?? card.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.18f, 1f);

        // Children layout
        var vg = card.GetComponent<VerticalLayoutGroup>() ?? card.AddComponent<VerticalLayoutGroup>();
        vg.padding        = new RectOffset(12, 12, 12, 12);
        vg.spacing        = 8f;
        vg.childAlignment = TextAnchor.UpperCenter;
        vg.childControlWidth   = true;
        vg.childControlHeight  = true;
        vg.childForceExpandWidth  = true;
        vg.childForceExpandHeight = false;

        // Rarity background — stretch behind everything (make it first child)
        var rarBG = card.transform.Find("RarityBackground");
        if (rarBG != null)
        {
            // Pull it out of the layout
            var le2 = rarBG.GetComponent<LayoutElement>() ?? rarBG.gameObject.AddComponent<LayoutElement>();
            le2.ignoreLayout = true;
            SetStretch(rarBG.gameObject);
            var ri = rarBG.GetComponent<Image>() ?? rarBG.gameObject.AddComponent<Image>();
            ri.color = new Color(0.55f, 0.55f, 0.55f, 0.25f);
            rarBG.SetAsFirstSibling();
        }

        // Icon
        var icon = card.transform.Find("Icon");
        if (icon != null)
        {
            SetLayout(icon.gameObject, 100f, false);
            var ii = icon.GetComponent<Image>() ?? icon.gameObject.AddComponent<Image>();
            ii.color = Color.white;
            ii.preserveAspect = true;
        }

        // NameText
        var nameT = card.transform.Find("NameText");
        if (nameT != null)
        {
            SetLayout(nameT.gameObject, 36f, false);
            var t = nameT.GetComponent<TextMeshProUGUI>() ?? nameT.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize  = 20;
            t.fontStyle = FontStyles.Bold;
            t.color     = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.text      = "Weapon Name";
        }

        // DescriptionText
        var descT = card.transform.Find("DescriptionText");
        if (descT != null)
        {
            SetLayout(descT.gameObject, 60f, false);
            var t = descT.GetComponent<TextMeshProUGUI>() ?? descT.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize  = 14;
            t.color     = new Color(0.8f, 0.8f, 0.8f);
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.text      = "Description";
        }

        // LevelText
        var lvlT = card.transform.Find("LevelText");
        if (lvlT != null)
        {
            SetLayout(lvlT.gameObject, 24f, false);
            var t = lvlT.GetComponent<TextMeshProUGUI>() ?? lvlT.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize  = 13;
            t.color     = new Color(1f, 0.85f, 0.3f);
            t.alignment = TextAlignmentOptions.Center;
            t.text      = "Lv.1 → 2";
        }

        // Spacer — must have sizeDelta.y=0 so VLG doesn't count its default 100px rect height
        var spacer = GetOrCreate(card, "Spacer");
        var spacerRT = spacer.GetComponent<RectTransform>();
        if (spacerRT != null) spacerRT.sizeDelta = new Vector2(spacerRT.sizeDelta.x, 0f);
        var sleFlex = spacer.GetComponent<LayoutElement>() ?? spacer.AddComponent<LayoutElement>();
        sleFlex.preferredHeight = 0f;
        sleFlex.minHeight       = 0f;
        sleFlex.flexibleHeight  = 1f;

        // Select button (main)
        var btnGO = card.transform.Find("Button");
        if (btnGO != null)
        {
            SetLayout(btnGO.gameObject, 50f, false);
            StyleButton(btnGO.gameObject, "Select", new Color(0.2f, 0.7f, 0.3f));
        }

        // Banish button
        var banishGO = card.transform.Find("BanishButton");
        if (banishGO != null)
        {
            SetLayout(banishGO.gameObject, 36f, false);
            StyleButton(banishGO.gameObject, "Banish", new Color(0.7f, 0.2f, 0.2f));
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static void SetStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static void SetAnchored(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin       = anchorMin;
        rt.anchorMax       = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = size;
    }

    static void SetLayout(GameObject go, float height, bool flex)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.preferredHeight  = height;
        le.flexibleHeight   = flex ? 1f : 0f;
        le.flexibleWidth    = 1f;
        le.minHeight        = height;
    }

    static void EnsureImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
    }

    static void EnsureTMP(GameObject go, string text, float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = color;
        tmp.alignment = align;
    }

    static void EnsureHorizontalLayout(GameObject go, float spacing)
    {
        var hlg = go.GetComponent<HorizontalLayoutGroup>() ?? go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = spacing;
        hlg.childAlignment       = TextAnchor.MiddleCenter;
        hlg.childControlWidth    = true;
        hlg.childControlHeight   = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 0, 0);
    }

    static void StyleButton(GameObject go, string label, Color bgColor)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = bgColor;

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text      = label;
            tmp.fontSize  = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
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

    static Transform FindChildByName(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
            if (child.name == name) return child;
        return null;
    }
}
