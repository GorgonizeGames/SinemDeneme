using UnityEngine;
using UnityEditor;
using System.IO;

namespace Gorgonize.Plugins.HierarchyDecorator.Core.Editor
{
    /// <summary>
    /// Project Settings integration for Hierarchy Decorator
    /// Provides easy access to settings through Unity's Project Settings window
    /// </summary>
    public class HierarchyDecoratorSettingsProvider : SettingsProvider
    {
        #region Constants
        private const string SETTINGS_PATH = "Project/Gorgonize/Hierarchy Decorator";
        private const string SETTINGS_RESOURCE_PATH = "HierarchyDecoratorSettings";
        private const string PACKAGE_FOLDER = "Assets/Plugins/Gorgonize/HierarchyDecorator";
        private const string RESOURCES_FOLDER = PACKAGE_FOLDER + "/Resources";
        private const string FULL_SETTINGS_PATH = RESOURCES_FOLDER + "/HierarchyDecoratorSettings.asset";
        #endregion

        #region Private Fields
        private HierarchyDecoratorSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;
        #endregion

        #region Constructor
        public HierarchyDecoratorSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope)
        {
            keywords = new[] { "gorgonize", "hierarchy", "decorator", "icons", "styling" };
        }
        #endregion

        #region Settings Provider Registration
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new HierarchyDecoratorSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
            return provider;
        }
        #endregion

        #region Lifecycle Methods
        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            LoadOrCreateSettings();
        }

        public override void OnGUI(string searchContext)
        {
            DrawGUI();
        }

        public override void OnDeactivate()
        {
            if (_serializedSettings != null && _serializedSettings.hasModifiedProperties)
            {
                _serializedSettings.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }
        #endregion

        #region Settings Management
        private void LoadOrCreateSettings()
        {
            _settings = Resources.Load<HierarchyDecoratorSettings>(SETTINGS_RESOURCE_PATH);
            
            if (_settings == null)
            {
                // Try to find existing settings in the project
                var guids = AssetDatabase.FindAssets("t:HierarchyDecoratorSettings");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settings = AssetDatabase.LoadAssetAtPath<HierarchyDecoratorSettings>(path);
                }
            }

            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        private void CreateNewSettings()
        {
            // Ensure directories exist
            if (!Directory.Exists(PACKAGE_FOLDER))
                Directory.CreateDirectory(PACKAGE_FOLDER);
            if (!Directory.Exists(RESOURCES_FOLDER))
                Directory.CreateDirectory(RESOURCES_FOLDER);

            // Create new settings asset
            _settings = ScriptableObject.CreateInstance<HierarchyDecoratorSettings>();
            AssetDatabase.CreateAsset(_settings, FULL_SETTINGS_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _serializedSettings = new SerializedObject(_settings);
            
            Debug.Log($"[Gorgonize Hierarchy Decorator] Created new settings at: {FULL_SETTINGS_PATH}");
        }
        #endregion

        #region GUI Drawing
        private void DrawGUI()
        {
            EditorGUILayout.Space(10);
            
            // Header
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            if (_settings == null)
            {
                DrawNoSettingsGUI();
                return;
            }

            // Settings content
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            if (_serializedSettings != null)
            {
                _serializedSettings.Update();
                DrawSettingsGUI();
                if (_serializedSettings.ApplyModifiedProperties())
                {
                    HierarchyDecorator.RefreshDecorator();
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            DrawFooter();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Gorgonize Hierarchy Decorator", headerStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Professional Hierarchy Enhancement System", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawNoSettingsGUI()
        {
            EditorGUILayout.HelpBox(
                "No Hierarchy Decorator settings found!\n\n" +
                "Settings should be located at:\n" +
                FULL_SETTINGS_PATH + "\n\n" +
                "Click the button below to create default settings.",
                MessageType.Warning
            );
            
            EditorGUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Create Default Settings", GUILayout.Height(30), GUILayout.Width(200)))
            {
                CreateNewSettings();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "Alternative: Create settings manually via:\n" +
                "Assets > Create > Gorgonize > Hierarchy Decorator Settings\n\n" +
                "Then place the created asset in the Resources folder.",
                MessageType.Info
            );
        }

        private void DrawSettingsGUI()
        {
            // Status indicator
            DrawStatusIndicator();
            
            EditorGUILayout.Space(10);
            
            // Quick actions
            DrawQuickActions();
            
            EditorGUILayout.Space(15);
            
            // Settings properties
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Draw all properties except the package info (we handle that separately)
            var property = _serializedSettings.GetIterator();
            property.NextVisible(true); // Skip script reference
            
            while (property.NextVisible(false))
            {
                if (property.name == "PackageInfo") continue; // Skip package info
                
                EditorGUILayout.PropertyField(property, true);
            }
        }

        private void DrawStatusIndicator()
        {
            var isActive = HierarchyDecorator.IsActive;
            var statusColor = isActive ? Color.green : Color.red;
            var statusText = isActive ? "ACTIVE" : "DISABLED";
            
            var originalColor = GUI.color;
            GUI.color = statusColor;
            
            var statusStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            
            EditorGUILayout.LabelField($"Status: {statusText}", statusStyle);
            GUI.color = originalColor;
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Hierarchy", GUILayout.Height(25)))
            {
                HierarchyDecorator.RefreshDecorator();
                EditorApplication.RepaintHierarchyWindow();
            }
            
            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Settings",
                    "Are you sure you want to reset all settings to default values?\nThis action cannot be undone.",
                    "Reset",
                    "Cancel"))
                {
                    _settings.ResetToDefaults();
                    _serializedSettings.Update();
                    HierarchyDecorator.RefreshDecorator();
                }
            }
            
            if (GUILayout.Button("Clear Icon Cache", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Icon Cache",
                    "This will clear the icon cache and refresh the hierarchy.\nUseful for troubleshooting icon issues.",
                    "Clear",
                    "Cancel"))
                {
                    HierarchyDecorator.RefreshDecorator();
                    //ShowNotification(new GUIContent("Icon cache cleared!"));
                }
            }
            
            if (GUILayout.Button("Open in Inspector", GUILayout.Height(25)))
            {
                Selection.activeObject = _settings;
                EditorGUIUtility.PingObject(_settings);
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(5);
            
            var footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 10
            };
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Version {HierarchyDecorator.GetVersion()} | Gorgonize Plugins", footerStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            if (_settings != null)
            {
                var settingsPath = AssetDatabase.GetAssetPath(_settings);
                EditorGUILayout.LabelField($"Settings Path: {settingsPath}", footerStyle);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Safe icon loading enabled - No more icon errors!", footerStyle);
        }
        #endregion
    }
}