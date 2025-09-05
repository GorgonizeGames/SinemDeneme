using System.Collections.Generic;
using UnityEngine;

namespace Gorgonize.Plugins.HierarchyDecorator.Core.Editor
{
    /// <summary>
    /// Professional Hierarchy Decorator Settings
    /// Advanced configuration system for Unity Hierarchy window customization
    /// 
    /// Part of Gorgonize Plugins Package
    /// 
    /// Usage: 
    /// 1. Create via "Assets > Create > Gorgonize > Hierarchy Decorator Settings"
    /// 2. Place in "Plugins/GGorgonize/HierarchyDecorator/Resources/" folder
    /// 3. Configure via Project Settings > Gorgonize > Hierarchy Decorator
    /// </summary>
    [CreateAssetMenu(
        fileName = "HierarchyDecoratorSettings",
        menuName = "Gorgonize/Hierarchy Decorator Settings",
        order = 100
    )]
    public class HierarchyDecoratorSettings : ScriptableObject
    {
        #region System Information
        [Header("╔══════════ SYSTEM INFO ══════════")]
        [Space(5)]
        [TextArea(2, 3)]
        [Tooltip("Package information and instructions")]
        public string PackageInfo = "Gorgonize Hierarchy Decorator v1.1.0\nPlace this file in: Plugins/Gorgonize/HierarchyDecorator/Resources/";
        #endregion

        #region Master Control
        [Header("╔══════════ MASTER CONTROL ══════════")]
        [Tooltip("Master switch to enable/disable all Hierarchy Decorator functionality")]
        public bool IsEnabled = true;
        #endregion

        #region Header System
        [Header("╔══════════ HEADER SEPARATORS ══════════")]
        [Tooltip("GameObjects starting with this prefix will be styled as section headers")]
        [Space(5)]
        public string HeaderPrefix = "---";

        [Space(10)]
        [Header("Header Styling")]
        [Tooltip("Background color for header objects")]
        [ColorUsage(true, false)]
        public Color HeaderBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        [Space(5)]
        [Header("Header Text Styling")]
        [ColorUsage(true, false)]
        public Color HeaderTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        [Tooltip("Font style for header text")]
        public FontStyle HeaderFontStyle = FontStyle.Bold;

        [Tooltip("Font size for header text")]
        [Range(8, 20)]
        public int HeaderFontSize = 12;

        [Tooltip("Text alignment for headers")]
        public TextAnchor HeaderAlignment = TextAnchor.MiddleCenter;
        #endregion

        #region Interactive Elements
        [Header("╔══════════ INTERACTIVE ELEMENTS ══════════")]
        [Tooltip("Adds clickable eye icon to toggle GameObject active state directly in hierarchy")]
        public bool ShowActiveToggle = true;
        #endregion

        #region Component Icon System
        [Header("╔══════════ COMPONENT ICONS ══════════")]
        [Tooltip("Automatically displays icons for all components on GameObjects")]
        public bool ShowAllComponentIcons = true;

        [Space(5)]
        [Tooltip("Maximum number of component icons to display (prevents UI clutter)")]
        [Range(1, 15)]
        public int MaxComponentIcons = 10;

        [Space(5)]
        [Tooltip("Component types to hide from automatic icon display")]
        public List<string> ExcludedComponentNames = new List<string>
        { 
            // Varsayılan Unity component'leri
            "Transform",
            "CanvasRenderer",

            // URP/HDRP component'leri (Hataları önlemek için varsayılan olarak eklendi)
            "UniversalAdditionalCameraData",
            "UniversalAdditionalLightData",
            "Volume"
        };
        #endregion

        #region Style Rules
        [Header("╔══════════ TAG & LAYER RULES ══════════")]
        [Space(5)]
        [Tooltip("Apply custom styling based on GameObject tags")]
        public List<TagStyleRule> TagRules = new List<TagStyleRule>();

        [Space(10)]
        [Tooltip("Apply custom styling based on GameObject layers")]
        public List<LayerStyleRule> LayerRules = new List<LayerStyleRule>();
        #endregion

