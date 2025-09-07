using Game.Runtime.Character.Animation;
using Game.Runtime.Character.Motor;
using UnityEngine;

namespace Game.Runtime.Character.Animation
{
    /// <summary>
    /// Oyunumuz için gerekli animasyon parametreleri
    /// Sadece Idle, Walk ve Carrying animasyonları var
    /// </summary>
    public static class AnimationParameters
    {
        // Base Layer Parameters (Alt gövde - yürüme)
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");

        // Upper Body Layer Parameters (Üst gövde - taşıma)
        public static readonly int IsCarrying = Animator.StringToHash("IsCarrying");
    }

    /// <summary>
    /// Animator Layer indices
    /// </summary>
    public static class AnimationLayers
    {
        public const int BaseLayer = 0;      // Yürüme ve idle
        public const int UpperBodyLayer = 1; // Taşıma durumu
    }
}