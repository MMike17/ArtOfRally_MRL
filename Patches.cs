using HarmonyLib;

// TODO : Start detection

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
            if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CUSTOM)
            {
                Main.Log("Detected end of rally !!!"); // This pings when we go to the end screen

                Main.Log("Current time : " +
                TimeFormatter.GetCachedFormattedTimeLong(GameModeManager.GetSeasonDataCurrentGameMode().DriverList.Find(item => item.isPlayer).GetResultsForCurrentRally().GetTotalRallyTime()));
            }
        }

        //this will negate the method

        //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    foreach (var instruction in instructions)
        //        yield return new CodeInstruction(OpCodes.Ret);
        //}

        static void Postfix()
        {
            //
        }
    }
}
