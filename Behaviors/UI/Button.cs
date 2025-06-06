using System;
using UnityEngine;

namespace GorillaTrials.Behaviors.UI
{
    public class UIButton : MonoBehaviour
    {
        public float lastClicked;
        public float debounceTime = 0.1f;
        public Action onPressed;
        public void Awake()
        {
            lastClicked = Time.time + debounceTime;
        }

        public void SetAction(Action action)
        {
            onPressed = action;
        }
        public void OnTriggerEnter(Collider collider)
        {
            if (!enabled)
            {
                return;
            }
            if (lastClicked + debounceTime >= Time.time)
            {
                return;
            }
            if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
            {
                return;
            }
            lastClicked = Time.time;
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            if (component == null)
            {
                return;
            }
            if (onPressed != null)
                onPressed();
        }
    }
}