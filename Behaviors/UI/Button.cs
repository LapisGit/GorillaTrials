using System;
using UnityEngine;

namespace GorillaTrials.Behaviors.UI
{
    public class Button : MonoBehaviour
    {
        public bool useGlobalCooldown;
        public GameObject buttonVisual;
        public GameObject faceVisual;
        public Vector3 initialButtonLocalPos;
        public Vector3 initialFaceLocalPos;
        public Vector3 initialFaceLocalScale;
        public Action action;
        public float touchTime;
        public static float debounceTime = 0.125f;
        public int pressButtonSoundIndex = 67;
        public float instantiatedTime;

        public void Awake()
        {
            instantiatedTime = Time.time;
        }

        public void OnTriggerExit(Collider collider)
        {
            try
            {
                if (!enabled)
                    return;
                if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
                    return;

                buttonVisual.transform.localPosition = initialButtonLocalPos;

                if (faceVisual != null)
                {
                    faceVisual.transform.localPosition = initialFaceLocalPos;
                    faceVisual.transform.localScale = initialFaceLocalScale;
                }
                if (touchTime + debounceTime >= Time.time)
                {
                    return;
                }
                

                //InGameMenu.Instance.PlayButtonClick(instantiatedTime, collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>().isLeftHand, false);
                GorillaTagger.Instance.StartVibration(collider.GetComponent<GorillaTriggerColliderHandIndicator>().isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                ButtonPress(collider);
            }
            catch { } //bro idk
        }
        public void OnTriggerEnter(Collider collider)
        {
            try
            {
                if (!enabled)
                    return;
                if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
                    return;

                //InGameMenu.Instance.PlayButtonClick(instantiatedTime, collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>().isLeftHand, true);
                GorillaTagger.Instance.StartVibration(collider.GetComponent<GorillaTriggerColliderHandIndicator>().isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            }
            catch { } //bro idk
        }
        public void ButtonPress(Collider collider)
        {
            if (!enabled)
                return;
            if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
                return;

            if (touchTime + debounceTime >= Time.time)
            {
                return;
            }

            touchTime = Time.time;
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();

            if (action != null)
                action();

            ButtonActivation();
            ButtonActivationWithHand(component.isLeftHand);

            if (component == null)
                return;
        }
        public virtual void ButtonActivation()
        {
        }
        public virtual void ButtonActivationWithHand(bool isLeftHand)
        {
        }
    }
}
