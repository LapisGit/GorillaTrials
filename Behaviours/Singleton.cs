using UnityEngine;

namespace GorillaTrials.Behaviours
{
    internal class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public static bool HasInstance => Instance;

        private T GenericComponent => gameObject.GetComponent<T>();

        public void Awake()
        {
            if (HasInstance && Instance != GenericComponent)
            {
                Destroy(GenericComponent);
            }

            Instance = GenericComponent;
            Initialize();
        }

        public virtual void Initialize()
        {

        }
    }
}
