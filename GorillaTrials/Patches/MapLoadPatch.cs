using GorillaTrials.Behaviours;
using HarmonyLib;

namespace GorillaTrials.Patches
{
    [HarmonyPatch(typeof(CustomMapLoader))]
    [HarmonyPatch("OnLoadComplete")]
    internal class MapLoadPatch
    {

        static void Postfix()
        {
            CustomMapManager.instance.LoadTrialsFromScene();
        }
    }
}