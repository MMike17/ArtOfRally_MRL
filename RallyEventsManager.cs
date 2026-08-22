using UnityEngine;
using UnityEngine.Networking;

namespace MRL
{
    class RallyEventsManager
    {
        const string INFO_FILE_URL = "https://gist.github.com/MMike17/7b9ed3de87db0969d05877ac14f50fd5/raw/cea4ab6f61a7f3580f06ce8bbdada8bdf7f1b643/RallyEvent.txt";

        // TODO : Call this in Main
        public static void Init()
        {
            UnityWebRequest request = UnityWebRequest.Get(INFO_FILE_URL);
            AsyncOperation op = request.SendWebRequest();
            op.completed += asyncOp =>
            {
                if (request.isHttpError || request.isNetworkError)
                    Main.Error(request.error);
                else
                    Main.Log("Result : " + request.downloadHandler.text);
            };
        }
    }
}
