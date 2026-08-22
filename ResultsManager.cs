using System;
using System.IO;
using UnityEngine;

namespace MRL
{
    class ResultsManager
    {
        const string RESULTS_FILE_NAME = "RallyResults.txt";

        public static bool IsRecording { get; private set; }

        public static void StartRecording()
        {
            IsRecording = true;
            Main.Log("Starting recording");
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), RESULTS_FILE_NAME);

            File.WriteAllText(
                filePath,
                JsonUtility.ToJson(new EventResults(playerResults), true)
            );

            Main.Log("Saved rally results to " + filePath);
            // TODO : Open folder to file path ?

            IsRecording = false;
            Main.Log("Reset results recording");
        }
    }
}
