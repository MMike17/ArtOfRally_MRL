using System;
using System.Collections.Generic;
using System.Reflection;
using Bia.Countries.Iso3166;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AreaManager;
using static Car;

namespace MRL
{
    /// <summary>Screen that displays infos about MRL events</summary>
    public class MRL_Panel : Panel
    {
        private Button mainButton;
        private Text titleText;
        private Image flagImage;
        private Text subTitleText;
        private Text stagesText;
        private Image panelImage;
        private Text deadlineText;
        private Text descriptionText;
        private DateTime deadline;
        private bool isSeason;

        public void Setup(Font boldFont, Font standardFont, Action PopCarChoicePanel)
        {
            titleText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
            flagImage = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
            subTitleText = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            stagesText = transform.GetChild(1).GetComponent<Text>();
            panelImage = transform.GetChild(2).GetChild(0).GetComponent<Image>();
            deadlineText = transform.GetChild(3).GetComponent<Text>();
            descriptionText = transform.GetChild(4).GetComponent<Text>();

            UIScale uiScale = StyleManager.Instance().UIScale;

            titleText.font = boldFont;
            titleText.gameObject.AddComponent<StyleText>();
            titleText.fontSize = Mathf.RoundToInt(StyleConstants.Text.StageTitle.GetFontSize(uiScale));

            subTitleText.font = boldFont;
            subTitleText.gameObject.AddComponent<StyleText>();
            subTitleText.fontSize = Mathf.RoundToInt(Mathf.Lerp(
                StyleConstants.Text.StageTitle.GetFontSize(uiScale),
                StyleConstants.Text.Header1.GetFontSize(uiScale),
                0.66f
            ));

            stagesText.font = standardFont;
            stagesText.gameObject.AddComponent<StyleText>();
            stagesText.fontSize = StyleConstants.Text.Header1.GetFontSize(uiScale);
            stagesText.lineSpacing = 1.5f;

            deadlineText.font = boldFont;
            deadlineText.gameObject.AddComponent<StyleText>();
            deadlineText.fontSize = StyleConstants.Text.StageTitle.GetFontSize(uiScale);

            descriptionText.font = standardFont;
            descriptionText.gameObject.AddComponent<StyleText>();
            descriptionText.fontSize = StyleConstants.Text.Standard.GetFontSize(uiScale);

            mainButton = transform.GetChild(5).GetComponent<Button>();
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() =>
            {
                Main.Try("Setup MRL rally", () =>
                {
                    ServerInfo info = CustomEventManager.serverInfos;
                    GameModeManager.SetGameMode(GameModeManager.GAME_MODES.CUSTOM);
                    GameModeManager.RallyManager.SeasonData = info.GenerateSeason();

                    CarClass group = isSeason ? info.currentSeason.group : info.currentRally.openClassGroup;
                    CarManager.SetChosenClass(group);
                    SaveGame.Save();

                    CarChooserHelper chooser = GameObject.FindObjectOfType<CarChooserHelper>();
                    ServerInfo infos = CustomEventManager.serverInfos;
                    chooser.GroupTitle.carClass = group;
                    chooser.InitHideClass();
                    chooser.GroupTitle.ConstructString(group, (isSeason ? "Season" : "Open class") + " rally");

                    PopCarChoicePanel?.Invoke();
                });
            });

            FirstSelected = mainButton.gameObject;

            OnPanelPushedEvent = new UnityEngine.Events.UnityEvent();
            OnPanelPushedEvent.AddListener(() => EventSystem.current.SetSelectedGameObject(mainButton.gameObject));
            OnPanelPoppedEvent = new UnityEngine.Events.UnityEvent();
        }

        private void Update()
        {
            TimeSpan delay = deadline.ToLocalTime() - DateTime.Now;
            string delayText = $"{(delay.Days > 0 ? delay.Days + " days " : "")}{delay.Hours}:{delay.Minutes}:{delay.Seconds}";
            deadlineText.text = $"{deadline.ToLocalTime().ToString("dd MMMM yyyy, HH:mm")}\n{delayText}";
        }

        public void ShowInfos(bool isSeason)
        {
            this.isSeason = isSeason;
            ServerInfo infos = CustomEventManager.serverInfos;

            titleText.text = $"{infos.currentRally.name}";
            subTitleText.text = $"{infos.currentRally.country} - {infos.currentSeason.year} - " +
                $"{(isSeason ? "Season" : "Open class")} rally";

            Country country = Countries.GetCountryByPartialFullName(infos.currentRally.country)[0];
            flagImage.sprite = Resources.Load<Sprite>($"Sprites/CountryFlags/{country.Alpha2}");

            Dictionary<Areas, Area> areaDictionary = Main.GetField<Dictionary<Areas, Area>, AreaManager>(
                null,
                "areaDictionary",
                BindingFlags.Static
            );

            List<Stage> stageList = areaDictionary[infos.currentRally.location].stageList;
            string stages = "Stages :";

            for (int i = 0; i < infos.currentRally.stageIndeces.Length; i++)
            {
                stages += $"\n- <b>{FormatStageName(stageList[infos.currentRally.stageIndeces[i]])}" +
                    $"</b> : {infos.currentRally.stageWeathers[i]}";
            }

            stagesText.text = stages;
            panelImage.sprite = infos.currentRally.panel;

            if (deadline == default)
                deadline = DateTimeOffset.FromUnixTimeSeconds(infos.currentRally.deadline).DateTime;

            descriptionText.text = infos.currentRally.description;
            CustomEventManager.StartRecording();
        }

        private string FormatStageName(Stage stage) => stage.Name.Substring(0, 1).ToUpper() + stage.Name.Substring(1);
    }
}
