using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRL
{
    [Serializable]
    public class RallyResults
    {
        public string carName;
        public string carClass;
        public List<StageResult> results;
        public string final;

        public RallyResults(DriverRallyResults source)
        {
            Season season = GameModeManager.GetSeasonDataCurrentGameMode();
            carName = season.SelectedCar.name;
            carClass = season.SelectedCar.carClass.ToString();
            results = new List<StageResult>();

            for (int i = 0; i < season.Rallies[0].StageCount; i++)
            {
                Stage stage = season.Rallies[0].StageList[i];
                results.Add(new StageResult(
                    stage.Name,
                    stage.Weather.ToString(),
                    TimeFormatter.GetCachedFormattedTimeLong(source.StageTimes[i]))
                );
            }

            final = TimeFormatter.GetCachedFormattedTimeLong(source.GetTotalRallyTime());
        }

        public string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            int insertIndex = json.IndexOf(',') + 2;
            string stageResultsJson = "\"" + nameof(results) + "\": [\n";

            for (int i = 0; i < results.Count; i++)
            {
                stageResultsJson += JsonUtility.ToJson(results[i], true);

                if (i < results.Count - 1)
                    stageResultsJson += ",\n";
            }

            stageResultsJson += "\n],\n";
            json = json.Insert(insertIndex, stageResultsJson);
            return json;
        }

        [Serializable]
        public class StageResult
        {
            public string stageName;
            public string stageWeather;
            public string time;

            public StageResult(string stageName, string stageWeather, string time)
            {
                this.stageName = stageName;
                this.stageWeather = stageWeather;
                this.time = time;
            }
        }
    }
}
