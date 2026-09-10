using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static AreaManager;
using static Car;
using static StyleText;

namespace MRL
{
    /// <summary>Screen that displays infos about MRL events</summary>
    public class MRL_Panel : Panel
    {
        private const string STYLE_PROP_NAME = "_scaleType";

        private Button mainButton;
        private Text titleText;
        private Text stagesText;
        private Text deadlineText;
        private Text descriptionText;
        private bool isSeason;

        public void Setup(Font boldFont, Font standardFont)
        {
            titleText = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            stagesText = transform.GetChild(1).GetComponent<Text>();
            deadlineText = transform.GetChild(3).GetComponent<Text>();
            descriptionText = transform.GetChild(4).GetComponent<Text>();

            UIScale uiScale = StyleManager.Instance().UIScale;

            titleText.font = boldFont;
            StyleText style = titleText.gameObject.AddComponent<StyleText>();
            Main.SetField(style, STYLE_PROP_NAME, BindingFlags.Instance, TextType.StageTitle);
            titleText.fontSize = StyleConstants.Text.StageTitle.GetFontSize(uiScale);

            stagesText.font = standardFont;
            style = stagesText.gameObject.AddComponent<StyleText>();
            Main.SetField(style, STYLE_PROP_NAME, BindingFlags.Instance, TextType.Header1);
            stagesText.fontSize = StyleConstants.Text.Header1.GetFontSize(uiScale);

            deadlineText.font = boldFont;
            style = deadlineText.gameObject.AddComponent<StyleText>();
            Main.SetField(style, STYLE_PROP_NAME, BindingFlags.Instance, TextType.StageTitle);
            deadlineText.fontSize = StyleConstants.Text.StageTitle.GetFontSize(uiScale);

            descriptionText.font = standardFont;
            style = descriptionText.gameObject.AddComponent<StyleText>();
            Main.SetField(style, STYLE_PROP_NAME, BindingFlags.Instance, TextType.Standard);
            descriptionText.fontSize = StyleConstants.Text.Standard.GetFontSize(uiScale);

            mainButton = transform.GetChild(5).GetComponent<Button>();
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() =>
            {
                Main.Try("Setup MRL rally", () =>
                {
                    Main.Log("1");
                    // TODO : Test the order of this to make sure it works
                    ServerInfo info = CustomEventManager.serverInfos;
                    Main.Log("2");
                    GameModeManager.SetGameMode(GameModeManager.GAME_MODES.CUSTOM);
                    Main.Log("3");
                    GameModeManager.RallyManager.SeasonData = info.GenerateSeason();
                    Main.Log("4");

                    CarClass group = isSeason ? info.currentSeason.group : info.currentRally.openClassGroup;
                    Main.Log("5");
                    CarManager.SetChosenClass(group);
                    Main.Log("6");
                    SaveGame.Save();
                    Main.Log("7");

                    CarChooserHelper chooser = GameObject.FindObjectOfType<CarChooserHelper>();
                    Main.Log("8");
                    ServerInfo infos = CustomEventManager.serverInfos;
                    Main.Log("9");
                    chooser.GroupTitle.carClass = group;
                    Main.Log("10");
                    chooser.InitHideClass();
                    Main.Log("11");
                    chooser.GroupTitle.ConstructString(group, (isSeason ? "Season" : "Open class") + " rally");
                    Main.Log("12");
                });
            });

            FirstSelected = mainButton.gameObject;
            OnPanelPushedEvent = new UnityEngine.Events.UnityEvent();

            // TODO : How do I come back when we press B ?
        }

        public void ShowInfos(bool isSeason)
        {
            this.isSeason = isSeason;
            ServerInfo infos = CustomEventManager.serverInfos;

            // TODO : Here we should get the flag from the game's leaderboard system
            titleText.text = $"{infos.currentRally.name}\n{infos.currentRally.country} - {infos.currentSeason.year}";

            Dictionary<Areas, Area> areaDictionary = Main.GetField<Dictionary<Areas, Area>, AreaManager>(
                null,
                "areaDictionary",
                System.Reflection.BindingFlags.Static
            );

            List<Stage> stageList = areaDictionary[infos.currentRally.location].stageList;
            string stages = "Stages :";

            for (int i = 0; i < infos.currentRally.stageIndeces.Length; i++)
            {
                stages += $"\n- <b>{FormatStageName(stageList[infos.currentRally.stageIndeces[i]])}" +
                    $"</b> : {infos.currentRally.stageWeathers[i]}";
            }

            stagesText.text = stages;
            string suffix = string.Empty;

            if (infos.currentRally.deadline.Contains("st"))
                suffix = "st";

            if (infos.currentRally.deadline.Contains("nd"))
                suffix = "nd";

            if (infos.currentRally.deadline.Contains("rd"))
                suffix = "rd";

            if (infos.currentRally.deadline.Contains("th"))
                suffix = "th";

            deadlineText.text = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.Parse(infos.currentRally.deadline.Replace(suffix, "")).AddHours(12),
                TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time")
            ).ToLocalTime().ToString("dd MMMM yyyy, HH:mm").Insert(2, suffix);

            descriptionText.text = infos.currentRally.description;
            CustomEventManager.StartRecording();
        }

        private string FormatStageName(Stage stage) => stage.Name.Substring(0, 1).ToUpper() + stage.Name.Substring(1);
    }
}