        #region Component Icon Overrides
        [Header("╔══════════ COMPONENT ICON OVERRIDES ══════════")]
        [Tooltip("Advanced: Override default component icons with custom ones or change their positioning")]
        [Space(5)]
        public List<ComponentIconRule> ComponentIcons = new List<ComponentIconRule>();
        #endregion

        #region Hidden Legacy Fields
        [HideInInspector] public bool UseRowStriping = false;
        [HideInInspector] public Color EvenRowColor = Color.clear;
        [HideInInspector] public Color OddRowColor = Color.clear;
        #endregion

        #region Validation & Initialization
        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            HeaderFontSize = Mathf.Clamp(HeaderFontSize, 8, 20);
            MaxComponentIcons = Mathf.Clamp(MaxComponentIcons, 1, 15);

            // Ensure essential excluded components exist
            if (ExcludedComponentNames == null)
                ExcludedComponentNames = new List<string>();

            if (!ExcludedComponentNames.Contains("Transform"))
                ExcludedComponentNames.Add("Transform");

            if (!ExcludedComponentNames.Contains("CanvasRenderer"))
                ExcludedComponentNames.Add("CanvasRenderer");

            // Initialize component icons list if null
            if (ComponentIcons == null)
                ComponentIcons = new List<ComponentIconRule>();

            // Initialize safe component icons only if list is empty
            if (ComponentIcons.Count == 0)
            {
                InitializeSafeComponentIcons();
            }

            // Validate header prefix
            if (string.IsNullOrEmpty(HeaderPrefix))
                HeaderPrefix = "---";
        }

        private void OnEnable()
        {
            // Update package info with current settings path
#if UNITY_EDITOR
            PackageInfo = "Gorgonize Hierarchy Decorator v1.1.0\n" +
                         "Place this file in: Plugins/Gorgonize/HierarchyDecorator/Resources/\n" +
                         $"Current asset path: {UnityEditor.AssetDatabase.GetAssetPath(this)}";
#endif
        }

