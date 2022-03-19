using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pools
{
    /// <summary>
    /// Pool responsible for generating instances based on the
    /// pooled object.
    /// </summary>
    public class Pool : MonoBehaviour, IEnumerable<Poolable>
    {
        private readonly List<Poolable> instances = new List<Poolable>();
        
        /// <summary>
        /// Returns all instances.
        /// </summary>
        public IEnumerable<Poolable> AllInstances => instances
            .ToList()
            .AsReadOnly();
        
        /// <summary>
        /// Returns all available instances.
        /// </summary>
        public IEnumerable<Poolable> OccupiedInstances => instances
            .Where(p => p.isActiveAndEnabled)
            .ToList()
            .AsReadOnly();
        
        /// <summary>
        /// Returns all available instances.
        /// </summary>
        public IEnumerable<Poolable> AvailableInstances => instances
            .Where(p => !p.isActiveAndEnabled)
            .ToList()
            .AsReadOnly();

        /// <summary>
        /// Returns whether there are any instances available.
        /// </summary>
        public bool HasAvailableInstances => AvailableInstances.Any();
        
        private uint sequence = 1;
        /// <summary>
        /// Current sequence. Represents how many instances were
        /// created until now
        /// </summary>
        public uint Sequence => sequence;

        [SerializeField]
        [Tooltip("Object used as template to create instances.")]
        private Poolable pooledObject;
        
        /// <summary>
        /// Object used as template to create instances.
        /// </summary>
        public Poolable PooledObject
        {
            get => pooledObject;
            set => pooledObject = value;
        }

        [SerializeField] 
        [Tooltip("Default parent of the instances. Instance will be in directly in scene if not specified.")]
        private Transform defaultParent;
        
        /// <summary>
        /// Parent of the instances. Instance will be in directly in scene if not specified.
        /// </summary>
        public Transform DefaultParent
        {
            get => defaultParent;
            set => defaultParent = value;
        }

        [SerializeField]
        [Tooltip("Amount of instances that should be generated initially.")]
        private uint initialCapacity;
        
        /// <summary>
        /// Amount of instances that should be generated initially.
        /// </summary>
        public uint InitialCapacity
        {
            get => initialCapacity;
            set => initialCapacity = value;
        }

        [SerializeField]
        [Tooltip("Determines if the initial capacity can be exceeded.")]
        private bool hasFixedCapacity;
        
        /// <summary>
        /// Determines if the initial capacity can be exceeded.
        /// </summary>
        public bool HasFixedCapacity
        {
            get => hasFixedCapacity;
            set => hasFixedCapacity = value;
        }

        [SerializeField]
        [Tooltip("Amount of instances that should be additionally generated whenever there are no available instances.")]
        private uint growingCapacity;
        
        /// <summary>
        /// Amount of instances that should be additionally generated whenever there are no available instances
        /// </summary>
        public uint GrowingCapacity
        {
            get => growingCapacity;
            set => growingCapacity = value;
        }

        /// <summary>
        /// Fetches the next available instance in this pool.
        /// </summary>
        /// <returns>Poolable. May be null if at capacity.</returns>
        [ContextMenu(nameof(Fetch))]
        public GameObject Fetch()
        {
            var poolable = FirstOrInit();
            if (poolable == null)
                return null;
            
            if (poolable != null)
                poolable.Fetch();

            return poolable.gameObject;
        }

        public void Init()
        {
            for (var i = 0; i < InitialCapacity; i++)
                InitInstance();
        }

        private void Start() => Init();

        private void OnDestroy()
        {
            for (var i = instances.Count - 1; i >= 0; i--)
                OnPoolableDestroyed(instances[i]);
        }

        private Poolable FirstOrInit()
        {
            if (HasAvailableInstances) 
                return AvailableInstances.First();

            if (hasFixedCapacity)
                return null;
                    
            var poolable = InitInstance();
            for (var i = 0; i < GrowingCapacity; i++)
                InitInstance();

            return poolable;
        }

        private Poolable InitInstance()
        {
            var poolable = Instantiate(PooledObject, DefaultParent);
            poolable.gameObject.name = $"{pooledObject.name}-{sequence}";
            poolable.gameObject.SetActive(false);
            poolable.OnDestroyEvent += OnPoolableDestroyed;
            instances.Add(poolable);
            sequence++;
            return poolable;
        }

        private void OnPoolableDestroyed(Poolable poolable)
        {
            poolable.OnDestroyEvent -= OnPoolableDestroyed;
            instances.Remove(poolable);
        }

        public IEnumerator<Poolable> GetEnumerator()
        {
            return instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}