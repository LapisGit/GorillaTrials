using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Behaviours;

// make this actually work without breaking for some reason lol
// probably not needed on release
public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public float maxTime = 0f;

    private void Start()
    {
        instance = this;
    }

    public void Update()
    {
        if (TrialManager.Instance.Started)
        {
            Logging.Info(TrialManager.Instance.currentTrial.stopwatch.Elapsed.TotalSeconds);
            Logging.Info(maxTime);
            if (maxTime == 0)
            {
                return;
            }
            if (TrialManager.Instance.currentTrial.stopwatch.Elapsed.TotalSeconds > maxTime)
            {
                TrialManager.Instance.currentTrial.stateMachine.SwitchState(new Trial_End(TrialManager.Instance.currentTrial, false));
                Logging.Info("time limit reached, ending trial early...");
            }
        }
    }
}