﻿using HarmonyLib;
using LLVMSharp.Interop;

namespace AssetRipper.Translation.Cpp;

[HarmonyPatch]
internal static class Patches
{
	public static void Apply() => Harmony.CreateAndPatchAll(typeof(Patches));

	[HarmonyPrefix]
	[HarmonyPatch(typeof(LLVMValueRef), $"get_{nameof(LLVMValueRef.SuccessorsCount)}")]
	public static bool GetSuccessorsCountPatch(ref uint __result)
	{
		__result = 0;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(LLVMValueRef), $"get_{nameof(LLVMValueRef.InstructionClone)}")]
	public static bool GetInstructionClonePatch(ref LLVMValueRef __result)
	{
		__result = default;
		return false;
	}
}
