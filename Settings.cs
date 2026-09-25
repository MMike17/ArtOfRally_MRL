using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace MRL
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

        [Draw(DrawType.Auto)]
        public bool trainingMode = false;
        [Draw(DrawType.Auto)]
        public bool openFolderOnResults = true;
        [Draw(DrawType.Auto)]
        public bool sendResultsToMod = false;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;
        //public bool disableInfoLogs = false;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            // SnapValue(, 0.1f);
        }

        internal void OnGUI()
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
