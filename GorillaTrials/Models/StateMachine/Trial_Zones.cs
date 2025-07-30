using System.Collections.Generic;
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

            GameObject trialZoneAsset = Singleton<TrialManager>.Instance.trialBoxAsset;

            for (int i = 0; i < 2; i++)
            {
                GameObject newZone = Object.Instantiate(trialZoneAsset);
                newZone.transform.SetParent(Singleton<TrialManager>.Instance.transform);
                newZone.SetActive(true);
                newZone.SetLayer(UnityLayer.GorillaBoundary);
                newZone.GetComponent<Collider>().isTrigger = true;

                TrialBoxCollider zoneCollider = newZone.GetOrAddComponent<TrialBoxCollider>();
                zones.Add(zoneCollider);

                newZone.transform.position = Trial.boxPositions[i];
                newZone.name = (i == 0)
                    ? $"Start Zone ({Trial.TrialServerName})"
                    : $"End Zone ({Trial.TrialServerName})";

                newZone.transform.localScale = new Vector3(1,1,1); // here just incase if i need to change it lol

                AudioSource audioSource = newZone.AddComponent<AudioSource>();
                audioSource.volume = 0.125f;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                audioSource.dopplerLevel = 0;
                audioSource.clip = VRRig.LocalRig.clipToPlay[5];
            }

            Logging.Info("Start and End zones created.");
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

            Logging.Info("destroyed zones :3");
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
                Logging.Info($"End zone triggered: {triggeredZone.gameObject.name}. Trial complete!");
                Trial.stateMachine.SwitchState(new Trial_End(Trial, true));
                return;
            }

            Logging.Warning($"Triggered zone at unexpected index {index}: {triggeredZone.gameObject.name}");
        }
    }
}
