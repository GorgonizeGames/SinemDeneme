using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Character.Components;
using Game.Runtime.Items.Interfaces;

namespace Game.Runtime.Zones
{
    public class PickupZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private string zoneId = "PickupZone";
        [SerializeField] private List<string> allowedItemTypes = new List<string>();
        [SerializeField] private bool autoPickup = true;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject zoneIndicator;
        [SerializeField] private Color zoneColor = Color.green;

        public string ZoneId => zoneId;
        public bool AutoPickup => autoPickup;

        void Awake()
        {
            SetupVisuals();
        }

        public bool IsItemAllowed(IPickupable item)
        {
            if (allowedItemTypes.Count == 0) return true; // Allow all if no restrictions
            return allowedItemTypes.Contains(item.ItemId);
        }

        private void SetupVisuals()
        {
            if (zoneIndicator != null)
            {
                var renderer = zoneIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = zoneColor;
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}