using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MRL
{
    class CustomEventManager
    {
        const string INFO_FILE_URL = "https://gist.githubusercontent.com/MMike17/7b9ed3de87db0969d05877ac14f50fd5/raw/RallyEvent.txt";
        private const string SEASON_TAG = "  \"currentSeason\": ";
        private const string RALLY_TAG = "  \"currentRally\": ";
        private const string SEASON_CAR_TAG = "\"seasonCar\":";
        private const string OPEN_CAR_TAG = "\"openClassCar\":";
        private const string SEASON_RESULTS_TAG = "\"seasonResults\":[";
        private const string OPEN_RESULTS_TAG = "\"openClassResults\":[";
        private const string USER_ID_HEADER = "userID";
        private const string RESULTS_ENDPOINT = "submission";
        private const string CAR_ENDPOINT = "car";
        private const string TIMES_ENDPOINT = "times";
        private const int REQUEST_DELAY = 4;
        private const int REQUEST_TRIES = 4;

        const string RESULTS_FILE_NAME = "RallyResults.mrl";
        const int ENCRYPTION_KEY = 573; // TODO : This will get changed with rolling encryption

        public static bool IsRecording { get; private set; }
        public static ServerInfo serverInfos { get; private set; }
        public static int seasonCar { get; private set; }
        public static int openClassCar { get; private set; }
        public static string[] seasonResults { get; private set; }
        public static string[] openClassResults { get; private set; }

        private static bool inTraining;

        public static IEnumerator FetchAllInfos()
        {
            bool failed = false;
            yield return FetchServerInfos(() => failed = true);

            if (failed)
            {
                Main.Error("Aborted getting user car from discord bot");
                yield break;
            }

            yield return FetchUserCar();
            yield return FetchUserResults();
        }

        public static IEnumerator FetchServerInfos(Action OnFail)
        {
            Main.Log("Sending server infos request...");
            UnityWebRequest infoRequest = UnityWebRequest.Get(INFO_FILE_URL);

            yield return RequestLoop(
                infoRequest,
                request =>
                {
                    Main.Try("On received server infos", () =>
                    {
                        string seasonJson = request.downloadHandler.text
                            .Split(new[] { SEASON_TAG }, StringSplitOptions.None)[1]
                            .Split('}')[0] + '}';

                        string rallyJson = request.downloadHandler.text
                            .Split(new[] { RALLY_TAG }, StringSplitOptions.None)[1]
                            .Split('}')[0] + '}';

                        serverInfos = JsonUtility.FromJson<ServerInfo>(request.downloadHandler.text);
                        serverInfos.SetSubClasses(seasonJson, rallyJson);

                        byte[] data = Convert.FromBase64String(serverInfos.currentRally.rallyPanel.Split(',')[1]);
                        Texture2D texture = new Texture2D(0, 0);
                        texture.LoadImage(data);

                        serverInfos.currentRally.panel = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            Vector2.one / 2
                        );

                        Main.Log("Received server infos");
                    });
                },
                error =>
                {
                    Main.Error("Couldn't retrieve server infos : " + error);
                    OnFail?.Invoke();
                }
            );
        }

        public static IEnumerator FetchUserCar()
        {
            Main.Log("Sending user car request...");

            UnityWebRequest getRequest = UnityWebRequest.Get(serverInfos.uploadURL + "/" + CAR_ENDPOINT);
            getRequest.SetRequestHeader(USER_ID_HEADER, $"{Platform.Get().GetPlatformType()}:{Platform.Get().GetUserName()}");

            yield return RequestLoop(
                getRequest,
                request =>
                {
                    Main.Try("Receive user cars", () =>
                    {
                        string result = request.downloadHandler.text;
                        seasonCar = int.Parse(result
                            .Split(new[] { SEASON_CAR_TAG }, StringSplitOptions.None)[1].Split(',')[0]);
                        openClassCar = int.Parse(result
                            .Split(new[] { OPEN_CAR_TAG }, StringSplitOptions.None)[1].Split('}')[0]);

                        MRL_Panel.LoadCarSprites();
                        Main.Log("Received user cars");
                    });
                },
                error => Main.Error("Couldn't get user car from discord bot : " + error)
            );
        }

        public static IEnumerator FetchUserResults()
        {
            Main.Log("Sending user results request...");

            UnityWebRequest getRequest = UnityWebRequest.Get(serverInfos.uploadURL + "/" + TIMES_ENDPOINT);
            getRequest.SetRequestHeader(USER_ID_HEADER, $"{Platform.Get().GetPlatformType()}:{Platform.Get().GetUserName()}");

            yield return RequestLoop(
                getRequest,
                request =>
                {
                    Main.Try("Receive user times", () =>
                    {
                        seasonResults = ParseAndSanitizeResults(request.downloadHandler.text, SEASON_RESULTS_TAG);
                        openClassResults = ParseAndSanitizeResults(request.downloadHandler.text, OPEN_RESULTS_TAG);

                        Main.Log("Received user results");
                    });
                },
                error => Main.Error("Couldn't get user results from discord bot : " + error)
            );

            string[] ParseAndSanitizeResults(string text, string startTag)
            {
                return text.Split(new[] { startTag }, StringSplitOptions.None)[1]
                    .Split(']')[0]
                    .Replace(" ", "")
                    .Replace("\"", "")
                    .Split(',');
            }
        }

        public static void StartRecording()
        {
            IsRecording = true;
            inTraining = Main.settings.trainingMode;
        }

        public static void MarkTraining()
        {
            if (!inTraining)
                Main.Log("Detected in training");

            inTraining = true;
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            if (inTraining)
            {
                Main.Log("Training mode was detected at some point in the rally");
                return;
            }

            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                RESULTS_FILE_NAME
            );

            RallyResults results = new RallyResults(playerResults);
            File.WriteAllText(filePath, EncryptResults(results));
            Main.Log("Saved rally results to " + filePath);

            if (Main.settings.openFolderOnResults)
            {
                Process.Start(
                    "explorer.exe",
                    "\"" + Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\""
                );
            }

            if (Main.settings.sendResultsToMod)
                CoroutineRunner.StartCoroutine(SendResults(results));

            IsRecording = false;
        }

        private static IEnumerator RequestLoop(UnityWebRequest request, Action<UnityWebRequest> OnSuccess, Action<string> OnFail)
        {
            int tries = 0;

            while (tries < REQUEST_TRIES)
            {
                AsyncOperation op = request.SendWebRequest();
                yield return new WaitUntil(() => op.isDone || request.isNetworkError || request.isHttpError);

                if (request.isHttpError || request.isNetworkError)
                    yield return new WaitForSeconds((float)Math.Pow(REQUEST_DELAY, tries));
                else
                {
                    OnSuccess?.Invoke(request);
                    yield break;
                }

                tries++;
            }

            OnFail?.Invoke(request.error + (request.isHttpError ? " / " + request.downloadHandler.text : ""));
        }

        private static IEnumerator SendResults(RallyResults results)
        {
            Main.Log("Started sending results to discord bot...");
            bool failed = false;
            yield return FetchServerInfos(() => failed = true);

            if (failed)
            {
                Main.Error("Aborted sending results to discord bot");
                yield break;
            }

            byte[] resultsData = Encoding.UTF8.GetBytes(results.ToJson());
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("file", resultsData, RESULTS_FILE_NAME, "application/json"));
            UnityWebRequest postRequest = UnityWebRequest.Post(serverInfos.uploadURL + "/" + RESULTS_ENDPOINT, formData);
            postRequest.SetRequestHeader(USER_ID_HEADER, $"{Platform.Get().GetPlatformType()}:{Platform.Get().GetUserName()}");

            yield return RequestLoop(
                postRequest,
                request =>
                {
                    Main.Log("Results sent to discord bot with header : " + postRequest.GetRequestHeader(USER_ID_HEADER));
                    CoroutineRunner.StartCoroutine(FetchUserResults()); // update results from server
                },
                error => Main.Error("Couldn't send results to discord bot : " + error)
            );
        }

        private static string EncryptResults(RallyResults results)
        {
            string jsonData = results.ToJson();
            string result = string.Empty;

            for (int i = 0; i < jsonData.Length; i++)
                result += ((char)(jsonData[i] ^ ENCRYPTION_KEY));

            return result;
        }
    }
}
