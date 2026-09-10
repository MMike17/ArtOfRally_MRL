using UnityEngine;
using UnityEngine.UI;

namespace MRL
{
    /// <summary>Screen that displays infos about MRL events</summary>
    public class MRL_Panel : Panel
    {
        // TODO : What do we need here ?

        private Text titleText;
        private Text stagesText;
        private Text deadlineText;
        private Text descriptionText;

        public void Setup()
        {
            titleText = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            stagesText = transform.GetChild(1).GetComponent<Text>();
            deadlineText = transform.GetChild(3).GetComponent<Text>();
            descriptionText = transform.GetChild(4).GetComponent<Text>();

            // TODO : Also setup Panel stuff here
        }

        public void ShowInfos(bool isSeason)
        {
            // TODO : Finish this

            //CustomEventManager.serverInfos
            //titleText.text = ;
        }
    }
}
