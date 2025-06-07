using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaTrials.Behaviors
{
    public class TrialBoxCollider : GorillaTriggerBox
    {
        public string trialservername;
        public int index;
        public override void OnBoxTriggered()
        {
            base.OnBoxTriggered();
            Trials.boxesCollected += 1;
            if (Trials.boxesCollected >= Trials.boxesToCollect)
            {
                Trials.EndTrial();
            }
            gameObject.SetActive(false);
        }
    }
}
