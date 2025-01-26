using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.GameContent;

namespace FasterTorchIgnition;

[HarmonyPatch(typeof(BlockTorch), nameof(BlockTorch.OnTryIgniteBlock))]
public static class FTITorchTryIgniteBlockPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Ldc_R4, 1f))
            .ThrowIfInvalid("Could not match extinguished torch ignition time in OnTryIgniteBlock")
            .SetOperandAndAdvance(0.75f);
        return matcher.Instructions();
    }
}