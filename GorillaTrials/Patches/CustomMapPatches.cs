using GorillaTrials.Behaviours;
using HarmonyLib;

namespace GorillaTrials.Patches
{
    [HarmonyPatch(typeof(CustomMapLoader), nameof(CustomMapLoader.OnInitialLoadComplete))]
    internal class MapLoadPatch
    {
        private static async void Postfix()
        {
            await CustomMapManager.instance.CheckIfApprovedMap(CustomMapLoader.LoadedMapModId);
        }
    }
    
    [HarmonyPatch(typeof(CustomMapLoader), nameof(CustomMapLoader.UnloadSceneCoroutine), MethodType.Enumerator)]
    internal class MapUnloadPatch
    {
        private static void Postfix()
        {
            CustomMapManager.instance.DestroyAllTrialsFromCustomMap();
        }
    }
}