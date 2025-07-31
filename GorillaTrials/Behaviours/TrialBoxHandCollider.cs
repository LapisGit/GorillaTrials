using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaTrials.Behaviours
{
    public class TrialBoxHandCollider : MonoBehaviour
    {
        private TrialBoxCollider parentBox;

        protected readonly float debounceTime = 0.1f;
        protected static float lastHandTriggerTime;

        void Awake()
        {
            parentBox = GetComponentInParent<TrialBoxCollider>();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out HandIndicator handIndicator))
            {
                if (enabled && Time.realtimeSinceStartup > lastHandTriggerTime)
                {
                    lastHandTriggerTime = Time.realtimeSinceStartup + debounceTime;
                    
                    if (parentBox != null)
                    {
                        parentBox.OnBoxTriggered();
                    }
                }
            }
        }
    }
}