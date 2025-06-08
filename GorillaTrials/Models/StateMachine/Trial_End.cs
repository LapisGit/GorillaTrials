using System;
using GorillaTrials.Behaviours;

namespace GorillaTrials.Models.StateMachine
{
    public class Trial_End : TrialState
    {
        protected bool submitTime = true;

        public Trial_End(Trial trial, bool submitTime) : base(trial)
        {
            this.submitTime = submitTime;
            ;
        }

        public override void Enter()
        {
            base.Enter();

            Trial.stopwatch.Stop();

            Trial.trialObject.SetActive(true);

            Singleton<TrialManager>.Instance.EndTrial(submitTime ? Math.Round(Trial.stopwatch.Elapsed.TotalSeconds, 3) : null);
        }
    }
}
