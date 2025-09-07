using Game.Runtime.Character.Data;
using UnityEngine;

namespace Game.Runtime.Character.Settings
{
    // Runtime state için ayrı class
    public class CharacterRuntimeSettings
    {
        private CharacterData _baseSettings;
        private float _speedMultiplier = 1f;
        private float _boostEndTime = 0f;

        public CharacterRuntimeSettings(CharacterData baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public float EffectiveMoveSpeed
        {
            get
            {
                UpdateBoost();
                return _baseSettings.MoveSpeed * _speedMultiplier;
            }
        }

        public float RotationSpeed => _baseSettings.RotationSpeed;
        public float Acceleration => _baseSettings.Acceleration;
        public float AnimationSpeedMultiplier => _baseSettings.AnimationSpeedMultiplier;
        public bool UseRootMotion => _baseSettings.UseRootMotion;
        public bool FreezeYPosition => _baseSettings.FreezeYPosition;
        public float Drag => _baseSettings.Drag;

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _speedMultiplier = multiplier;
            _boostEndTime = Time.time + duration;
        }

        public void ResetSpeedBoost()
        {
            _speedMultiplier = 1f;
            _boostEndTime = 0f;
        }

        private void UpdateBoost()
        {
            if (_boostEndTime > 0 && Time.time > _boostEndTime)
            {
                ResetSpeedBoost();
            }
        }

        public void ChangeBaseSettings(CharacterData newSettings)
        {
            _baseSettings = newSettings;
        }
    }
}
