using System;
using UnityEngine;

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// using dev9998's button.cs as a test, all credits to them, will be replaced later on
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

namespace GorillaTrials.Behaviors.UI
{
    public class Button : MonoBehaviour
    {
        public const float Debounce = 0.25f;
        private float _timeStamp = 1;
        private float _lastPress;

        // Updated event to include GameObject
        public event Action<GorillaTriggerColliderHandIndicator, GameObject> OnPress;

        public void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = (int)UnityLayer.GorillaInteractable;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Time.time > _lastPress + Debounce)
            {
                _timeStamp = 0;
                _lastPress = Time.time;

                OnPress?.Invoke(component, gameObject); // Passes the button object
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 1.25f, GorillaTagger.Instance.tapHapticDuration / 1.1f);
            }
        }
    }
}
