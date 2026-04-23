using GorillaTrials.Behaviours;

namespace GorillaTrials.Models.StateMachine
{
    public class TrialState(Trial trial)
    {
        public Trial Trial => trial;

        protected Trial trial = trial;

        protected bool initialized;

        public virtual void Initialize()
        {
            initialized = true;
        }

        public virtual void Enter()
        {
            if (!initialized)
            {
                Initialize();
                return;
            }

            Resume();
        }

        public virtual void Resume()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {

        }

        public virtual void BoxTriggered(TrialBoxCollider box)
        {

        }
    }
}
