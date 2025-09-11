using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Interactions.Interfaces;
using Game.Runtime.Character.Interfaces;
using Game.Runtime.Core.Extensions;

namespace Game.Runtime.Character.Components
{
    [RequireComponent(typeof(Collider))]
    public class InteractionController : MonoBehaviour, IInteractor
    {
        [Header("Settings")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactionInterval = 0.5f;

        private BaseCharacterController _character;
        private ICarryingController _cachedCarryController;
        private Dictionary<IInteractable, float> _activeInteractions = new Dictionary<IInteractable, float>();
        private float _lastInteractionTime;

        // Performance optimization - reuse collection for cleanup
        private readonly List<IInteractable> _interactionCleanupList = new List<IInteractable>();
        
        // Cache validation flags to avoid repeated GetComponent calls
        private bool _componentsValidated = false;
        private bool _hasValidComponents = false;

        public BaseCharacterController Character => _character;
        public bool IsInteracting => _activeInteractions.Count > 0;
        public bool CanStartInteraction => _hasValidComponents;

        void Awake()
        {
            ValidateAndCacheComponents();
        }

        private void ValidateAndCacheComponents()
        {
            if (_componentsValidated) return;

            try
            {
                _character = GetComponentInParent<BaseCharacterController>();
                
                if (_character != null)
                {
                    // Cache the carry controller to avoid repeated GetComponent calls
                    _cachedCarryController = _character.CarryingController;
                    _hasValidComponents = (_cachedCarryController != null);
                }

                if (_character == null)
                {
                    Debug.LogError($"[{gameObject.name}] InteractionController requires BaseCharacterController in parent!", this);
                    _hasValidComponents = false;
                }

                var collider = GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] InteractionController requires a Collider component!", this);
                    _hasValidComponents = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error validating components: {e.Message}", this);
                _hasValidComponents = false;
            }
            finally
            {
                _componentsValidated = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_hasValidComponents || !IsValidInteractable(other.gameObject)) return;

            try
            {
                var interactable = other.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(this))
                {
                    StartInteraction(interactable);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnTriggerEnter: {e.Message}", this);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (!_hasValidComponents || Time.time - _lastInteractionTime < interactionInterval) return;

            try
            {
                var interactable = other.GetComponent<IInteractable>();
                if (interactable != null && _activeInteractions.ContainsKey(interactable))
                {
                    interactable.OnInteractionContinue(this);
                    _activeInteractions[interactable] = Time.time;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnTriggerStay: {e.Message}", this);
                // Remove problematic interaction
                if (other != null)
                {
                    var interactable = other.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        EndInteraction(interactable);
                    }
                }
            }

            _lastInteractionTime = Time.time;
        }

        void OnTriggerExit(Collider other)
        {
            if (!_hasValidComponents) return;

            try
            {
                var interactable = other.GetComponent<IInteractable>();
                if (interactable != null && _activeInteractions.ContainsKey(interactable))
                {
                    EndInteraction(interactable);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnTriggerExit: {e.Message}", this);
            }
        }

        public void StartInteraction(IInteractable interactable)
        {
            if (interactable == null || !_hasValidComponents) return;
            
            if (!_activeInteractions.ContainsKey(interactable))
            {
                try
                {
                    _activeInteractions.Add(interactable, Time.time);
                    interactable.OnInteractionStart(this);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error starting interaction: {e.Message}", this);
                    // Clean up failed interaction
                    _activeInteractions.Remove(interactable);
                }
            }
        }

        public void EndInteraction(IInteractable interactable)
        {
            if (interactable == null) return;
            
            if (_activeInteractions.ContainsKey(interactable))
            {
                try
                {
                    _activeInteractions.Remove(interactable);
                    interactable.OnInteractionEnd(this);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error ending interaction: {e.Message}", this);
                }
            }
        }

        private bool IsValidInteractable(GameObject obj)
        {
            return obj != null && ((1 << obj.layer) & interactableLayer) != 0;
        }

        // Cached helper methods for performance - no more repeated GetComponent calls
        public bool HasItemsInHand()
        {
            return _cachedCarryController != null && _cachedCarryController.IsCarrying;
        }

        public bool CanCarryMore()
        {
            return _cachedCarryController != null && !_cachedCarryController.IsFull();
        }

        // Provide access to cached carry controller for external optimization
        public ICarryingController GetCachedCarryController()
        {
            return _cachedCarryController;
        }

        void OnDestroy()
        {
            SafeCleanupInteractions();
        }

        void OnDisable()
        {
            SafeCleanupInteractions();
        }

        private void SafeCleanupInteractions()
        {
            if (_activeInteractions.Count == 0) return;

            try
            {
                // Performance optimized cleanup - no allocation during cleanup
                _interactionCleanupList.Clear();
                
                foreach (var kvp in _activeInteractions)
                {
                    _interactionCleanupList.Add(kvp.Key);
                }

                foreach (var interactable in _interactionCleanupList)
                {
                    if (interactable != null)
                    {
                        try
                        {
                            interactable.OnInteractionEnd(this);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error during interaction cleanup: {e.Message}");
                        }
                    }
                }

                _interactionCleanupList.Clear();
                _activeInteractions.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during safe cleanup: {e.Message}");
                // Force clear in case of errors
                _activeInteractions.Clear();
            }
        }
    }
}