using GorillaTrials.Behaviours;
using HarmonyLib;

namespace GorillaTrials.Patches
{
    [HarmonyPatch(typeof(CustomMapLoader), nameof(CustomMapLoader.OnLoadComplete))]
    internal class MapLoadPatch
    {
        private static async void Postfix()
        {
            await CustomMapManager.instance.CheckIfApprovedMap(CustomMapLoader.LoadedMapModId);
        }
    }
}