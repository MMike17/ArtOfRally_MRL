using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace MRL
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

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

            if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM)
            {
                if (!EventsManager.IsRecording)
                {
                    if (GUILayout.Button("Start run recording", GUILayout.Width(400)))
                        EventsManager.StartRecording();
                }
                else
                    GUILayout.Label("Already recording run");
            }
            else
                GUILayout.Label("Can't use this mod outside of the \"Custom Rally\" game mode");
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
