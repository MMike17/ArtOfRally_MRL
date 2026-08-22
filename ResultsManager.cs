using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MRL
{
    class ResultsManager
    {
        const string RESULTS_FILE_NAME = "RallyResults.mrl";
        const int ENCRYPTION_KEY = 573; // TODO : This will get changed with rolling encryption

        public static bool IsRecording { get; private set; }

        public static void StartRecording()
        {
            IsRecording = true;
            Main.Log("Starting recording");
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                RESULTS_FILE_NAME
            );

            File.WriteAllText(filePath, EncryptResults(new EventResults(playerResults)));
            Main.Log("Saved rally results to " + filePath);

            // TODO : Open folder to file path ?
            Process.Start("explorer.exe", "\"" + Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\"");
            IsRecording = false;
        }

        private static string EncryptResults(EventResults results)
        {
            string jsonData = JsonUtility.ToJson(results, true);
            string result = string.Empty;

            for (int i = 0; i < jsonData.Length; i++)
                result += ((char)(jsonData[i] ^ ENCRYPTION_KEY));

            return result + "\nSend this file to https://discord.gg/U9bdFC7v5m in the #event-results channel";
        }
    }
}
