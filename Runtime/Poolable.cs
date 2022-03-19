using UnityEngine;
using UnityEngine.Events;

namespace Pools
{
    /// <summary>
    /// Reference for pools to get notified when this gets destroyed. 
    /// </summary>
    public class Poolable : MonoBehaviour
    {
        /// <summary>
        /// Invoked when this instance gets destroyed.
        /// </summary>
        public PoolableEvent OnDestroyEvent;

        [SerializeField]
        [Tooltip("Invoked when this instance get fetched.")]
        private UnityEvent onFetchEvent = new UnityEvent();
        
        /// <summary>
        /// Invoked when this instance get fetched.
        /// </summary>
        public UnityEvent OnFetchEvent => onFetchEvent;

        /// <summary>
        /// Activates this instance and invokes the OnFetch event.
        /// </summary>
        public void Fetch()
        {
            gameObject.SetActive(true);
            OnFetchEvent.Invoke();
        }

        private void OnDestroy() => OnDestroyEvent?.Invoke(this);
    }
}