using HarmonyLib;

namespace ModBase
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
	
	//	this will negate the method
	//  	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	//  	{
	//      	foreach (var instruction in instructions)
	//          	yield return new CodeInstruction(OpCodes.Ret);
	//  	}

	// 	static void Postfix()
	// 	{
	// 		//
	// 	}
	// }
}
