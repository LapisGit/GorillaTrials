using GorillaTrials.Behaviours;
using GorillaTrials.Tools;
using HarmonyLib;
using Photon.Pun.UtilityScripts;

namespace GorillaTrials.Patches
{
    [HarmonyPatch(typeof(CustomMapLoader))]
    [HarmonyPatch("UnloadSceneCoroutine",MethodType.Enumerator)]
    internal class MapUnloadPatch 
    {
        static void Postfix()
        { 
            Logging.Info("MapUnloadPatch Postfix");
            CustomMapManager.instance.DestroyAllTrialsFromCustomMap();
        }
    }
}