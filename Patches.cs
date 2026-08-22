using HarmonyLib;

namespace MRL
{
    // Patch model
    // [HarmonyPatch(typeof(), nameof())]
    // [HarmonyPatch(typeof(), MethodType.)]
    // static class type_method_Patch
    // {
    // 	static void Prefix()
    // 	{
    // 		//
    // 	}

    // //	this will negate the method
    // //  	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    // //  	{
    // //      	foreach (var instruction in instructions)
    // //          	yield return new CodeInstruction(OpCodes.Ret);
    // //  	}

    // 	static void Postfix()
    // 	{
    // 		//
    // 	}
    // }

    [HarmonyPatch(typeof(RallyData), nameof(RallyData.SetRallyComplete))]
    static class CompletionDetector
    {
        static void Prefix()
        {
            if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM && ResultsManager.IsRecording)
            {
                Driver player = GameModeManager.GetSeasonDataCurrentGameMode().DriverList.Find(item => item.isPlayer);
                ResultsManager.WriteResults(player.GetResultsForCurrentRally());
            }
        }
    }
}
