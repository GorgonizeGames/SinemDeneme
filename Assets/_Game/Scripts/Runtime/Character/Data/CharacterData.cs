using DG.Tweening;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Core.Data;
using UnityEngine;

namespace Game.Runtime.Character.Data
{
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
    public class CharacterData : BaseDataModel
    {
        [Header("Character Info")]
        [SerializeField] private CharacterType _characterType;

        [Header("Visuals")]
        [SerializeField] private Sprite _characterSprite;

        [Header("Movement & Physics")]
        [SerializeField, Range(1f, 10f)] private float _moveSpeed = 5f;
        [SerializeField, Range(1f, 20f)] private float _rotationSpeed = 15f;
        [SerializeField, Range(5f, 20f)] private float _acceleration = 10f;
        [SerializeField, Range(0f, 1f)] private float _drag = 0.1f;
        [SerializeField] private bool _freezeYPosition = true;

        [Header("Item Handling")]
        [SerializeField, Range(1f, 10f)] private float _itemCapacity = 5f;
        [SerializeField] private float _takeItemFromMachineDuration = 1f;
        [SerializeField] private float _takeItemJumpPower = 1f;
        [SerializeField] private float _takeItemDuration = 1f;
        [SerializeField] private Ease _takeItemEase = Ease.OutFlash;

        [Header("Animation")]
        [SerializeField, Range(0.1f, 2f)] private float _animationSpeedMultiplier = 1f;
        [SerializeField] private bool _useRootMotion = false;

        // --- PUBLIC PROPERTIES (READ-ONLY) ---
        
        // Character Info
        public CharacterType CharacterType => _characterType;

        // Visuals
        public Sprite CharacterSprite => _characterSprite;
        
        // Movement & Physics
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float Acceleration => _acceleration;
        public float Drag => _drag;
        public bool FreezeYPosition => _freezeYPosition;

        // Item Handling
        public float ItemCapacity => _itemCapacity;
        public float TakeItemFromMachineDuration => _takeItemFromMachineDuration;
        public float TakeItemJumpPower => _takeItemJumpPower;
        public float TakeItemDuration => _takeItemDuration;
        public Ease TakeItemEase => _takeItemEase;
        
        // Animation
        public float AnimationSpeedMultiplier => _animationSpeedMultiplier;
        public bool UseRootMotion => _useRootMotion;
    }
}