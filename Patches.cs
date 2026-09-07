using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MRL
{
    // Patch model
    // [HarmonyPatch(typeof(), nameof())]
    // [HarmonyPatch(typeof(), MethodType.)]
    // static class type_method_Patch
    // {
    // 	static void Prefix()
    // 	{
    // 		//
    // 	}

    // //	this will negate the method
    // //  	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    // //  	{
    // //      	foreach (var instruction in instructions)
    // //          	yield return new CodeInstruction(OpCodes.Ret);
    // //  	}

    // 	static void Postfix()
    // 	{
    // 		//
    // 	}
    // }

    // TODO : Remove the menu option on toggle ?

    [HarmonyPatch(typeof(RallyData), nameof(RallyData.SetRallyComplete))]
    static class CompletionDetector
    {
        static void Prefix()
        {
            Main.Try("CompletionDetector", () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM && EventsManager.IsRecording)
                {
                    Driver player = GameModeManager.GetSeasonDataCurrentGameMode().DriverList.Find(item => item.isPlayer);
                    EventsManager.WriteResults(player.GetResultsForCurrentRally());
                }
            });
        }
    }

    [HarmonyPatch(typeof(PanelManager), nameof(PanelManager.Start))]
    static class CustomButtonBuilder
    {
        static void Postfix(PanelManager __instance)
        {
            Main.Try(nameof(CustomButtonBuilder), () =>
            {
                // error message is already logged on fetching
                if (EventsManager.serverInfos == null)
                    return;

                Transform panelRoot = __instance.OnlineEventsSelect.transform;
                CustomButton[] buttons = panelRoot.transform.GetChild(0).GetComponentsInChildren<CustomButton>();
                CustomButton newButton = GameObject.Instantiate(buttons[buttons.Length - 1], panelRoot.transform.GetChild(0));
                newButton.name = "Masters of rally league (Button)";

                Navigation nav = buttons[0].navigation;
                nav.selectOnUp = newButton;
                buttons[0].navigation = nav;

                nav = buttons[buttons.Length - 1].navigation;
                nav.selectOnDown = newButton;
                buttons[buttons.Length - 1].navigation = nav;

                nav = newButton.navigation;
                nav.selectOnUp = buttons[buttons.Length - 1]; // on down is already correct
                newButton.navigation = nav;

                newButton.onClick = new Button.ButtonClickedEvent();

                newButton.onClick.AddListener(() =>
                {
                    Main.Try("Custom rally setup", () =>
                    {
                        // TODO : Future cool screen to show infos would be here instead of pop season directly
                        __instance.AddPanelAddToHistory(__instance.CarChooserPanel);
                        // TODO : Select car class here ?
                        GameObject.FindObjectOfType<CarChooserHelper>().InitDisplayClass();

                        GameModeManager.SetGameMode(GameModeManager.GAME_MODES.CUSTOM);
                        GameModeManager.RallyManager.SeasonData = EventsManager.serverInfos.GenerateSeason();
                        EventsManager.StartRecording();
                    });
                });

                newButton.GetComponentInChildren<Text>().text = "masters of rally league";
            });
        }
    }

    [HarmonyPatch(typeof(PauseScreen), "OnEnable")]
    static class PauseRestartsRemover
    {
        static void Postfix(PauseScreen __instance)
        {
            Main.Try(nameof(PauseRestartsRemover), () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM &&
                    EventsManager.IsRecording &&
                    !Main.settings.trainingMode)
                    ButtonUtilities.HideButtonAndUpdateNavigation(__instance.RestartButton);
            });
        }
    }

    [HarmonyPatch(typeof(PostStageScreen), nameof(PostStageScreen.ForceUpdateOfUI))]
    static class EndRestartRemover
    {
        static void Postfix(PostStageScreen __instance)
        {
            Main.Try(nameof(EndRestartRemover), () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM &&
                    EventsManager.IsRecording &&
                    !Main.settings.trainingMode)
                    ButtonUtilities.HideButtonAndUpdateNavigation(__instance.RestartButton);
            });
        }
    }
}
