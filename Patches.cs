using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [HarmonyPatch(typeof(RallyData), nameof(RallyData.SetRallyComplete))]
    static class CompletionDetector
    {
        static void Prefix()
        {
            Main.Try("CompletionDetector", () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM && CustomEventManager.IsRecording)
                {
                    Driver player = GameModeManager.GetSeasonDataCurrentGameMode().DriverList.Find(item => item.isPlayer);
                    CustomEventManager.WriteResults(player.GetResultsForCurrentRally());
                }
            });
        }
    }

    [HarmonyPatch(typeof(PanelManager), nameof(PanelManager.Start))]
    static class MRLScreenBuilder
    {
        private static MRL_Panel panel;

        static void Postfix(PanelManager __instance)
        {
            Main.Try(nameof(MRLScreenBuilder), () =>
            {
                if (CustomEventManager.serverInfos == null || SceneManager.GetActiveScene().buildIndex != 3)
                    return;

                Font boldFont = __instance.MainPanel.transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().font;
                Font standardFont = __instance.GetComponentInChildren<VersionText>().GetComponent<Text>().font;

                panel = Main.SpawnMRL_Panel(__instance.transform);
                panel.Setup(boldFont, standardFont, () => __instance.AddPanelAddToHistory(__instance.CarChooserPanel));

                Transform panelRoot = __instance.OnlineEventsSelect.transform;
                List<CustomButton> buttons = new List<CustomButton>(
                    panelRoot.transform.GetChild(0).GetComponentsInChildren<CustomButton>());

                buttons.Add(SpawnNewButton(
                    buttons[buttons.Count - 1],
                    panelRoot.transform.GetChild(0),
                    "MRL season",
                    __instance,
                    true
                ));

                buttons.Add(SpawnNewButton(
                    buttons[buttons.Count - 1],
                    panelRoot.transform.GetChild(0),
                    "MRL open class",
                    __instance,
                    false
                ));

                for (int i = 0; i < buttons.Count; i++)
                {
                    Navigation currentNav = buttons[i].navigation;
                    currentNav.selectOnUp = buttons[i == 0 ? buttons.Count - 1 : i - 1];
                    currentNav.selectOnDown = buttons[i == buttons.Count - 1 ? 0 : i + 1];
                    buttons[i].navigation = currentNav;
                }
            });
        }

        private static CustomButton SpawnNewButton(
            CustomButton model,
            Transform parent,
            string buttonText,
            PanelManager instance,
            bool isSeason
        )
        {
            CustomButton newButton = GameObject.Instantiate(model, parent);
            newButton.name = $"{buttonText} (Button)";

            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                Main.Try("Show MRL panel", () =>
                {
                    panel.ShowInfos(isSeason);
                    instance.AddPanelAddToHistory(panel, true);
                });
            });

            newButton.GetComponentInChildren<Text>().text = buttonText;
            return newButton;
        }
    }

    [HarmonyPatch(typeof(PauseScreen), "OnEnable")]
    static class PauseRestartsRemover
    {
        static void Postfix(PauseScreen __instance)
        {
            Main.Try(nameof(PauseRestartsRemover), () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM && CustomEventManager.IsRecording)
                {
                    if (Main.settings.trainingMode)
                        CustomEventManager.MarkTraining();
                    else
                        ButtonUtilities.HideButtonAndUpdateNavigation(__instance.RestartButton);
                }
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
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM && CustomEventManager.IsRecording)
                {
                    if (Main.settings.trainingMode)
                        CustomEventManager.MarkTraining();
                    else
                        ButtonUtilities.HideButtonAndUpdateNavigation(__instance.RestartButton);
                }
            });
        }
    }
}
