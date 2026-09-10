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
                AIDriverSkillTables.AI_Skill.MASTER,
                Season.STATUS.UNLOCKED
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
            public string name = "Test very long rally name with suffix";
            public string description = "This is a test rally description with a long text to check for font size and new lines. I hope this will work correctly because I want to jump out the window, but given I live on the first floor, I'm most likely going to break my legs or end up paralized instead of dead, not cool. Otherwise I'll have to try to land on my head, which is really hard given I only have 1 story. Anyway, does this work correctly ? Is this the right size and not super fucking huge or way too small ?";
            public string deadline = "12th September 2026";
            public string country = "Antartica";

            public CarClass openClassGroup;
            public Areas location;
            public int[] stageIndeces;
            public Weather[] stageWeathers;
        }
    }
}
