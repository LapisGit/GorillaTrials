using GorillaTrials.Tools;

namespace GorillaTrials.Models.StateMachine
{
    public class TrialStateMachine
    {
        public TrialState CurrentState => currentState;

        protected TrialState currentState;

        public void SwitchState(TrialState newState)
        {
            if (newState is null)
            {
                Logging.Fatal("NEW STATE IS NULL, JIGSAW!!!");
                return;
            }

            if (currentState is not null)
            {
                currentState.Exit();
                ;
            }

            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }
    }
}
