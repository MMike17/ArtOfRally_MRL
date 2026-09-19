using System.Collections;
using UnityEngine;

namespace MRL
{
    /// <summary>Runs coroutines, that's it</summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        public static new void StartCoroutine(IEnumerator Coroutine)
        {
            if (instance == null)
            {
                instance = new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(instance.gameObject);
            }

            (instance as MonoBehaviour).StartCoroutine(Coroutine);
        }
    }
}
