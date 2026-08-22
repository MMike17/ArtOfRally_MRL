using System;
using System.Collections.Generic;

namespace MRL
{
    [Serializable]
    class EventResults
    {
        public List<string> results;
        public string final;

        public EventResults(DriverRallyResults source)
        {
            results = new List<string>(source.StageTimes.Count);
            source.StageTimes.ForEach(item => results.Add(TimeFormatter.GetCachedFormattedTimeLong(item)));
            final = TimeFormatter.GetCachedFormattedTimeLong(source.GetTotalRallyTime());
        }
    }
}
