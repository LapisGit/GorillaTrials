using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Behaviours;

public class TimeManager : MonoBehaviour
{
    public float maxTime = 0f;
    public bool timeLimit = false;
    public void Update()
    {
        if (TrialManager.Instance.currentTrial.stopwatch.Elapsed.TotalSeconds >= maxTime && TrialManager.Instance.Started && timeLimit);
        {
            TrialManager.Instance.currentTrial.stateMachine.SwitchState(new Trial_End(TrialManager.Instance.currentTrial, false));
            Logging.Info("time limit reached, ending trial early...");
        }
    }
}