using GorillaTrials.Behaviours;
using System;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_End(Trial trial, bool submitTime) : TrialState(trial)
    {
        protected bool submitTime = submitTime;

        public override void Enter()
        {
            base.Enter();

            VRRig.LocalRig.PlayTagSoundLocal(2, 0.25f, true);

            Trial.stopwatch.Stop();

            Trial.trialObject.SetActive(true);

            HUDManager.instance.ClearHUD();

            Singleton<TrialManager>.Instance.EndTrial(submitTime ? Math.Round(Trial.stopwatch.Elapsed.TotalSeconds, 3) : null);
        }
    }
}
