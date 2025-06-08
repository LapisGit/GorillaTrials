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

                TrialBoxCollider trialBox = newBox.GetOrAddComponent<TrialBoxCollider>();
                boxes.Add(trialBox);

                newBox.transform.position = Trial.boxPositions[i];
                newBox.name = $"Trial Box #{i + 1} ({Trial.TrialServerName})";

                newBox.transform.localScale = Vector3.one * GetBoxScaleFactor(i);
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

            for (int i = 0; i < 5; i++)
            {
                if (index == boxesCollected + i)
                    return 0.5f - (i * 0.1f);
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

                Logging.Info($"Triggered relevant box {box.gameObject.name} (proceeded to {boxesCollected})");

                Logging.Info($"{boxesCollected}/{boxesToCollect}");

                if (boxesCollected >= boxesToCollect)
                {
                    Logging.Info("Boxes collected!");
                    Trial.stateMachine.SwitchState(new Trial_End(Trial, true));
                    return;
                }

                for (int i = 0; i < boxes.Count; i++)
                {
                    boxes[i].transform.localScale = Vector3.one * GetBoxScaleFactor(i);
                }

                return;
            }

            Logging.Warning($"Triggered unknown box {box.gameObject.name} (boxesCollected = {boxesCollected})");
        }
    }
}
