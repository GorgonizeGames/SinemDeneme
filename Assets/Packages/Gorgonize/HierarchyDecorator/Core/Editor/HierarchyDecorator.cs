using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Gorgonize.Plugins.HierarchyDecorator.Core.Editor
{
    /// <summary>
    /// Professional AAA-grade Hierarchy Decorator System
    /// Provides advanced visual enhancements for Unity's Hierarchy window
    /// 
    /// Part of Gorgonize Plugins Package
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyDecorator
    {
        #region Constants
        private const string SETTINGS_PATH = "HierarchyDecoratorSettings";
        private const string PACKAGE_NAME = "Gorgonize Hierarchy Decorator";
        private const string VERSION = "1.1.0";
        #endregion

        #region Private Fields
        private static HierarchyDecoratorSettings _settings;
        private static readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<int, CachedObjectData> _objectDataCache = new Dictionary<int, CachedObjectData>();
        private static bool _isInitialized = false;
        #endregion

        #region Initialization
        static HierarchyDecorator()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_isInitialized) return;

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += InvalidateCache;

            // Custom header colors'ý yükle
            LoadCustomHeaderColors();

            _isInitialized = true;

#if GORGONIZE_DEBUG
            Debug.Log($"[{PACKAGE_NAME}] v{VERSION} initialized successfully.");
#endif
        }
        #endregion

        #region Cache System
        private class CachedObjectData
        {
            public Component[] Components { get; set; }
            public string LayerName { get; set; }
            public bool IsHeader { get; set; }
            public string CleanHeaderName { get; set; }
            public List<Texture2D> CachedIcons { get; set; } = new List<Texture2D>();
            public int FrameLastUpdated { get; set; }
        }

        private static void InvalidateCache()
        {
            _objectDataCache.Clear();
            _iconCache.Clear();

#if GORGONIZE_DEBUG
            Debug.Log($"[{PACKAGE_NAME}] Cache invalidated due to hierarchy changes.");
#endif
        }

        private static CachedObjectData GetOrCreateCachedData(GameObject obj, int instanceID)
        {
            if (_objectDataCache.TryGetValue(instanceID, out var cachedData) &&
                cachedData.FrameLastUpdated == Time.frameCount)
            {
                return cachedData;
            }

            cachedData = new CachedObjectData
            {
                Components = obj.GetComponents<Component>(),
                LayerName = LayerMask.LayerToName(obj.layer),
                IsHeader = obj.name.StartsWith(_settings?.HeaderPrefix ?? "---"),
                CleanHeaderName = obj.name.StartsWith(_settings?.HeaderPrefix ?? "---") ?
                    obj.name.Replace(_settings?.HeaderPrefix ?? "---", "").Trim() : obj.name,
                FrameLastUpdated = Time.frameCount
            };

            _objectDataCache[instanceID] = cachedData;
            return cachedData;
        }
        #endregion

        #region Settings Management
        private static void LoadSettings()
        {
            if (_settings != null) return;

            _settings = Resources.Load<HierarchyDecoratorSettings>(SETTINGS_PATH);

            if (_settings == null)
            {
#if GORGONIZE_DEBUG
                Debug.LogWarning($"[{PACKAGE_NAME}] Settings not found at path 'Resources/{SETTINGS_PATH}.asset'.");
#endif
            }
        }

        public static HierarchyDecoratorSettings GetSettings()
        {
            LoadSettings();
            return _settings;
        }
        #endregion

        #region Main Rendering Pipeline
        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (!ShouldProcessObject()) return;

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;

            try
            {
                var renderingContext = new HierarchyRenderingContext
                {
                    GameObject = gameObject,
                    InstanceID = instanceID,
                    SelectionRect = selectionRect,
                    CachedData = GetOrCreateCachedData(gameObject, instanceID)
                };

                RenderHierarchyItem(renderingContext);
            }
            catch (System.Exception ex)
            {
                // Silent error handling
            }
        }

        private static bool ShouldProcessObject()
        {
            LoadSettings();
            return _settings != null && _settings.IsEnabled;
        }

        private static void RenderHierarchyItem(HierarchyRenderingContext context)
        {
            var styleData = CalculateStyleData(context);

            RenderBackground(context, styleData);
            RenderText(context, styleData);
            RenderIcons(context, styleData);
            RenderInteractiveElements(context);
        }
        #endregion

        #region Rendering Context Data Structures
        private class HierarchyRenderingContext
        {
            public GameObject GameObject { get; set; }
            public int InstanceID { get; set; }
            public Rect SelectionRect { get; set; }
            public CachedObjectData CachedData { get; set; }
        }

        private class StyleData
        {
            public Color BackgroundColor { get; set; } = Color.clear;
            public Color TextColor { get; set; }
            public bool HasCustomTextColor { get; set; }
            public bool IsHeader { get; set; }
            public List<Texture2D> Icons { get; set; } = new List<Texture2D>();
            public HeaderStyle HeaderStyle { get; set; }
        }

        private class HeaderStyle
        {
            public Color BackgroundColor { get; set; }
            public Color TextColor { get; set; }
            public FontStyle FontStyle { get; set; }
            public TextAnchor Alignment { get; set; }
            public int FontSize { get; set; }
            public string DisplayName { get; set; }
        }
        #endregion

        #region Style Calculation Engine
        private static StyleData CalculateStyleData(HierarchyRenderingContext context)
        {
            var styleData = new StyleData
            {
                TextColor = GUI.skin.label.normal.textColor,
                IsHeader = context.CachedData.IsHeader
            };

            ApplyTagLayerRules(context, styleData);

            if (styleData.IsHeader)
            {
                ApplyHeaderStyling(context, styleData);
            }

            CollectIcons(context, styleData);
            return styleData;
        }

        private static void ApplyTagLayerRules(HierarchyRenderingContext context, StyleData styleData)
        {
            var tagRule = _settings.TagRules?.FirstOrDefault(r => r.Enabled && r.Tag == context.GameObject.tag);
            if (tagRule != null)
            {
                ApplyStyleRule(tagRule, styleData);
            }

            var layerRule = _settings.LayerRules?.FirstOrDefault(r => r.Enabled && r.LayerName == context.CachedData.LayerName);
            if (layerRule != null)
            {
                ApplyStyleRule(layerRule, styleData);
            }
        }

        private static void ApplyStyleRule(StyleRule rule, StyleData styleData)
        {
            if (rule.BackgroundColor.a > 0)
                styleData.BackgroundColor = rule.BackgroundColor;

            if (rule.OverrideTextColor)
            {
                styleData.TextColor = rule.TextColor;
                styleData.HasCustomTextColor = true;
            }

            if (rule.Icon != null)
                styleData.Icons.Add(rule.Icon);
        }

        private static void ApplyHeaderStyling(HierarchyRenderingContext context, StyleData styleData)
        {
            // Check for custom header color first
            Color headerBgColor = GetCustomHeaderColor(context.GameObject.name);
            if (headerBgColor == Color.clear)
            {
                headerBgColor = _settings.HeaderBackgroundColor;
            }

            styleData.HeaderStyle = new HeaderStyle
            {
                BackgroundColor = headerBgColor,
                TextColor = _settings.HeaderTextColor,
                FontStyle = _settings.HeaderFontStyle,
                Alignment = _settings.HeaderAlignment,
                FontSize = _settings.HeaderFontSize,
                DisplayName = context.CachedData.CleanHeaderName
            };
        }
        #endregion

        #region Header Color Management - Persistent Version
        private static readonly Dictionary<string, Color> _customHeaderColors = new Dictionary<string, Color>();
        private static bool _headerColorsLoaded = false;

        /// <summary>
        /// Load custom header colors from EditorPrefs
        /// </summary>
        private static void LoadCustomHeaderColors()
        {
            if (_headerColorsLoaded) return;

            _customHeaderColors.Clear();

            // EditorPrefs'den kayýtlý header isimlerini al
            string savedHeaders = EditorPrefs.GetString("HierarchyDecorator.CustomHeaderNames", "");

            if (!string.IsNullOrEmpty(savedHeaders))
            {
                string[] headerNames = savedHeaders.Split('|');
                foreach (string headerName in headerNames)
                {
                    if (string.IsNullOrEmpty(headerName)) continue;

                    // Her header için renk bilgisini yükle
                    string colorKey = $"HierarchyDecorator.HeaderColor.{headerName}";
                    string colorString = EditorPrefs.GetString(colorKey, "");

                    if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color color))
                    {
                        _customHeaderColors[headerName] = color;
                    }
                }
            }

            _headerColorsLoaded = true;

