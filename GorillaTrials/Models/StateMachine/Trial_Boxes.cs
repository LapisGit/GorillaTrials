using System.Collections.Generic;
using System.Linq;
using GorillaExtensions;
using GorillaTrials.Behaviours;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_Boxes : TrialState
    {
        protected int boxesCollected, boxesToCollect;

        protected readonly List<TrialBoxCollider> boxes = [];

        public Trial_Boxes(Trial trial) : base(trial)
        {
            ;
        }

        public override void Enter()
        {
            base.Enter();

            boxesCollected = 0;
            boxesToCollect = Trial.boxPositions.Count;

            GameObject trialBoxAsset = Singleton<TrialManager>.Instance.trialBoxAsset;

            for (int i = 0; i < boxesToCollect; i++)
            {
                GameObject newBox = Object.Instantiate(trialBoxAsset);
                newBox.transform.SetParent(Singleton<TrialManager>.Instance.transform);
                newBox.SetActive(true);
                newBox.SetLayer(UnityLayer.GorillaBoundary);
                newBox.GetComponent<Collider>().isTrigger = true;
                
                Transform handColliderTransform = newBox.transform.Find("HandCollider");
                if (handColliderTransform != null)
                {
                    Collider handCollider = handColliderTransform.GetComponent<Collider>();
                    if (handCollider != null)
                        handCollider.isTrigger = true;
                    
                    if (!handColliderTransform.TryGetComponent(out Rigidbody rb))
                    {
                        rb = handColliderTransform.gameObject.AddComponent<Rigidbody>();
                        rb.isKinematic = true;
                    }
                    
                    handColliderTransform.gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");
                    
                    handColliderTransform.gameObject.GetOrAddComponent<TrialBoxHandCollider>(); 
                }


                TrialBoxCollider trialBox = newBox.GetOrAddComponent<TrialBoxCollider>();
                boxes.Add(trialBox);

                newBox.transform.position = Trial.boxPositions[i];
                newBox.name = $"Trial Box #{i + 1} ({Trial.TrialServerName})";

                newBox.transform.localScale = Vector3.one * GetBoxScaleFactor(i);

                AudioSource audioSource = newBox.AddComponent<AudioSource>();
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

            List<GameObject> boxObjects = boxes.Select(box => box.gameObject).ToList();
            for (int i = 0; i < boxes.Count; i++)
            {
                Object.Destroy(boxObjects[i]);
            }
            boxes.Clear();
        }

        public float GetBoxScaleFactor(int index)
        {
            if (index < boxesCollected)
                return Mathf.Epsilon; // closest number to 0

            int apparentBoxCount = 3;
            float fullSize = 0.5f;

            for (int i = 0; i < apparentBoxCount; i++)
            {
                if (index == boxesCollected + i)
                    return fullSize - (i * (fullSize / apparentBoxCount));
            }

            return 0.05f;
        }

        public override void BoxTriggered(TrialBoxCollider box)
        {
            base.BoxTriggered(box);

            int index = boxes.IndexOf(box);

            if (index == -1)
            {
                Logging.Info($"Triggered unrelated box {box.gameObject.name}");
                return;
            }

            if (index > boxesCollected)
            {
                Logging.Info($"Triggered future box {box.gameObject.name} (box is at {index}, we are at {boxesCollected})");
                return;
            }

            if (index == boxesCollected)
            {
                boxesCollected = index + 1;
                box.gameObject.SetActive(false);

#if DEBUG
                Logging.Info($"Triggered relevant box {box.gameObject.name} (proceeded to {boxesCollected})");
                Logging.Info($"{boxesCollected}/{boxesToCollect}");          
#endif

                if (boxesCollected >= boxesToCollect)
                {
                    Trial.stateMachine.SwitchState(new Trial_End(Trial, true));
                    return;
                }

                for (int i = 0; i < boxes.Count; i++)
                {
                    boxes[i].transform.localScale = Vector3.one * GetBoxScaleFactor(i);
                    if (i == boxesCollected && boxes[i].TryGetComponent(out AudioSource audioSource))
                        audioSource.Play();
                }

                return;
            }

            Logging.Warning($"Triggered unknown box {box.gameObject.name} (boxesCollected = {boxesCollected})");
        }
    }
}
