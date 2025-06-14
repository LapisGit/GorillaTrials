using System.Collections.Generic;
using System.Linq;
using GorillaExtensions;
using GorillaTrials.Behaviours;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_Zones : TrialState
    {
        protected readonly List<TrialBoxCollider> zones = [];

        public Trial_Zones(Trial trial) : base(trial)
        {
            ;
        }

        public override void Enter()
        {
            base.Enter();

            GameObject startZoneAsset = Singleton<TrialManager>.Instance.trialBoxAsset;
            
            GameObject startZone = Object.Instantiate(startZoneAsset);
            startZone.transform.SetParent(Singleton<TrialManager>.Instance.transform);
            startZone.SetActive(true);

            TrialBoxCollider startZone1 = startZone.GetOrAddComponent<TrialBoxCollider>();
            zones.Add(startZone1);

            startZone.transform.position = Trial.zoneData.startPosition;
            startZone.name = $"Trial Start Zone ({Trial.TrialServerName})";
            
            GameObject endZoneAsset = Singleton<TrialManager>.Instance.trialBoxAsset;
            
            GameObject endZone = Object.Instantiate(endZoneAsset);
            endZone.transform.SetParent(Singleton<TrialManager>.Instance.transform);
            endZone.SetActive(true);
            endZone.SetLayer(UnityLayer.GorillaBoundary);
            endZone.GetComponent<Collider>().isTrigger = true;

            TrialBoxCollider endZone1 = endZone.GetOrAddComponent<TrialBoxCollider>();
            zones.Add(endZone1);

            endZone.transform.position = Trial.zoneData.endPosition;
            endZone.name = $"Trial Start Zone ({Trial.TrialServerName})";
        }

        public override void Exit()
        {
            base.Exit();

            List<GameObject> zoneObjects = zones.Select(box => box.gameObject).ToList();
            for (int i = 0; i < zones.Count; i++)
            {
                Object.Destroy(zoneObjects[i]);
            }
            zones.Clear();
        }
        

        public override void BoxTriggered(TrialBoxCollider box)
        {
            base.BoxTriggered(box);

            Logging.Info("Zone Triggered!");
            Trial.stateMachine.SwitchState(new Trial_End(Trial, true));
        }
    }
}