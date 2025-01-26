using System;
using HarmonyLib;
using Vintagestory.API.Common;

namespace FasterTorchIgnition;

public class FTIModSystem : ModSystem
{
    public static ICoreAPI Api;
    private Harmony _harmony;
    
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Api = api;

        if (!Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            _harmony = new Harmony(Mod.Info.ModID);
            _harmony.PatchAll();
        }
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(Mod.Info.ModID);
    }
}