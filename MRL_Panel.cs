using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static Sprite seasonCar;
        private static Sprite openClassCar;

        const string CAR_SPRITES_PATH = ".Data.Cars.";

        private Button mainButton;
        private Text titleText;
        private Image flagImage;
        private Text subTitleText;
        private Text stagesText;
        private Text currentTimesText;
        private Image panelImage;
        private Image carImage;
        private Text deadlineText;
        private Text descriptionText;
        private DateTime deadline;
        private bool isSeason;

        // TODO : Skip car selection and force select car (if not in training mode AND has car from server)

        public void Setup(Font boldFont, Font standardFont, Action PopCarChoicePanel)
        {
            titleText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
            flagImage = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
            subTitleText = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            stagesText = transform.GetChild(1).GetComponent<Text>();
            currentTimesText = transform.GetChild(2).GetComponent<Text>();
            //transform.GetChild(2).GetComponent<Text>().enabled = false; // TEST
            panelImage = transform.GetChild(3).GetChild(0).GetComponent<Image>();
            carImage = transform.GetChild(4).GetComponent<Image>();
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

            currentTimesText.font = standardFont;
            currentTimesText.gameObject.AddComponent<StyleText>();
            currentTimesText.fontSize = StyleConstants.Text.Header1.GetFontSize(uiScale);
            currentTimesText.lineSpacing = 1.5f;

            deadlineText.font = boldFont;
            deadlineText.gameObject.AddComponent<StyleText>();
            deadlineText.fontSize = StyleConstants.Text.StageTitle.GetFontSize(uiScale);

            descriptionText.font = standardFont;
            descriptionText.gameObject.AddComponent<StyleText>();
            descriptionText.fontSize = StyleConstants.Text.Standard.GetFontSize(uiScale);

            mainButton = transform.GetChild(7).GetComponent<Button>();
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() =>
            {
                Main.Try("Setup MRL rally", () =>
                {
                    // set season
                    ServerInfo info = CustomEventManager.serverInfos;
                    GameModeManager.SetGameMode(GameModeManager.GAME_MODES.CUSTOM);
                    GameModeManager.RallyManager.SeasonData = info.GenerateSeason();

                    // set group
                    CarClass group = isSeason ? info.currentSeason.group : info.currentRally.openClassGroup;
                    CarManager.SetChosenClass(group);
                    SaveGame.Save();

                    // setup chooser
                    CarChooserHelper helper = GameObject.FindObjectOfType<CarChooserHelper>();
                    ServerInfo infos = CustomEventManager.serverInfos;
                    helper.GroupTitle.carClass = group;
                    helper.InitHideClass();
                    helper.GroupTitle.ConstructString(group, (isSeason ? "Season" : "Open class") + " rally");

                    // pre-select car
                    //int carIndex = isSeason ? CustomEventManager.seasonCar : CustomEventManager.openClassCar;

                    //if (carIndex != -1)
                    //{
                    //    helper.CarButton.index = carIndex;
                    //    CarManager.SetChosenCar(carIndex);
                    //    helper.CarButton.carChooserManager.ChangeCar(carIndex);
                    //    helper.CarButton.UpdateOptionTextAndArrows();
                    //    Main.InvokeMethod(helper.CarButton, "UpdateCarSpecs", BindingFlags.Instance, null);
                    //    helper.LiveryButton.UpdateIndexOfLivery();
                    //    helper.LiveryButton.UpdateOptionTextAndArrows();

                    //    helper.CarButton.gameObject.SetActive(false);
                    //    helper.CarButton.enabled = false;
                    //    helper.panel.FirstSelected = helper.LiveryButton.gameObject;
                    //    // TODO : Disable random button
                    //    // TODO : I'm getting the liveries from the 1st car instead of what I selected
                    //    //EventSystem.current.SetSelectedGameObject(helper.LiveryButton.gameObject);
                    //}

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

            Country country = Countries.GetCountryByPartialShortName(infos.currentRally.country)[0];
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

            string[] results = isSeason ? CustomEventManager.seasonResults : CustomEventManager.openClassResults;
            currentTimesText.enabled = results != null && results.Length > 0;

            if (currentTimesText.enabled)
                currentTimesText.text = "\n" + string.Join("\n", results);

            panelImage.sprite = infos.currentRally.panel;

            carImage.enabled = isSeason ? seasonCar != null : openClassCar != null;
            carImage.sprite = isSeason ? seasonCar : openClassCar;

            if (deadline == default)
                deadline = DateTimeOffset.FromUnixTimeSeconds(infos.currentRally.deadline).DateTime;

            descriptionText.text = infos.currentRally.description;
            CustomEventManager.StartRecording();
        }

        private string FormatStageName(Stage stage) => stage.Name.Substring(0, 1).ToUpper() + stage.Name.Substring(1);

        public static void LoadCarSprites()
        {
            seasonCar = LoadCarSprite(CustomEventManager.seasonCar, true);
            openClassCar = LoadCarSprite(CustomEventManager.openClassCar, false);
        }

        private static Sprite LoadCarSprite(int carIndex, bool isSeason)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string carsRootPath = Main.modFolderName + CAR_SPRITES_PATH;
            string[] resourcesPaths = assembly.GetManifestResourceNames();

            Car selectedCar = CarManager.GetCurrentCarsListForClass(isSeason ?
                CustomEventManager.serverInfos.currentSeason.group :
                CustomEventManager.serverInfos.currentRally.openClassGroup
            )[carIndex];

            if (selectedCar == null)
            {
                Main.Error("Couldn't find Car for index \"" + carIndex + "\"");
                return null;
            }

            string picturePath = resourcesPaths.FirstOrDefault(item =>
                item.Contains(CAR_SPRITES_PATH) && item.Contains(selectedCar.prefabName));

            if (string.IsNullOrEmpty(picturePath))
            {
                Main.Error("Couldn't find resource path for car at index \"" + carIndex +
                    "\", did you include that car picture in the files ?");
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(picturePath))
            {
                if (stream == null)
                {
                    Main.Error("Couldn't read local file at path \"" + picturePath +
                        "\". Make sure the files have been included in the build.");

                    return null;
                }

                byte[] data;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(data);

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
                sprite.name = selectedCar.name;

                return sprite;
            }
        }
    }
}
