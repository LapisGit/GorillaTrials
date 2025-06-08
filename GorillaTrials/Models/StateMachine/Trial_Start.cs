namespace GorillaTrials.Models.StateMachine
{
    public class Trial_Start : TrialState
    {
        public Trial_Start(Trial trial) : base(trial)
        {
            ;
        }

        public override void Enter()
        {
            base.Enter();

            VRRig.LocalRig.PlayTagSoundLocal(0, 0.25f, true);

            Trial.stopwatch.Restart();

            Trial.trialObject.SetActive(false);

            if (Trial.TrialType == (int)ETrialType.Box)
            {
                Trial.stateMachine.SwitchState(new Trial_Boxes(Trial));
                return;
            }

            Trial.stateMachine.SwitchState(new Trial_End(Trial, false));
        }
    }
}
