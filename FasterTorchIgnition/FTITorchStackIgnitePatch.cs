using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.GameContent;

namespace FasterTorchIgnition;

[HarmonyPatch]
public static class FTITorchStackIgnitePatch
{
    static MethodBase TargetMethod()
    {
        return typeof(BlockTorch).GetMethod(
            "Vintagestory.GameContent.IIgnitable.OnTryIgniteStack",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
    }
    
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Ldc_R4, 2f))
            .ThrowIfInvalid("Could not match extinguished torch ignition time")
            .SetOperandAndAdvance(0.75f);
        return matcher.Instructions();
    }
}