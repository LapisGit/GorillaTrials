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
        
        public void Awake()
        {
            gameObject.SetLayer(UnityLayer.GorillaInteractable);
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (enabled && Time.realtimeSinceStartup > lastButtonClick && collider.TryGetComponent(out HandIndicator handIndicator))
            {
                lastButtonClick = Time.realtimeSinceStartup + debounceTime;

                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, handIndicator.isLeftHand, 0.05f);
                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                onPressed?.Invoke();
            }
        }
    }
}