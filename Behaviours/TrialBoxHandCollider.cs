using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaTrials.Behaviours
{
    public class TrialBoxHandCollider : MonoBehaviour
    {
        private TrialBoxCollider parentBox;
        private const float debounceTime = 0.1f;
        private static float lastTriggerTime;

        void Awake()
        {
            parentBox = GetComponentInParent<TrialBoxCollider>();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (!enabled) return;

            if (parentBox == null)
                parentBox = GetComponentInParent<TrialBoxCollider>();

            if (Time.realtimeSinceStartup > lastTriggerTime &&
                collider.TryGetComponent(out HandIndicator handIndicator))
            {
                lastTriggerTime = Time.realtimeSinceStartup + debounceTime;

                //GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, handIndicator.isLeftHand, 0.05f);
                //GorillaTagger.Instance.StartVibration(
                //   handIndicator.isLeftHand,
                //   GorillaTagger.Instance.tapHapticStrength / 2f,
                //   GorillaTagger.Instance.tapHapticDuration
                // );
                // used for debugging ^

                if (parentBox != null)
                {
                    parentBox.OnBoxTriggered();
                    GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                }
                else
                {
                    Debug.LogWarning("HandCollider triggered, but parentBox is NULL! >:3");
                }
            }
        }

    }
}