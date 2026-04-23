using GorillaTrials.Models.StateMachine;
using UnityEngine;

namespace GorillaTrials.Behaviours;

public class EarlyEnd : MonoBehaviour
{
    private float bothButtonsHeldTime = 0f;
    private readonly float requiredHoldDuration = Plugin.EarlyEndTime.Value;
    public void Update()
    {
        bool leftHeld = ControllerInputPoller.instance.leftControllerSecondaryButton;
        bool rightHeld = ControllerInputPoller.instance.rightControllerSecondaryButton;

        if (rightHeld || leftHeld)
        {
            bothButtonsHeldTime += Time.deltaTime;
            if (bothButtonsHeldTime >= requiredHoldDuration)
            {
                bothButtonsHeldTime = 0f;
                OnBothButtonsHeld();
            }
        }
        else
        {
            bothButtonsHeldTime = 0f;
        }
    }

    private void OnBothButtonsHeld()
    {
        if (TrialManager.Instance.Started == false)
        {
            return;
        }
        TrialManager.Instance.currentTrial.stateMachine.SwitchState(new Trial_End(TrialManager.Instance.currentTrial, false));
    }
}