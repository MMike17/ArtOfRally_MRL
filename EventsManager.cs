using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MRL
{
    class EventsManager
    {
        const string INFO_FILE_URL = "https://gist.githubusercontent.com/MMike17/7b9ed3de87db0969d05877ac14f50fd5/raw/RallyEvent.txt";
        const string RESULTS_FILE_NAME = "RallyResults.mrl";
        const int ENCRYPTION_KEY = 573; // TODO : This will get changed with rolling encryption

        public static bool IsRecording { get; private set; }
        public static EventInfos rallyInfo { get; private set; }

        public static void GetEventInfos()
        {
            UnityWebRequest request = UnityWebRequest.Get(INFO_FILE_URL);
            AsyncOperation op = request.SendWebRequest();
            op.completed += asyncOp =>
            {
                if (request.isHttpError || request.isNetworkError)
                    Main.Error("Couldn't retrieve rally info from server\n" + request.error);
                else
                {
                    rallyInfo = JsonUtility.FromJson<EventInfos>(request.downloadHandler.text);
                    Main.Log("Received rally info");
                }
            };
        }

        public static void StartRecording()
        {
            IsRecording = true;
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                RESULTS_FILE_NAME
            );

            File.WriteAllText(filePath, EncryptResults(new EventResults(playerResults)));
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

        private static string EncryptResults(EventResults results)
        {
            string jsonData = results.ToJson();
            string result = string.Empty;

            for (int i = 0; i < jsonData.Length; i++)
                result += ((char)(jsonData[i] ^ ENCRYPTION_KEY));

            return result;
        }
    }
}
