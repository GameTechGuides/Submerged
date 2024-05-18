using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Systems.Oxygen.Patches;

[HarmonyPatch]
public static class SoundPatches
{
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
    [HarmonyPrefix]
    public static void ReplaceO2SabotageSoundWithAirshipPatch([HarmonyArgument(0)] ref AudioClip clip, [HarmonyArgument(2)] ref float volume)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (clip != MapLoader.Skeld.SabotageSound) return;

        SubmarineOxygenSystem system = SubmarineOxygenSystem.Instance;

        if (system.recentlyActive > 0)
        {
            clip = MapLoader.Airship.SabotageSound;

            if (system.playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId))
            {
                volume = 0.2f;
            }
            else if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                volume = 0.5f;
            }
            else
            {
                volume = 0.8f;
            }
        }
    }

    // Add a new patch for ReportButton.SetActive method to disable the report button during lights-out
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.SetActive))]
    [HarmonyPrefix]
    public static bool ReportButtonSetActivePatch(ReportButton __instance, bool value)
    {
        if (SubmarineOxygenSystem.Instance.CheckOxygenSabotageStatus())
        {
            __instance.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.5f); // Add a visual indicator
            return false; // Prevent the report button from being activated
        }
        return true; // Allow normal behavior if not during lights-out
    }
}
