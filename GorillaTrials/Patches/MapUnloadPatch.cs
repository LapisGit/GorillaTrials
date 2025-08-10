using GorillaTrials.Behaviours;
using HarmonyLib;

namespace GorillaTrials.Patches
{
    [HarmonyPatch(typeof(CustomMapLoader), nameof(CustomMapLoader.UnloadSceneCoroutine), MethodType.Enumerator)]
    internal class MapUnloadPatch
    {
        private static void Postfix()
        {
            CustomMapManager.instance.DestroyAllTrialsFromCustomMap();
        }
    }
}