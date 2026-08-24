using System;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace MRL
{
    /// <summary>Holds informations about rally events that are sent through a Gist</summary>
    [Serializable]
    public class EventInfos
    {
        public int year;
        public CarClass carClass;
        public Areas location;
        public int[] stageIndeces;
        public Weather[] stageWeathers;

        public Season GenerateSeason()
        {
            Season season = new Season(
                year,
                carClass,
                1,
                stageIndeces.Length,
                0,
                string.Empty,
                false,
                AIDriverSkillTables.AI_Skill.MASTER
            );

            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)location);
            season.Rallies[0].SetStageCount(stageIndeces.Length);

            for (int i = 0; i < stageIndeces.Length; i++)
            {
                int index = stageIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, location);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, stageWeathers[i]);
            }

            return season;
        }

        // TODO : What else do we need here for the future ?
        // description
        // picture (can I encode this for the Gist and then decode it locally ?)
    }
}
