using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Pools.Tests
{
    public class PoolTest
    {
        private GameObject poolableObj;
        private Poolable poolable;
        
        private GameObject poolObj;
        private Pool pool;
        public IEnumerator SetUp(Action<Pool> additionalSetup = null)
        {
            SetupPoolable();
            SetupPool();
            
            pool.PooledObject = poolable;
            additionalSetup?.Invoke(pool);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(poolableObj);
            Object.Destroy(poolObj);
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertInstancesAreGenerated()
        {
            yield return SetUp(p =>
            {
                p.InitialCapacity = RngRange(50, 100);
            });
            
            Assert.AreEqual(pool.InitialCapacity, pool.AllInstances.Count());
            Assert.AreEqual(pool.InitialCapacity, pool.AvailableInstances.Count());
            Assert.AreEqual(0, pool.OccupiedInstances.Count());
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertFetchingUpdatesAvailableAndOccupiedInstances()
        {
            yield return SetUp(p =>
            {
                p.InitialCapacity = RngRange(51, 100);
            });
            
            var fetchedInstances = FetchRngRange(1, 50);
            yield return null;
            
            Assert.AreEqual(pool.InitialCapacity, pool.AllInstances.Count());
            Assert.AreEqual(pool.InitialCapacity - fetchedInstances, pool.AvailableInstances.Count());
            Assert.AreEqual(fetchedInstances, pool.OccupiedInstances.Count());
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertFetchingIncreasesCapacity()
        {   
            yield return SetUp();
            
            var fetchedInstances = FetchRngRange(1, 50);
            yield return null;
            
            Assert.AreEqual(fetchedInstances, pool.AllInstances.Count());
            Assert.AreEqual(0, pool.AvailableInstances.Count());
            Assert.AreEqual(fetchedInstances, pool.OccupiedInstances.Count());
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertFetchingWithGrowingCapacityIncreasesCapacity()
        {
            yield return SetUp(p =>
            {
                p.GrowingCapacity = RngRange(1, 10);
            });
            
            Fetch(1);
            yield return null;
            
            Assert.AreEqual(pool.GrowingCapacity + 1, pool.AllInstances.Count());
            Assert.AreEqual(pool.GrowingCapacity, pool.AvailableInstances.Count());
            Assert.AreEqual(1, pool.OccupiedInstances.Count());
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertDestroyedPoolablesAreRemovedFromPool()
        {
            yield return SetUp(p =>
            {
                p.InitialCapacity = RngRange(51, 100);
            });

            var fetchedInstances = FetchRngRange(1, 50);
            var poolablesToDestroy = pool.OccupiedInstances.ToList();
            poolablesToDestroy.ForEach(Object.Destroy);
            yield return null;
            
            Assert.AreEqual(pool.InitialCapacity -  fetchedInstances, pool.AllInstances.Count());
            Assert.AreEqual(pool.InitialCapacity -  fetchedInstances, pool.AvailableInstances.Count());
            Assert.AreEqual(0, pool.OccupiedInstances.Count());
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AssertFetchOnPoolablesIsInvoked()
        {
            yield return SetUp(p =>
            {
                p.InitialCapacity = RngRange(51, 100);
            });

            var listener = new UnityEventListener("Event");
            pool.AvailableInstances.ToList().ForEach(p =>
            {
                p.OnFetchEvent.AddListener(listener.Invoke);
            });

            var fetchedInstances = FetchRngRange(1, 50);
            yield return null;

            listener.AssertInvocations((int) fetchedInstances);
            yield return null;
        }

        private void Fetch(uint amt) => FetchRngRange(amt, amt);
        private uint FetchRngRange(uint min, uint max)
        {
            var fetchedInstances = RngRange(min, max);
            for (var i = 0; i < fetchedInstances; i++)
                pool.Fetch();

            return fetchedInstances;
        }
        private static uint RngRange(uint min, uint max)
        {
            return (uint) Random.Range(min, max);
        }

        private void SetupPool()
        {
            poolObj = new GameObject {name = "Pool"};
            pool = poolObj.AddComponent<Pool>();
        }

        private void SetupPoolable()
        {
            poolableObj = new GameObject {name = "Pooled"};
            poolable = poolableObj.AddComponent<Poolable>();
            poolableObj.AddComponent<Observer>();
        }
    }

    class Observer : MonoBehaviour
    {
        private Poolable poolable;
        private UnityEventListener listener;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        private void Start()
        {
            listener = new UnityEventListener(poolable.name);
            poolable.OnFetchEvent.AddListener(listener.Invoke);
        }
    }
}