using GorillaExtensions;
using GorillaTrials.Behaviours;
using GorillaTrials.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_Zones(Trial trial) : TrialState(trial)
    {
        protected readonly List<TrialBoxCollider> zones = [];

        public override void Enter()
        {
            base.Enter();

            GameObject trialZoneAsset = Singleton<TrialManager>.Instance.trialBoxAsset;

            for (int i = 0; i < 2; i++)
            {
                GameObject newZone = Object.Instantiate(trialZoneAsset);
                newZone.transform.SetParent(Singleton<TrialManager>.Instance.transform);
                newZone.SetActive(true);
                newZone.SetLayer(UnityLayer.GorillaBoundary);
                newZone.GetComponent<Collider>().isTrigger = true;

                Transform handColliderTransform = newZone.transform.Find("HandCollider");
                if (handColliderTransform != null)
                {
                    Collider handCollider = handColliderTransform.GetComponent<Collider>();
                    if (handCollider != null)
                    {
                        handCollider.isTrigger = true;
                    }
                }

                TrialBoxCollider zoneCollider = newZone.GetOrAddComponent<TrialBoxCollider>();
                zones.Add(zoneCollider);

                newZone.transform.position = Trial.boxPositions[i];
                newZone.name = (i == 0)
                    ? $"{Trial.TrialServerName}: Start Zone"
                    : $"{Trial.TrialServerName}: End Zone";

                newZone.transform.localScale = Vector3.one; // here just incase if i need to change it lol

                AudioSource audioSource = newZone.AddComponent<AudioSource>();
                audioSource.volume = 0.125f;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                audioSource.dopplerLevel = 0;
                audioSource.clip = VRRig.LocalRig.clipToPlay[5];
            }
        }

        public override void Exit()
        {
            base.Exit();

            foreach (var zone in zones)
            {
                if (zone != null)
                {
                    Object.Destroy(zone.gameObject);
                }
            }
            zones.Clear();
        }

        public override void BoxTriggered(TrialBoxCollider triggeredZone)
        {
            base.BoxTriggered(triggeredZone);

            int index = zones.IndexOf(triggeredZone);

            if (index == -1)
            {
                Logging.Warning($"Triggered unknown zone {triggeredZone.gameObject.name}");
                return;
            }

            if (index == 0)
            {
                Logging.Info($"Start zone triggered: {triggeredZone.gameObject.name}");
                return;
            }

            if (index == 1)
            {
                Trial.stateMachine.SwitchState(new Trial_End(Trial, true));
                return;
            }

            Logging.Warning($"Triggered zone at unexpected index {index}: {triggeredZone.gameObject.name}");
        }

        private static List<Vector3> SampleCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, float spacing, int resolution)
        {
            List<Vector3> list = [];

            Vector3 lastPoint = startPoint;
            list.Add(lastPoint);

            float totalDistance = 0f;

            for (int i = 1; i <= resolution; i++)
            {
                float position = i / (float)resolution;
                Vector3 point = QuadraticBezier(startPoint, controlPoint, endPoint, position);

                totalDistance += Vector3.Distance(lastPoint, point);

                if (totalDistance >= spacing)
                {
                    list.Add(point);
                    totalDistance = 0f;
                }

                lastPoint = point;
            }

            return list;
        }

        private static Vector3 ControlPoint(Vector3 startPoint, Vector3 endPoint, float amount)
        {
            Vector3 mid = (startPoint + endPoint) / 2;
            Vector3 forward = (endPoint - startPoint).normalized;
            Vector3 perpendicular = Vector3.Cross(Vector3.up, forward).normalized;
            return mid + perpendicular * amount;
        }

        private static Vector3 QuadraticBezier(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, float position) => Mathf.Pow(1 - position, 2) * startPoint + 2 * (1 - position) * position * controlPoint + Mathf.Pow(position, 2) * endPoint;
    }
}
