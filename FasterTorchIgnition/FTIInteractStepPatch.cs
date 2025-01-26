using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FasterTorchIgnition;

[HarmonyPatch(typeof(BlockBehaviorCanIgnite), nameof(BlockBehaviorCanIgnite.OnHeldInteractStep))]
public class FTIInteractStepPatch
{
    static bool Prefix(ref bool __result, float secondsUsed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        if (blockSel == null)
        {
            __result = false;
            return false; //override original
        }

        IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
        if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
        {
            __result = false;
            return false;
        }


        Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);

        EnumIgniteState igniteState = EnumIgniteState.NotIgnitable;

        IIgnitable ign = block.GetInterface<IIgnitable>(byEntity.World, blockSel.Position);
        if (ign != null) igniteState = ign.OnTryIgniteBlock(byEntity, blockSel.Position, secondsUsed);
        if (igniteState == EnumIgniteState.NotIgnitablePreventDefault)
        {
            __result = false;
            return false;
        }

        handling = EnumHandling.PreventDefault;

        if (byEntity.World is IClientWorldAccessor && secondsUsed > 0.25f && (int)(30 * secondsUsed) % 2 == 1)
        {
            Random rand = byEntity.World.Rand;
            Vec3d pos = blockSel.Position.ToVec3d().Add(blockSel.HitPosition).Add(rand.NextDouble()*0.25 - 0.125, rand.NextDouble() * 0.25 - 0.125, rand.NextDouble() * 0.25 - 0.125);

            Block blockFire = byEntity.World.GetBlock(new AssetLocation("fire"));

            AdvancedParticleProperties props = blockFire.ParticleProperties[blockFire.ParticleProperties.Length - 1].Clone();
            props.basePos = pos;
            props.Quantity.avg = 0.5f;

            byEntity.World.SpawnParticles(props, byPlayer);

            props.Quantity.avg = 0;
        }


        // Crappy fix to make igniting not buggy T_T
        if (byEntity.World.Side == EnumAppSide.Server)
        {
            __result = true;
            return false;
        }

        var delay = 3.2;
        if (block is BlockTorch)
        {
            delay = 0.75f;
        }
        __result = secondsUsed <= delay;
        return false;
    }
}