using HarmonyLib;
using Photon.Pun;
using UnityEngine.UI;

namespace WristButtons.Buttons
{
    [HarmonyPatch(typeof(GorillaScoreboardSpawner))]
    [HarmonyPatch("IsCurrentScoreboard", MethodType.Normal)]
    class ScoreBoardPatch
    {
        static bool Prefix(ref bool __result, GorillaScoreboardSpawner __instance)
        {
            //if(__instance == Main.wristScoreBoardSpawner)
            //{
            //    __result = true;
            //    return false;
            //}
            return true;
        }
    }
}