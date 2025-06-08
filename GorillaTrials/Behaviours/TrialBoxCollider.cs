namespace GorillaTrials.Behaviours
{
    public class TrialBoxCollider : GorillaTriggerBox
    {
        public override void OnBoxTriggered()
        {
            base.OnBoxTriggered();

            if (Singleton<TrialManager>.HasInstance && Singleton<TrialManager>.Instance.Started)
                Singleton<TrialManager>.Instance.CurrentTrial.stateMachine.CurrentState.BoxTriggered(this);
        }
    }
}
