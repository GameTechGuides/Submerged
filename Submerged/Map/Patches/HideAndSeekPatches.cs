using System.Linq;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Systems.Elevator;
using IntroCutscene_CoBegin = IntroCutscene._CoBegin_d__35;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class HideAndSeekPatches
{
    [HarmonyPatch(typeof(HideAndSeekManager), nameof(HideAndSeekManager.StartGame))]
    [HarmonyPostfix]
    public static void DisableConsolesPatch()
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        ShipStatus.Instance.GetComponentsInChildren<SystemConsole>()
            .Where(c => c.MinigamePrefab.name.StartsWith("CamsSabotageMinigame"))
            .Do(c => c.enabled = false);

        ShipStatus.Instance.GetComponentsInChildren<ElevatorConsole>()
            .Do(c =>
            {
                c.enabled = false;
                c.UsableDistance = -1;
            });

        SubmarineStatus.instance.elevators.Do(e =>
        {
            e.system.tandemSystemType = default;
            e.system.upperDeckIsTargetFloor = SubmergedHnSManager.CurrentGameIsOnUpperDeck;
        });
    }

    [HarmonyPatch(typeof(IntroCutscene_CoBegin), nameof(IntroCutscene_CoBegin.MoveNext))]
    [HarmonyPrefix]
    public static bool DisplaySpawnInBeforeGameStartsPatch(IntroCutscene_CoBegin __instance, ref bool __result)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return true;
        if (!GameManager.Instance.IsHideAndSeek()) return true;

        switch (__instance.__1__state)
        {
            case 5:
                __instance.__1__state = -1;

                __instance._playerSlot_5__3.gameObject.SetActive(false);
                __instance.__4__this.HideAndSeekPanels.SetActive(false);
                __instance.__4__this.CrewmateRules.SetActive(false);
                __instance.__4__this.ImpostorRules.SetActive(false);

                __instance.__2__current = ShipStatus.Instance.PrespawnStep().Cast<CppObject>();

                __instance.__1__state = 1337;
                __result = true;

                return false;

            case 1337:
                __instance.__1__state = 5;

                return true;

            default:
                return true;
        }
    }

    // Add logic to display indicators for unusable vents during hide and seek mode
    [HarmonyPatch(typeof(LogicUsablesHnS), nameof(LogicUsablesHnS.CanUse))]
    [HarmonyPostfix]
    public static void MarkUnusableVentsPatch(ref bool __result, [HarmonyArgument(0)] IUsable usable, [HarmonyArgument(1)] PlayerControl player)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (!GameManager.Instance.IsHideAndSeek()) return;

        // Check if the usable is a vent and if it's one of the vents that should be marked as unusable
        if (usable.TryCast<Vent>() is { } vent && (vent.Id == UPPER_CENTRAL_VENT_ID || vent.Id == LOWER_CENTRAL_VENT_ID || vent.Id == ADMIN_VENT_ID))
        {
            // Display the wet floor sign for the admin vent and caution tape for central vents
            SpriteRenderer ventRenderer = vent.GetComponent<SpriteRenderer>();
            if (vent.Id == ADMIN_VENT_ID)
            {
                ventRenderer.sprite = AssetLoader.GetSprite("WetFloorSign");
            }
            else
            {
                ventRenderer.sprite = AssetLoader.GetSprite("CautionTape");
            }

            // Ensure the indicators are only visible in hide and seek mode
            __result = false;
        }
    }
}
