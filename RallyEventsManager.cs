using UnityEngine;
using UnityEngine.Networking;

namespace MRL
{
    class RallyEventsManager
    {
        const string INFO_FILE_URL = "https://gist.githubusercontent.com/MMike17/7b9ed3de87db0969d05877ac14f50fd5/raw/RallyEvent.txt";

        public static void Init()
        {
            UnityWebRequest request = UnityWebRequest.Get(INFO_FILE_URL);
            AsyncOperation op = request.SendWebRequest();
            op.completed += asyncOp =>
            {
                if (request.isHttpError || request.isNetworkError)
                    Main.Error(request.error);
                else
                {
                    RallyEventInfos rallyInfo = JsonUtility.FromJson<RallyEventInfos>(request.downloadHandler.text);
                    Main.Log("Received rally info");
                }
            };
        }
    }
}