        /// <summary>
        /// Initialize only the most commonly available Unity component icons
        /// These are tested to work across Unity versions
        /// </summary>
        private void InitializeSafeComponentIcons()
        {
            ComponentIcons = new List<ComponentIconRule>
            {
                // Core Unity Components - More robust icon names
                new ComponentIconRule("Camera", "Camera Icon", null, IconAlignment.Right, "Main scene camera"),
                new ComponentIconRule("Light", "Light Icon", null, IconAlignment.Right, "Scene lighting"),
                new ComponentIconRule("AudioSource", "AudioSource Icon", null, IconAlignment.Right, "Audio playback"),
                
                // 3D Physics - Basic colliders
                new ComponentIconRule("Rigidbody", "Rigidbody Icon", null, IconAlignment.Left, "3D physics body"),
                new ComponentIconRule("BoxCollider", "BoxCollider Icon", null, IconAlignment.Left, "Box collision shape"),
                new ComponentIconRule("SphereCollider", "SphereCollider Icon", null, IconAlignment.Left, "Sphere collision shape"),
                new ComponentIconRule("CapsuleCollider", "CapsuleCollider Icon", null, IconAlignment.Left, "Capsule collision shape"),
                
                // 2D Physics - Basic colliders
                new ComponentIconRule("Rigidbody2D", "Rigidbody2D Icon", null, IconAlignment.Left, "2D physics body"),
                new ComponentIconRule("BoxCollider2D", "BoxCollider2D Icon", null, IconAlignment.Left, "2D box collider"),
                new ComponentIconRule("CircleCollider2D", "CircleCollider2D Icon", null, IconAlignment.Left, "2D circle collider"),

                // Rendering - Essential components
                new ComponentIconRule("MeshRenderer", "MeshRenderer Icon", null, IconAlignment.Left, "3D mesh rendering"),
                new ComponentIconRule("SpriteRenderer", "SpriteRenderer Icon", null, IconAlignment.Left, "2D sprite rendering"),
                new ComponentIconRule("ParticleSystem", "ParticleSystem Icon", null, IconAlignment.Left, "Particle effects"),

                // Animation
                new ComponentIconRule("Animator", "Animator Icon", null, IconAlignment.Left, "Animation controller"),

                // UI - Core components
                new ComponentIconRule("Canvas", "Canvas Icon", null, IconAlignment.Right, "UI canvas"),
                new ComponentIconRule("Image", "Image Icon", null, IconAlignment.Left, "UI image"),
                new ComponentIconRule("Text", "Text Icon", null, IconAlignment.Left, "UI text"),
                new ComponentIconRule("Button", "Button Icon", null, IconAlignment.Left, "UI button")
            };
        }

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        [ContextMenu("Reset to Default Values")]
        public void ResetToDefaults()
        {
            IsEnabled = true;

            HeaderPrefix = "---";
            HeaderBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            HeaderTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            HeaderFontStyle = FontStyle.Bold;
            HeaderFontSize = 12;
            HeaderAlignment = TextAnchor.MiddleCenter;
            ShowActiveToggle = true;
            ShowAllComponentIcons = true;
            MaxComponentIcons = 10;

            ExcludedComponentNames = new List<string>
            {
                "Transform",
                "CanvasRenderer",
                "UniversalAdditionalCameraData",
                "UniversalAdditionalLightData",
                "Volume"
            };

            TagRules = new List<TagStyleRule>();
            LayerRules = new List<LayerStyleRule>();

            // Reset to safe component icons
            InitializeSafeComponentIcons();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Add a safe component icon rule (validates icon exists before adding)
        /// </summary>
        public void AddSafeComponentIcon(string componentName, string iconName, IconAlignment alignment = IconAlignment.Left, string description = "")
        {
#if UNITY_EDITOR
            // Test if the icon exists before adding
            var iconContent = UnityEditor.EditorGUIUtility.IconContent(iconName);
            if (iconContent?.image != null)
            {
                if (ComponentIcons == null) ComponentIcons = new List<ComponentIconRule>();

                // Check if rule already exists
                var existingRule = ComponentIcons.Find(r => r.ComponentName == componentName);
                if (existingRule != null)
                {
                    // Update existing rule
                    existingRule.BuiltInIconName = iconName;
                    existingRule.Alignment = alignment;
                    existingRule.Description = description;
                }
                else
                {
                    // Add new rule
                    ComponentIcons.Add(new ComponentIconRule(componentName, iconName, null, alignment, description));
                }

                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// Defines where component icons should be positioned relative to GameObject names
    /// </summary>
    public enum IconAlignment
    {
        [Tooltip("Icons appear on the left side of the GameObject name")]
        Left,

        [Tooltip("Icons appear on the right side of the GameObject name")]
        Right
    }
    #endregion

    #region Style Rules Base Classes
    /// <summary>
    /// Base class for all styling rules - provides common styling properties
    /// Implements consistent behavior across different rule types
    /// </summary>
    [System.Serializable]
    public abstract class StyleRule
    {
        [Header("Rule Configuration")]
        [Tooltip("Enable/disable this specific styling rule")]
        public bool Enabled = true;

        [Header("Icon Settings")]
        [Tooltip("Custom icon to display next to matching GameObjects")]
        public Texture2D Icon;

        [Tooltip("Where to position the icon relative to the GameObject name")]
        public IconAlignment Alignment = IconAlignment.Right;

        [Header("Background Styling")]
        [Tooltip("Background color for the entire row (transparent = no background)")]
        [ColorUsage(true, false)]
        public Color BackgroundColor = new Color(0, 0, 0, 0);

        [Header("Text Styling")]
        [Tooltip("Override the default text color for GameObject names")]
        public bool OverrideTextColor = false;

        [Tooltip("Custom text color (only used if Override Text Color is enabled)")]
        [ColorUsage(true, false)]
        public Color TextColor = Color.white;

        /// <summary>
        /// Validate rule settings
        /// </summary>
        public virtual bool IsValid()
        {
            return Enabled;
        }
    }
    #endregion

    #region Specific Style Rule Types
    /// <summary>
    /// Style rule that matches GameObjects by their assigned tag
    /// Useful for categorizing and styling objects by gameplay purpose
    /// </summary>
    [System.Serializable]
    public class TagStyleRule : StyleRule
    {
        [Header("Tag Matching")]
        [Tooltip("The exact tag name to match (case-sensitive)\nExample: 'Player', 'Enemy', 'Collectible'")]
        public string Tag;

        [Space(5)]
        [TextArea(2, 3)]
        [Tooltip("Optional description of what this rule does")]
        public string Description;

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrEmpty(Tag);
        }
    }

    /// <summary>
    /// Style rule that matches GameObjects by their assigned layer
    /// Useful for styling objects by rendering or physics layer
    /// </summary>
    [System.Serializable]
    public class LayerStyleRule : StyleRule
    {
        [Header("Layer Matching")]
        [Tooltip("The exact layer name to match (case-sensitive)\nExample: 'UI', 'Ground', 'Player'")]
        public string LayerName;

        [Space(5)]
        [TextArea(2, 3)]
        [Tooltip("Optional description of what this rule does")]
        public string Description;

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrEmpty(LayerName);
        }
    }
    #endregion

    #region Component Icon Rule
    /// <summary>
    /// Advanced rule for customizing component icon display
    /// Allows overriding default Unity component icons with custom ones
    /// and controlling their positioning in the hierarchy
    /// </summary>
    [System.Serializable]
    public class ComponentIconRule
    {
        [Header("Component Identification")]
        [Tooltip("Enable/disable this component icon rule")]
        public bool Enabled = true;

        [Tooltip("Exact component class name (e.g., 'Camera', 'Rigidbody', 'YourCustomScript')\n" +
                "Case-sensitive! Must match exactly with the component's class name.")]
        public string ComponentName;

        [Header("Icon Configuration")]
        [Tooltip("Unity built-in icon name (e.g., 'Camera Icon', 'Rigidbody Icon')\n" +
                "Leave empty if using custom icon. Check Unity's Icon Browser for available names.")]
        public string BuiltInIconName;

        [Tooltip("Custom icon texture. Overrides built-in icon if assigned.\n" +
                "Recommended size: 16x16 pixels for best results.")]
        public Texture2D CustomIcon;

        [Tooltip("Where to position this component's icon in the hierarchy")]
        public IconAlignment Alignment = IconAlignment.Left;

        [Header("Documentation")]
        [Tooltip("Optional description of what this component does (for documentation purposes)")]
        public string Description;

        #region Constructors
        public ComponentIconRule()
        {
            Enabled = true;
        }

        public ComponentIconRule(string componentName, string builtInIconName, Texture2D customIcon, IconAlignment alignment)
        {
            Enabled = true;
            ComponentName = componentName;
            BuiltInIconName = builtInIconName;
            CustomIcon = customIcon;
            Alignment = alignment;
        }

        public ComponentIconRule(string componentName, string builtInIconName, Texture2D customIcon, IconAlignment alignment, string description)
            : this(componentName, builtInIconName, customIcon, alignment)
        {
            Description = description;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Check if this rule is properly configured
        /// </summary>
        public bool IsValid()
        {
            return Enabled &&
                   !string.IsNullOrEmpty(ComponentName) &&
                   (!string.IsNullOrEmpty(BuiltInIconName) || CustomIcon != null);
        }

        /// <summary>
        /// Get the effective icon for this rule (custom takes priority)
        /// </summary>
        public Texture2D GetEffectiveIcon()
        {
#if UNITY_EDITOR
            if (CustomIcon != null) return CustomIcon;

            try
            {
                var iconContent = UnityEditor.EditorGUIUtility.IconContent(BuiltInIconName);
                return iconContent?.image as Texture2D;
            }
            catch (System.Exception)
            {
                return null;
            }
#else
            return CustomIcon;
#endif
        }
        #endregion
    }
    #endregion    
}