#if GORGONIZE_DEBUG
            Debug.Log($"[{PACKAGE_NAME}] Loaded {_customHeaderColors.Count} custom header colors");
#endif
        }

        /// <summary>
        /// Save custom header colors to EditorPrefs
        /// </summary>
        private static void SaveCustomHeaderColors()
        {
            // Header isimlerini birleþtirip kaydet
            string headerNames = string.Join("|", _customHeaderColors.Keys);
            EditorPrefs.SetString("HierarchyDecorator.CustomHeaderNames", headerNames);

            // Her rengi ayrý ayrý kaydet
            foreach (var kvp in _customHeaderColors)
            {
                string colorKey = $"HierarchyDecorator.HeaderColor.{kvp.Key}";
                string colorString = ColorUtility.ToHtmlStringRGBA(kvp.Value);
                EditorPrefs.SetString(colorKey, $"#{colorString}");
            }

#if GORGONIZE_DEBUG
            Debug.Log($"[{PACKAGE_NAME}] Saved {_customHeaderColors.Count} custom header colors");
#endif
        }

        /// <summary>
        /// Get custom header color for specific header name
        /// </summary>
        private static Color GetCustomHeaderColor(string headerName)
        {
            LoadCustomHeaderColors(); // Ýlk çaðrýda renkleri yükle

            string cleanName = headerName.Replace(_settings?.HeaderPrefix ?? "---", "").Trim();
            return _customHeaderColors.TryGetValue(cleanName, out var color) ? color : Color.clear;
        }

        /// <summary>
        /// Set custom color for a header
        /// </summary>
        public static void SetCustomHeaderColor(string headerName, Color color)
        {
            LoadCustomHeaderColors(); // Önce mevcut renkleri yükle

            string cleanName = headerName.Replace(_settings?.HeaderPrefix ?? "---", "").Trim();
            _customHeaderColors[cleanName] = color;

            SaveCustomHeaderColors(); // Deðiþiklikleri kaydet
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// Remove custom color for a header (revert to default)
        /// </summary>
        public static void RemoveCustomHeaderColor(string headerName)
        {
            LoadCustomHeaderColors(); // Önce mevcut renkleri yükle

            string cleanName = headerName.Replace(_settings?.HeaderPrefix ?? "---", "").Trim();

            if (_customHeaderColors.Remove(cleanName))
            {
                // EditorPrefs'den de sil
                string colorKey = $"HierarchyDecorator.HeaderColor.{cleanName}";
                EditorPrefs.DeleteKey(colorKey);

                SaveCustomHeaderColors(); // Deðiþiklikleri kaydet
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        /// <summary>
        /// Clear all custom header colors
        /// </summary>
        public static void ClearAllCustomHeaderColors()
        {
            LoadCustomHeaderColors();

            // Tüm kayýtlý header renklerini EditorPrefs'den sil
            foreach (string headerName in _customHeaderColors.Keys)
            {
                string colorKey = $"HierarchyDecorator.HeaderColor.{headerName}";
                EditorPrefs.DeleteKey(colorKey);
            }

            EditorPrefs.DeleteKey("HierarchyDecorator.CustomHeaderNames");
            _customHeaderColors.Clear();
            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// Get all custom header colors (for UI display)
        /// </summary>
        public static Dictionary<string, Color> GetAllCustomHeaderColors()
        {
            LoadCustomHeaderColors();
            return new Dictionary<string, Color>(_customHeaderColors);
        }
        #endregion

        #region Component Icon System
        private static void CollectIcons(HierarchyRenderingContext context, StyleData styleData)
        {
            CollectCustomComponentIcons(context, styleData);

            if (_settings.ShowAllComponentIcons)
            {
                CollectAutoComponentIcons(context, styleData);
            }
        }

        private static void CollectCustomComponentIcons(HierarchyRenderingContext context, StyleData styleData)
        {
            if (_settings.ComponentIcons == null) return;

            foreach (var rule in _settings.ComponentIcons.Where(r => r.IsValid()))
            {
                var hasComponent = context.CachedData.Components.Any(c =>
                    c != null && c.GetType().Name == rule.ComponentName);

                if (!hasComponent) continue;

                var icon = rule.GetEffectiveIcon();
                if (icon != null && styleData.Icons.Count < _settings.MaxComponentIcons)
                {
                    styleData.Icons.Add(icon);
                }
            }
        }

        private static void CollectAutoComponentIcons(HierarchyRenderingContext context, StyleData styleData)
        {
            var customRuleTypes = new HashSet<string>(
                _settings.ComponentIcons?.Where(r => r.IsValid()).Select(r => r.ComponentName) ??
                System.Linq.Enumerable.Empty<string>()
            );

            foreach (var component in context.CachedData.Components)
            {
                if (component == null) continue;
                if (styleData.Icons.Count >= _settings.MaxComponentIcons) break;

                string componentType = component.GetType().Name;

                if (_settings.ExcludedComponentNames.Contains(componentType) || customRuleTypes.Contains(componentType))
                    continue;

                var icon = GetComponentIcon(component);
                if (icon != null && !styleData.Icons.Contains(icon))
                {
                    styleData.Icons.Add(icon);
                }
            }
        }
        #endregion

        #region Rendering Implementation
        private static void RenderBackground(HierarchyRenderingContext context, StyleData styleData)
        {
            if (Event.current.type != EventType.Repaint) return;

            Color backgroundColorToRender = Color.clear;

            if (styleData.IsHeader && styleData.HeaderStyle != null)
            {
                backgroundColorToRender = styleData.HeaderStyle.BackgroundColor;
            }
            else if (styleData.BackgroundColor.a > 0 && !Selection.Contains(context.InstanceID))
            {
                backgroundColorToRender = styleData.BackgroundColor;
            }

            if (backgroundColorToRender.a > 0)
            {
                EditorGUI.DrawRect(context.SelectionRect, backgroundColorToRender);
            }
        }

        private static void RenderText(HierarchyRenderingContext context, StyleData styleData)
        {
            if (styleData.IsHeader)
            {
                RenderHeaderText(context, styleData);
            }
            else
            {
                RenderNormalText(context, styleData);
            }
        }

        private static void RenderHeaderText(HierarchyRenderingContext context, StyleData styleData)
        {
            if (styleData.HeaderStyle == null) return;

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = styleData.HeaderStyle.TextColor },
                fontStyle = styleData.HeaderStyle.FontStyle,
                alignment = styleData.HeaderStyle.Alignment,
                fontSize = styleData.HeaderStyle.FontSize
            };

            EditorGUI.LabelField(context.SelectionRect, styleData.HeaderStyle.DisplayName, headerStyle);
        }

        private static void RenderNormalText(HierarchyRenderingContext context, StyleData styleData)
        {
            if (!styleData.HasCustomTextColor) return;

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Selection.Contains(context.InstanceID) ? Color.white : styleData.TextColor;

            var labelRect = new Rect(context.SelectionRect)
            {
                x = context.SelectionRect.x + 16,
                width = context.SelectionRect.width - 16
            };

            EditorGUI.LabelField(labelRect, context.GameObject.name, labelStyle);
        }

        private static void RenderIcons(HierarchyRenderingContext context, StyleData styleData)
        {
            if (styleData.Icons.Count == 0) return;

            const float iconSize = 16f;
            const float iconSpacing = 2f;
            float toggleSpace = _settings.ShowActiveToggle ? 22f : 4f;

            float currentX = context.SelectionRect.x + context.SelectionRect.width - toggleSpace;

            for (int i = styleData.Icons.Count - 1; i >= 0; i--)
            {
                currentX -= iconSize;
                var iconRect = new Rect(currentX, context.SelectionRect.y + (context.SelectionRect.height - iconSize) / 2, iconSize, iconSize);
                GUI.DrawTexture(iconRect, styleData.Icons[i], ScaleMode.ScaleToFit);

                if (i > 0) currentX -= iconSpacing;
            }
        }

        private static void RenderInteractiveElements(HierarchyRenderingContext context)
        {
            if (!_settings.ShowActiveToggle) return;

            const float toggleWidth = 18f;
            var toggleRect = new Rect(
                context.SelectionRect.x + context.SelectionRect.width - toggleWidth,
                context.SelectionRect.y,
                toggleWidth,
                context.SelectionRect.height
            );

            var buttonStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            var iconName = context.GameObject.activeSelf ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff";
            var iconContent = EditorGUIUtility.IconContent(iconName);

            if (GUI.Button(toggleRect, iconContent, buttonStyle))
            {
                Undo.RecordObject(context.GameObject, $"Toggle {context.GameObject.name} Active State");
                context.GameObject.SetActive(!context.GameObject.activeSelf);
                EditorUtility.SetDirty(context.GameObject);
            }
        }
        #endregion

        #region Icon Management Utilities
        private static Texture2D GetComponentIcon(Component component)
        {
            if (component == null) return null;

            string typeName = component.GetType().FullName;
            if (_iconCache.TryGetValue(typeName, out var cachedIcon))
            {
                return cachedIcon;
            }

            var content = EditorGUIUtility.ObjectContent(component, component.GetType());
            var icon = content?.image as Texture2D;

            if (icon != null && icon.name.ToLower() == "cs script icon")
            {
                var scriptAsset = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
                if (scriptAsset != null)
                {
                    var scriptIcon = AssetPreview.GetMiniThumbnail(scriptAsset);
                    if (scriptIcon != null && scriptIcon.name.ToLower() != "cs script icon")
                    {
                        icon = scriptIcon;
                    }
                }
            }

            _iconCache[typeName] = icon;
            return icon;
        }
        #endregion

        #region Public API
        public static void RefreshDecorator()
        {
            InvalidateCache();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static bool IsActive => _settings != null && _settings.IsEnabled;

        public static string GetVersion() => VERSION;
        #endregion
    }

    #region Header Editor Menu Items
    /// <summary>
    /// Header editing functionality through context menus
    /// </summary>
    public static class HierarchyHeaderEditor
    {
        [MenuItem("GameObject/Gorgonize/Set Header Color", false, 0)]
        private static void SetHeaderColor()
        {
            if (Selection.activeGameObject == null) return;

            var settings = HierarchyDecorator.GetSettings();
            if (settings == null) return;

            var obj = Selection.activeGameObject;
            if (!obj.name.StartsWith(settings.HeaderPrefix))
            {
                EditorUtility.DisplayDialog("Header Color",
                    $"Selected object is not a header. Headers must start with '{settings.HeaderPrefix}'",
                    "OK");
                return;
            }

            HeaderColorPickerWindow.ShowWindow(obj);
        }

        [MenuItem("GameObject/Gorgonize/Set Header Color", true)]
        private static bool ValidateSetHeaderColor()
        {
            if (Selection.activeGameObject == null) return false;

            var settings = HierarchyDecorator.GetSettings();
            if (settings == null) return false;

            return Selection.activeGameObject.name.StartsWith(settings.HeaderPrefix);
        }

        [MenuItem("GameObject/Gorgonize/Reset Header Color", false, 1)]
        private static void ResetHeaderColor()
        {
            if (Selection.activeGameObject == null) return;

            HierarchyDecorator.RemoveCustomHeaderColor(Selection.activeGameObject.name);
        }

        [MenuItem("GameObject/Gorgonize/Reset Header Color", true)]
        private static bool ValidateResetHeaderColor()
        {
            return ValidateSetHeaderColor();
        }

        [MenuItem("Tools/Gorgonize/Header Color Manager", false, 100)]
        private static void OpenHeaderColorManager()
        {
            HeaderColorManagerWindow.ShowWindow();
        }
    }

    #region Header Color Picker Window
    /// <summary>
    /// Simple color picker window for individual headers
    /// </summary>
    public class HeaderColorPickerWindow : EditorWindow
    {
        private static GameObject _targetObject;
        private Color _selectedColor = Color.white;
        private bool _useCustomColor = true;

        public static void ShowWindow(GameObject target)
        {
            _targetObject = target;
            var window = GetWindow<HeaderColorPickerWindow>("Header Color");
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnGUI()
        {
            if (_targetObject == null)
            {
                EditorGUILayout.HelpBox("No target object selected", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField($"Header: {_targetObject.name}", EditorStyles.boldLabel);

            GUILayout.Space(10);

            _useCustomColor = EditorGUILayout.Toggle("Use Custom Color", _useCustomColor);

            if (_useCustomColor)
            {
                _selectedColor = EditorGUILayout.ColorField("Header Color", _selectedColor);
            }

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply"))
            {
                if (_useCustomColor)
                {
                    HierarchyDecorator.SetCustomHeaderColor(_targetObject.name, _selectedColor);
                }
                else
                {
                    HierarchyDecorator.RemoveCustomHeaderColor(_targetObject.name);
                }
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion

    #region Header Color Manager Window
    /// <summary>
    /// Advanced window to manage all header colors at once
    /// </summary>
    public class HeaderColorManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<GameObject> _headerObjects = new List<GameObject>();

        public static void ShowWindow()
        {
            var window = GetWindow<HeaderColorManagerWindow>("Header Manager");
            window.minSize = new Vector2(400, 300);
            window.Show();
            window.RefreshHeaderList();
        }

        private void OnGUI()
        {
            var settings = HierarchyDecorator.GetSettings();
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Hierarchy Decorator Settings not found!", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Header Color Manager", EditorStyles.boldLabel);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Headers"))
            {
                RefreshHeaderList();
            }
            if (GUILayout.Button("Reset All Colors"))
            {
                if (EditorUtility.DisplayDialog("Reset All", "Reset all custom header colors to default?", "Yes", "No"))
                {
                    ResetAllHeaderColors();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var header in _headerObjects)
            {
                if (header == null) continue;

                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                EditorGUILayout.LabelField(header.name, GUILayout.Width(200));

                if (GUILayout.Button("Edit Color", GUILayout.Width(80)))
                {
                    HeaderColorPickerWindow.ShowWindow(header);
                }

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = header;
                    EditorGUIUtility.PingObject(header);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshHeaderList()
        {
            _headerObjects.Clear();
            var settings = HierarchyDecorator.GetSettings();
            if (settings == null) return;

            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.StartsWith(settings.HeaderPrefix))
                {
                    _headerObjects.Add(obj);
                }
            }

            _headerObjects.Sort((a, b) => string.Compare(a.name, b.name));
        }

        private void ResetAllHeaderColors()
        {
            // Tüm custom renkleri temizle
            HierarchyDecorator.ClearAllCustomHeaderColors();
            Repaint();
        }
    }
    #endregion
    #endregion
}