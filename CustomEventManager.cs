using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MRL
{
    class CustomEventManager
    {
        static readonly string SEASON_TAG = "  \"currentSeason\": ";
        static readonly string RALLY_TAG = "  \"currentRally\": ";

        const string INFO_FILE_URL = "https://gist.githubusercontent.com/MMike17/7b9ed3de87db0969d05877ac14f50fd5/raw/RallyEvent.txt";
        const string RESULTS_FILE_NAME = "RallyResults.mrl";
        const int ENCRYPTION_KEY = 573; // TODO : This will get changed with rolling encryption

        public static bool IsRecording { get; private set; }
        public static ServerInfo serverInfos { get; private set; }

        private static bool inTraining;

        public static void GetServerInfos()
        {
            UnityWebRequest request = UnityWebRequest.Get(INFO_FILE_URL);
            AsyncOperation op = request.SendWebRequest();
            op.completed += asyncOp =>
            {
                Main.Try("On received server infos", () =>
                {
                    if (request.isHttpError || request.isNetworkError)
                        Main.Error("Couldn't retrieve rally info from server\n" + request.error);
                    else
                    {
                        string seasonJson = request.downloadHandler.text
                            .Split(new[] { SEASON_TAG }, StringSplitOptions.None)[1]
                            .Split('}')[0] + '}';

                        string rallyJson = request.downloadHandler.text
                            .Split(new[] { RALLY_TAG }, StringSplitOptions.None)[1]
                            .Split('}')[0] + '}';

                        serverInfos = new ServerInfo(seasonJson, rallyJson);

                        byte[] data = Convert.FromBase64String(serverInfos.currentRally.rallyPanel.Split(',')[1]);
                        Texture2D texture = new Texture2D(0, 0);
                        texture.LoadImage(data);

                        serverInfos.currentRally.panel = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            Vector2.one / 2
                        );

                        Main.Log("Received server info");
                    }
                });
            };
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

            File.WriteAllText(filePath, EncryptResults(new RallyResults(playerResults)));
            Main.Log("Saved rally results to " + filePath);

            if (Main.settings.openFolderOnResults)
            {
                Process.Start(
                    "explorer.exe",
                    "\"" + Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\""
                );
            }

            IsRecording = false;
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
