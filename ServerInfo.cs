using System;
using UnityEngine;
using static AreaManager;
using static Car;
using static ConditionTypes;

namespace MRL
{
    /// <summary>Represents the infos we get from the server</summary>
    [Serializable]
    public class ServerInfo
    {
        public SeasonInfo currentSeason;
        public RallyInfo currentRally;

        public ServerInfo(string seasonJson, string rallyJson)
        {
            currentSeason = JsonUtility.FromJson<SeasonInfo>(seasonJson);
            currentRally = JsonUtility.FromJson<RallyInfo>(rallyJson);
        }

        public Season GenerateSeason()
        {
            Season season = new Season(
                currentSeason.year,
                currentSeason.group,
                1,
                currentRally.stageIndeces.Length,
                0,
                string.Empty,
                false,
                AIDriverSkillTables.AI_Skill.MASTER
            );

            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)currentRally.location);
            season.Rallies[0].SetStageCount(currentRally.stageIndeces.Length);

            for (int i = 0; i < currentRally.stageIndeces.Length; i++)
            {
                int index = currentRally.stageIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, currentRally.location);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, currentRally.stageWeathers[i]);
            }

            return season;
        }

        /// <summary>Holds information about a rally season</summary>
        [Serializable]
        public class SeasonInfo
        {
            public int year;
            public CarClass group;
        }

        /// <summary>Holds information about a rally event</summary>
        [Serializable]
        public class RallyInfo
        {
            public CarClass openClassGroup;
            public Areas location;
            public int[] stageIndeces;
            public Weather[] stageWeathers;
        }
    }
}
