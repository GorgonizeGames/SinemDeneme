using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Items.Data;

namespace Game.Runtime.Items.Services
{
    public class ItemPoolService : MonoBehaviour, IItemPoolService
    {
        [System.Serializable]
        public class ItemPoolConfig
        {
            public ItemType Type;
            public GameObject Prefab;
            public int InitialPoolSize = 10;
        }

        [SerializeField] private List<ItemPoolConfig> poolConfigs;
        [SerializeField] private Transform poolContainer;

        private Dictionary<ItemType, Queue<Item>> _pools = new Dictionary<ItemType, Queue<Item>>();

        void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var config in poolConfigs)
            {
                var pool = new Queue<Item>();

                for (int i = 0; i < config.InitialPoolSize; i++)
                {
                    var itemGO = Instantiate(config.Prefab, poolContainer);
                    var item = itemGO.GetComponent<Item>();
                    itemGO.SetActive(false);
                    pool.Enqueue(item);
                }

                _pools[config.Type] = pool;
            }
        }

        public Item GetItem(ItemType type)
        {
            if (!_pools.ContainsKey(type)) return null;

            var pool = _pools[type];
            Item item = null;

            if (pool.Count > 0)
            {
                item = pool.Dequeue();
            }
            else
            {
                // Create new item if pool is empty
                var config = poolConfigs.Find(c => c.Type == type);
                if (config != null)
                {
                    var itemGO = Instantiate(config.Prefab, poolContainer);
                    item = itemGO.GetComponent<Item>();
                }
            }

            if (item != null)
            {
                item.gameObject.SetActive(true);
            }

            return item;
        }

        public void ReturnItem(Item item)
        {
            if (item == null) return;

            item.ReturnToPool();

            if (_pools.ContainsKey(item.ItemType))
            {
                _pools[item.ItemType].Enqueue(item);
            }
        }
    }
}