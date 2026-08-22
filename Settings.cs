using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace ModBase
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]
        
        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool showMarkers;
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            Main.SetMarkers(showMarkers);

            // SnapValue(, 0.1f);
        }

        internal void OnGUI ()
        {
            // custom GUI here
        }

        private float SnapValue(float value, float snapValue, float range, float snapPercent)
        {
            float snapDiff = range * snapPercent;
            float minTarget = snapValue - snapDiff / 2;
            float maxTarget = snapValue + snapDiff / 2;
            return value <= maxTarget && value >= minTarget ? snapValue : value;
        }
    }
}
