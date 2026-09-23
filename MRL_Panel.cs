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
        //private Text bestTimesText;
        private Image panelImage;
        //private Image carImage;
        private Text deadlineText;
        private Text descriptionText;
        private DateTime deadline;
        private bool isSeason;

        //const string CAR_SPRITES_PATH = ".Data.Cars.";

        //string carName = CarManager.GetCurrentCarsListForClass(carClass)[carIndex].prefabName;
        //Sprite result = carSprites.Find(item => item.name == carName);

        //carSprites = new List<Sprite>();
        //    string[] resourcesPaths = assembly.GetManifestResourceNames();
        //string carsRootPath = modFolderName + CAR_SPRITES_PATH;
        //int carsCount = 0;

        //    foreach (string path in resourcesPaths)
        //    {
        //        // load car sprites
        //        if (!path.Contains(CAR_SPRITES_PATH)) // skip non car paths
        //            continue;

        //        carsCount++;
        //        LoadCarSprite(assembly, path, carsRootPath);
        //    }

        //private void LoadCarSprite(Assembly assembly, string path, string carsRootPath)
        //{
        //    using (Stream stream = assembly.GetManifestResourceStream(path))
        //    {
        //        if (stream == null)
        //        {
        //            Main.Error("Couldn't read local file at path : " + path + ". Make sure the files have been included in the build.");
        //            return;
        //        }

        //        byte[] data;

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            stream.CopyTo(memoryStream);
        //            data = memoryStream.ToArray();
        //        }

        //        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        //        texture.LoadImage(data);

        //        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
        //        sprite.name = Path.GetFileNameWithoutExtension(path.Replace(carsRootPath, ""));
        //        carSprites.Add(sprite);
        //    }
        //}

        public void Setup(Font boldFont, Font standardFont, Action PopCarChoicePanel)
        {
            titleText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
            flagImage = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
            subTitleText = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            stagesText = transform.GetChild(1).GetComponent<Text>();
            //bestTimesText = transform.GetChild(2).GetComponent<Text>();
            panelImage = transform.GetChild(3).GetChild(0).GetComponent<Image>();
            //carImage = transform.GetChild(4).GetComponent<Image>();
            deadlineText = transform.GetChild(5).GetComponent<Text>();
            descriptionText = transform.GetChild(6).GetComponent<Text>();

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

            //bestTimesText.font = standardFont;
            //bestTimesText.gameObject.AddComponent<StyleText>();
            //bestTimesText.fontSize = StyleConstants.Text.Header1.GetFontSize(uiScale);
            //bestTimesText.lineSpacing = 1.5f;

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
            //bestTimesText
            panelImage.sprite = infos.currentRally.panel;
            //carImage

            if (deadline == default)
                deadline = DateTimeOffset.FromUnixTimeSeconds(infos.currentRally.deadline).DateTime;

            descriptionText.text = infos.currentRally.description;
            CustomEventManager.StartRecording();
        }

        private string FormatStageName(Stage stage) => stage.Name.Substring(0, 1).ToUpper() + stage.Name.Substring(1);
    }
}
