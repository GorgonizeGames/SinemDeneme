using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Interactions.Interfaces;

namespace Game.Runtime.Character.Components
{
    [RequireComponent(typeof(Collider))]
    public class InteractionController : MonoBehaviour, IInteractor
    {
        [Header("Settings")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactionInterval = 0.5f;

        private BaseCharacterController _character;
        private StackingCarryController _carryController;
        private Dictionary<IInteractable, float> _activeInteractions = new Dictionary<IInteractable, float>();
        private float _lastInteractionTime;

        public BaseCharacterController Character => _character;
        public bool IsInteracting => _activeInteractions.Count > 0;
        public bool CanStartInteraction => _character != null;

        void Awake()
        {
            _character = GetComponentInParent<BaseCharacterController>();
            _carryController = _character?.GetComponentInChildren<StackingCarryController>();
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_character == null)
                Debug.LogError($"[{gameObject.name}] InteractionController requires BaseCharacterController in parent!", this);

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsValidInteractable(other.gameObject)) return;

            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract(this))
            {
                StartInteraction(interactable);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (Time.time - _lastInteractionTime < interactionInterval) return;

            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null && _activeInteractions.ContainsKey(interactable))
            {
                interactable.OnInteractionContinue(this);
                _activeInteractions[interactable] = Time.time;
            }

            _lastInteractionTime = Time.time;
        }

        void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null && _activeInteractions.ContainsKey(interactable))
            {
                EndInteraction(interactable);
            }
        }

        public void StartInteraction(IInteractable interactable)
        {
            if (interactable == null) return;
            
            if (!_activeInteractions.ContainsKey(interactable))
            {
                _activeInteractions.Add(interactable, Time.time);
                interactable.OnInteractionStart(this);
            }
        }

        public void EndInteraction(IInteractable interactable)
        {
            if (interactable == null) return;
            
            if (_activeInteractions.ContainsKey(interactable))
            {
                _activeInteractions.Remove(interactable);
                interactable.OnInteractionEnd(this);
            }
        }

        private bool IsValidInteractable(GameObject obj)
        {
            return obj != null && ((1 << obj.layer) & interactableLayer) != 0;
        }

        // Helper methods for stacking system
        public bool HasItemsInHand()
        {
            return _carryController != null && _carryController.IsCarrying;
        }

        public bool CanCarryMore()
        {
            return _carryController != null && !_carryController.IsFull;
        }

        void OnDestroy()
        {
            // Simple cleanup - orijinal yaklaşım
            foreach (var kvp in _activeInteractions)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.OnInteractionEnd(this);
                }
            }
            _activeInteractions.Clear();
        }
    }
}