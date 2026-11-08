using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static AreaManager;
using static Car;

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
        private static CustomButton waitingButton;

        static void Postfix(PanelManager __instance)
        {
            if (SceneManager.GetActiveScene().buildIndex != 3 || panel != null)
                return;

            CoroutineRunner.StartCoroutine(WaitForInfos(__instance, instance =>
            {
                Main.Try(nameof(MRLScreenBuilder), () =>
                {
                    Font boldFont = instance.MainPanel.transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().font;
                    Font standardFont = instance.GetComponentInChildren<VersionText>().GetComponent<Text>().font;

                    panel = Main.SpawnMRL_Panel(instance.transform);
                    panel.Setup(boldFont, standardFont, () => instance.AddPanelAddToHistory(instance.CarChooserPanel));

                    Transform parent = instance.OnlineEventsSelect.transform.GetChild(0);
                    List<CustomButton> buttons = new List<CustomButton>(parent.GetComponentsInChildren<CustomButton>());

                    buttons.Add(SpawnNewButton(
                        buttons[0],
                        parent,
                        "MRL season",
                        instance,
                        true,
                        () => ShowMRLPanel(panel, instance, true)
                    ));

                    buttons.Add(SpawnNewButton(
                        buttons[0],
                        parent,
                        "MRL open class",
                        instance,
                        false,
                        () => ShowMRLPanel(panel, instance, false)
                    ));

                    for (int i = 0; i < buttons.Count; i++)
                    {
                        Navigation currentNav = buttons[i].navigation;
                        currentNav.selectOnUp = buttons[i == 0 ? buttons.Count - 1 : i - 1];
                        currentNav.selectOnDown = buttons[i == buttons.Count - 1 ? 0 : i + 1];
                        buttons[i].navigation = currentNav;
                    }

                    Main.Log(nameof(MRLScreenBuilder) + " : Setup buttons");
                });
            }));
        }

        private static CustomButton SpawnNewButton(
            CustomButton model,
            Transform parent,
            string buttonText,
            PanelManager instance,
            bool isSeason,
            Action OnClicked
        )
        {
            CustomButton newButton = GameObject.Instantiate(model, parent);
            newButton.name = $"{buttonText} (Button)";
            newButton.GetComponentInChildren<Text>().text = buttonText;

            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() => OnClicked?.Invoke());

            return newButton;
        }

        private static void ShowMRLPanel(MRL_Panel panel, PanelManager instance, bool isSeason)
        {
            Main.Try("Show MRL panel", () =>
            {
                panel.ShowInfos(isSeason);
                instance.AddPanelAddToHistory(panel, true);
            });
        }

        private static IEnumerator WaitForInfos(PanelManager instance, Action<PanelManager> OnReceivedInfo)
        {
            if (CustomEventManager.serverInfos == null)
            {
                Transform parent = instance.OnlineEventsSelect.transform.GetChild(0);
                waitingButton = SpawnNewButton(
                    parent.GetChild(parent.childCount - 1).GetComponent<CustomButton>(),
                    parent,
                    "<i>waiting for server infos...</i>",
                    instance,
                    false,
                    null
                );

                Navigation nav = waitingButton.navigation;
                nav.selectOnUp = null;
                nav.selectOnDown = null;
                nav.selectOnRight = null;
                nav.selectOnLeft = null;
                waitingButton.navigation = nav;

                waitingButton.interactable = false;
                Main.Log(nameof(MRLScreenBuilder) + " : Waiting for server infos");

                bool failed = false;
                yield return CustomEventManager.FetchServerInfos(() => failed = true);

                if (failed)
                {
                    waitingButton.GetComponentInChildren<Text>().text = "<i>couldn't retrieve server info</i>";
                    yield break;
                }
            }

            if (waitingButton != null)
                GameObject.DestroyImmediate(waitingButton.gameObject);

            OnReceivedInfo?.Invoke(instance);
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

    [HarmonyPatch(typeof(PanelManager), nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
    static class PanelPatcher
    {
        const string RESULTS_PANEL = "ResultsScreen";

        [HarmonyPostfix]
        static void CheckPanel(Panel panel)
        {
            if (!Main.enabled && !CustomEventManager.IsRecording)
                return;

            Main.Try(nameof(CheckPanel), () =>
            {
                ServerInfo infos = CustomEventManager.serverInfos;

                if (panel.name == RESULTS_PANEL)
                {
                    Main.DelayCall(() =>
                    {
                        GroupTitle title = panel.GetComponentInChildren<GroupTitle>();
                        title.SetString(
                            infos.currentRally.name + " | " + infos.currentSeason.year + " | " + infos.currentRally.country);
                        panel.transform.Find("TextSubtitleStage").GetComponent<Text>().text = "Rally results";
                    });
                }
            });
        }
    }

    [HarmonyPatch(typeof(GroupTitle), nameof(GroupTitle.ConstructStringUsingCareerData))]
    static class TitlePatcher
    {
        static string lastTitle;

        [HarmonyPrefix]
        static bool FixTitle(GroupTitle __instance)
        {
            if (Main.enabled && CustomEventManager.IsRecording)
            {
                Main.Try(nameof(FixTitle), () =>
                {
                    ServerInfo info = CustomEventManager.serverInfos;
                    string divider = Main.GetField<string, GroupTitle>(__instance, "divider", BindingFlags.Instance);
                    lastTitle = info.currentRally.name + " " + divider + " " + info.currentSeason.year;
                    Main.SetField(__instance, "suffix", BindingFlags.Instance, lastTitle);
                    __instance.SetString(lastTitle);
                });

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StageIntroCinematic), "DoCinematic")]
    static class RallyIntroPatcher
    {
        public static StageIntroCinematic instance;

        static void Prefix(StageIntroCinematic __instance)
        {
            if (Main.enabled && CustomEventManager.IsRecording)
                instance = __instance;
        }
    }

    [HarmonyPatch(typeof(LeanTween))]
    static class RallyIntroFixer
    {
        [HarmonyPatch(nameof(LeanTween.alphaText), new[] { typeof(RectTransform), typeof(float), typeof(float) })]
        [HarmonyPostfix]
        static void FixRallyIntro()
        {
            if (Main.enabled && RallyIntroPatcher.instance != null && CustomEventManager.IsRecording)
            {
                Main.Try(nameof(FixRallyIntro), () =>
                {
                    bool checkText = RallyIntroPatcher.instance.FirstText.text ==
                        AreaManager.GetAreaStringLocalized(GameModeManager.GetRallyDataCurrentGameMode().CurrentArea).ToLower();

                    if (checkText)
                    {
                        ServerInfo.RallyInfo infos = CustomEventManager.serverInfos.currentRally;
                        RallyIntroPatcher.instance.FirstText.text = infos.country;
                        RallyIntroPatcher.instance.SecondText.text = infos.name;
                        Main.Log("Fixing rally title");
                    }
                });
            }
        }

        [HarmonyPatch(nameof(LeanTween.alphaCanvas))]
        [HarmonyPrefix]
        static void FixLeaderboardTitle()
        {
            if (Main.enabled && CustomEventManager.IsRecording && RallyResultPatcher.title != null)
            {
                Main.Try(nameof(FixLeaderboardTitle), () =>
                {
                    RallyResultPatcher.title.text = CustomEventManager.serverInfos.currentRally.name;
                });
            }
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "SetGameModeTitle", new Type[] { typeof(CarClass), typeof(int), typeof(Areas) })]
    static class LoadingPatcher
    {
        static bool Prefix(LoadingScreen __instance)
        {
            if (Main.enabled && CustomEventManager.IsRecording)
            {
                ServerInfo infos = CustomEventManager.serverInfos;

                Main.Try(nameof(LoadingPatcher), () =>
                {
                    __instance.Title.SetString(
                        infos.currentRally.name + " | " + infos.currentSeason.year + " | " + infos.currentRally.country
                    );
                });

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PreStageScreen), "BuildHeading")]
    static class PreStagePatcher
    {
        static void Postfix(PreStageScreen __instance)
        {
            if (Main.enabled && CustomEventManager.IsRecording)
            {
                string area = AreaManager.GetAreaStringLocalized(
                    GameModeManager.GetRallyDataCurrentGameMode().CurrentArea
                ).ToLower();

                __instance.panelTitleControl.heading_small.fontSize -= 5;
                __instance.panelTitleControl.SetHeading(
                    __instance.panelTitleControl.heading_small.text.Replace(area, CustomEventManager.serverInfos.currentRally.name)
                );
            }
        }
    }

    [HarmonyPatch(typeof(RallyCompleteScreen), "ShowYearForCustomEvent")]
    static class RallyResultPatcher
    {
        public static Text title;

        static void Prefix(RallyCompleteScreen __instance)
        {
            if (Main.enabled && CustomEventManager.IsRecording)
                title = __instance.LocationText;
        }
    }
}
