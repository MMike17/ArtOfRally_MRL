using System;
using System.IO;
using UnityEngine;

namespace MRL
{
    class ResultsManager
    {
        const string RESULTS_FILE_NAME = "RallyResults.txt";
        const int ENCRYPTION_KEY = 573; // TODO : This will get changed with rolling encryption

        public static bool IsRecording { get; private set; }

        public static void StartRecording()
        {
            IsRecording = true;
            Main.Log("Starting recording");
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), RESULTS_FILE_NAME);

            File.WriteAllText(filePath, EncryptResults(new EventResults(playerResults)));
            Main.Log("Saved rally results to " + filePath);

            // TODO : Open folder to file path ?
            IsRecording = false;
        }

        private static string EncryptResults(EventResults results)
        {
            string jsonData = JsonUtility.ToJson(results, true);
            string result = string.Empty;

            for (int i = 0; i < jsonData.Length; i++)
                result += ((char)(jsonData[i] ^ ENCRYPTION_KEY));

            return result;
        }
    }
}
