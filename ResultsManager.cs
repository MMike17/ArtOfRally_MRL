namespace MRL
{
    class ResultsManager
    {
        public static bool IsRecording { get; private set; }

        public static void StartRecording()
        {
            IsRecording = true;
        }

        public static void WriteResults(DriverRallyResults playerResults)
        {
            //GetTotalRallyTime()

            IsRecording = false;
            Main.Log("Reset results recording");
        }
    }
}
