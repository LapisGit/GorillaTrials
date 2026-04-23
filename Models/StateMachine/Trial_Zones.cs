using GorillaExtensions;
using GorillaTrials.Behaviours;
using GorillaTrials.Tools;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_Zones(Trial trial) : TrialState(trial)
    {
        protected readonly List<TrialBoxCollider> zones = [];

        private List<GameObject> directionalObjects;

        public override void Enter()
        {
            base.Enter();

            GameObject trialZoneAsset = Singleton<TrialManager>.Instance.trialZoneAsset;

            for (int i = 0; i < 2; i++)
            {
                GameObject newZone = Object.Instantiate(trialZoneAsset);
                newZone.transform.SetParent(Singleton<TrialManager>.Instance.transform);
                newZone.SetActive(true);

                MeshCollider collider = newZone.GetComponentInChildren<MeshCollider>();
                collider.gameObject.SetLayer(UnityLayer.GorillaBoundary);
                collider.isTrigger = true;

                TrialBoxCollider zoneCollider = collider.gameObject.GetOrAddComponent<TrialBoxCollider>();
                zones.Add(zoneCollider);

                newZone.transform.position = Trial.boxPositions[i];
                newZone.name = (i == 0) ? $"{Trial.TrialServerName}: Start Zone" : $"{Trial.TrialServerName}: End Zone";

                AudioSource audioSource = newZone.AddComponent<AudioSource>();
                audioSource.volume = 0.125f;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                audioSource.dopplerLevel = 0;
                audioSource.clip = VRRig.LocalRig.clipToPlay[5];
            }

            TrialManager.Instance.StartCoroutine(HandleDirectionPoints());
        }

        public override void Exit()
        {
            base.Exit();

            foreach (var zone in zones)
            {
                if (zone != null)
                {
                    Object.Destroy(zone.transform.parent.gameObject);
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

        private IEnumerator HandleDirectionPoints()
        {
            GameObject trialBoxAsset = Object.Instantiate(Singleton<TrialManager>.Instance.trialBoxAsset);
            System.Array.ForEach(trialBoxAsset.GetComponentsInChildren<Collider>(true), collider => collider.enabled = false);

            Vector3 headPosition = GorillaLocomotion.GTPlayer.Instance.HeadCenterPosition;
            Vector3 startPoint = Vector3.MoveTowards(Trial.boxPositions[0], headPosition, -2f).WithY(headPosition.y);
            Vector3 endPoint = Trial.boxPositions[1];
            int resolution = Mathf.FloorToInt(Vector3.Distance(startPoint, endPoint));
            List<Vector3> curve = SampleCurve(startPoint, ControlPoint(startPoint, endPoint, 32f), endPoint, 2f, resolution);

            int curveCount = curve.Count;
            directionalObjects = [];

            for (int i = 0; i < curveCount; i++)
            {
                GameObject gameObject = Object.Instantiate(trialBoxAsset);
                gameObject.transform.SetParent(Singleton<TrialManager>.Instance.transform);
                gameObject.SetActive(true);
                gameObject.transform.position = curve[i];
                gameObject.transform.localScale = Vector3.one * GetBoxScaleFactor(i);
                directionalObjects.Insert(i, gameObject);
            }

            Object.Destroy(trialBoxAsset);

            yield return new WaitForEndOfFrame();
            yield return null;

            while (directionalObjects.Count != 0)
            {
                GameObject gameObject = directionalObjects[0];
                Object.Destroy(gameObject);
                directionalObjects.RemoveAt(0);
                curveCount--;

                for(int i = 0; i < curveCount; i++)
                {
                    gameObject = directionalObjects[i];
                    gameObject.transform.localScale = Vector3.one * GetBoxScaleFactor(i);
                }

                yield return new WaitForSeconds(0.05f);
            }

            yield break;
        }

        public float GetBoxScaleFactor(int index)
        {
            int apparentBoxCount = 3;
            float fullSize = 0.35f;

            for (int i = 0; i < apparentBoxCount; i++)
            {
                if (index == i)
                    return Mathf.Max(fullSize - (i * (fullSize / apparentBoxCount)), Mathf.Epsilon);
            }

            return Mathf.Epsilon;
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
