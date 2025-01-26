using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace FasterTorchIgnition;

public class FIModSystem : ModSystem
{
    public static ICoreServerAPI Api;
    private Harmony _harmony;

    public override bool ShouldLoad(EnumAppSide side)
    {
        return side == EnumAppSide.Server;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        Api = api;

        if (!Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            _harmony = new Harmony(Mod.Info.ModID);
            try
            {
                _harmony.PatchAll();
            }
            catch (Exception e)
            {
                api.Logger.Error($"Harmony exception: {e}");
                throw;
            }
        }
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(Mod.Info.ModID);
    }
}