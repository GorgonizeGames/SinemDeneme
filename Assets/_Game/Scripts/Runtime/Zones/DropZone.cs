using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Character.Components;
using Game.Runtime.Items.Interfaces;

namespace Game.Runtime.Zones
{
    public class DropZone : MonoBehaviour
    {
        [Header("Drop Zone Settings")]
        [SerializeField] private string zoneId = "DropZone";
        [SerializeField] private List<string> acceptedItemTypes = new List<string>();
        [SerializeField] private bool autoDrop = true;
        [SerializeField] private Transform dropPoint;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject zoneIndicator;
        [SerializeField] private Color dropZoneColor = Color.red;

        public string ZoneId => zoneId;
        public bool AutoDrop => autoDrop;

        public System.Action<string, IPickupable> OnItemDropped; // Event for game logic

        void Awake()
        {
            if (dropPoint == null)
                dropPoint = transform;
                
            SetupVisuals();
        }

        public bool IsItemAccepted(IPickupable item)
        {
            if (acceptedItemTypes.Count == 0) return true; // Accept all if no restrictions
            return acceptedItemTypes.Contains(item.ItemId);
        }

        public void NotifyItemDropped(IPickupable item)
        {
            OnItemDropped?.Invoke(zoneId, item);
        }

        private void SetupVisuals()
        {
            if (zoneIndicator != null)
            {
                var renderer = zoneIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = dropZoneColor;
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = dropZoneColor;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
            
            if (dropPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(dropPoint.position, 0.3f);
            }
        }
    }
}
