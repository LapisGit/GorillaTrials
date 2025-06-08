using System;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaTrials.Behaviours.UI
{
    public class TrialButton : MonoBehaviour
    {
        protected readonly float debounceTime = 0.1f;

        protected static float lastButtonClick;

        public Action onPressed;

        public void OnTriggerEnter(Collider collider)
        {
            if (enabled && Time.realtimeSinceStartup > lastButtonClick && collider.TryGetComponent(out HandIndicator handIndicator))
            {
                lastButtonClick = Time.realtimeSinceStartup + debounceTime;
                onPressed?.Invoke();
            }
        }
    }
}