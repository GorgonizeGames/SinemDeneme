using UnityEngine;

namespace Game.Runtime.Character.Animation
{
    /// <summary>
    /// Oyunumuz için gerekli animasyon parametreleri
    /// Layer-based sistem: Base Layer (hareket) + Upper Body Layer (taşıma)
    /// </summary>
    public static class AnimationParameters
    {
        // Parameter names as constants
        private const string SPEED_PARAM = "Speed";
        private const string IS_MOVING_PARAM = "IsMoving";
        private const string IS_CARRYING_PARAM = "IsCarrying";

        // Base Layer Parameters (Alt gövde - yürüme/idle)
        public static readonly int Speed = Animator.StringToHash(SPEED_PARAM);
        public static readonly int IsMoving = Animator.StringToHash(IS_MOVING_PARAM);

        // Upper Body Layer Parameters (Üst gövde - taşıma durumu)
        public static readonly int IsCarrying = Animator.StringToHash(IS_CARRYING_PARAM);

        // Parameter validation
        public static bool IsValidParameter(string parameterName)
        {
            return parameterName == SPEED_PARAM || 
                   parameterName == IS_MOVING_PARAM || 
                   parameterName == IS_CARRYING_PARAM;
        }

        // Get parameter name by hash (debugging purposes)
        public static string GetParameterName(int hash)
        {
            if (hash == Speed) return SPEED_PARAM;
            if (hash == IsMoving) return IS_MOVING_PARAM;
            if (hash == IsCarrying) return IS_CARRYING_PARAM;
            return "Unknown Parameter";
        }
    }

    /// <summary>
    /// Animator Layer indices ve layer management
    /// </summary>
    public static class AnimationLayers
    {
        public const int BaseLayer = 0;      // Yürüme ve idle (tüm vücut)
        public const int UpperBodyLayer = 1; // Taşıma durumu (sadece üst gövde)

        // Layer weights
        public const float DISABLED_WEIGHT = 0f;
        public const float ENABLED_WEIGHT = 1f;

        // Layer validation
        public static bool IsValidLayer(int layerIndex)
        {
            return layerIndex >= 0 && layerIndex <= 1;
        }

        // Layer names for debugging
        public static string GetLayerName(int layerIndex)
        {
            return layerIndex switch
            {
                BaseLayer => "Base Layer (Movement)",
                UpperBodyLayer => "Upper Body Layer (Carrying)", 
                _ => "Unknown Layer"
            };
        }
    }

    /// <summary>
    /// Base layer state names (sadece hareket için)
    /// </summary>
    public static class BaseLayerStates
    {
        // State names - sadece base layer için
        public const string IDLE_STATE = "Idle";
        public const string WALK_STATE = "Walk";

        // State hashes
        public static readonly int IdleHash = Animator.StringToHash(IDLE_STATE);
        public static readonly int WalkHash = Animator.StringToHash(WALK_STATE);

        public static string GetStateName(int hash)
        {
            if (hash == IdleHash) return IDLE_STATE;
            if (hash == WalkHash) return WALK_STATE;
            return "Unknown State";
        }
    }

    /// <summary>
    /// Upper body layer state names (taşıma için)
    /// </summary>
    public static class UpperBodyLayerStates
    {
        // Upper body layer state'leri
        public const string HANDS_FREE_STATE = "HandsFree";
        public const string CARRYING_STATE = "Carrying";

        // State hashes
        public static readonly int HandsFreeHash = Animator.StringToHash(HANDS_FREE_STATE);
        public static readonly int CarryingHash = Animator.StringToHash(CARRYING_STATE);

        public static string GetStateName(int hash)
        {
            if (hash == HandsFreeHash) return HANDS_FREE_STATE;
            if (hash == CarryingHash) return CARRYING_STATE;
            return "Unknown Upper Body State";
        }
    }

    /// <summary>
    /// Animation system helper methods
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Set carrying state on animator
        /// </summary>
        public static void SetCarryingState(Animator animator, bool isCarrying)
        {
            if (animator == null) return;

            // Set the IsCarrying parameter
            animator.SetBool(AnimationParameters.IsCarrying, isCarrying);
            
            // Set upper body layer weight
            float weight = isCarrying ? AnimationLayers.ENABLED_WEIGHT : AnimationLayers.DISABLED_WEIGHT;
            animator.SetLayerWeight(AnimationLayers.UpperBodyLayer, weight);
        }

        /// <summary>
        /// Set movement state on animator
        /// </summary>
        public static void SetMovementState(Animator animator, float speed, bool isMoving)
        {
            if (animator == null) return;

            animator.SetFloat(AnimationParameters.Speed, speed);
            animator.SetBool(AnimationParameters.IsMoving, isMoving);
        }

        /// <summary>
        /// Get current animation info for debugging
        /// </summary>
        public static string GetAnimationDebugInfo(Animator animator)
        {
            if (animator == null) return "Animator is null";

            var speed = animator.GetFloat(AnimationParameters.Speed);
            var isMoving = animator.GetBool(AnimationParameters.IsMoving);
            var isCarrying = animator.GetBool(AnimationParameters.IsCarrying);
            var upperBodyWeight = animator.GetLayerWeight(AnimationLayers.UpperBodyLayer);

            return $"Speed: {speed:F2}, Moving: {isMoving}, Carrying: {isCarrying}, UpperBodyWeight: {upperBodyWeight:F2}";
        }
    }
}