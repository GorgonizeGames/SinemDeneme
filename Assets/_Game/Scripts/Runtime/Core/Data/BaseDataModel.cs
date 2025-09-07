using System;
using UnityEngine;
using Game.Runtime.Core.Attributes;

namespace Game.Runtime.Core.Data
{
    public abstract class BaseDataModel : ScriptableObject
    {
        public string objectName;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        public string id;

        protected void OnValidate()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }
#endif
        }

        [ContextMenu("Set ID")]
        public void SetId()
        {
            id = Guid.NewGuid().ToString();
        }
    }
}
