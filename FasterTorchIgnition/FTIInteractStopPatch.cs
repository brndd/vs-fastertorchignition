using System.Runtime.CompilerServices;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FasterTorchIgnition;

[HarmonyPatch(typeof(BlockBehaviorCanIgnite), nameof(BlockBehaviorCanIgnite.OnHeldInteractStop))]
public class FTIInteractStopPatch
{
    static bool Prefix(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
        var delay = 3.0f;
        if (block is BlockTorch)
        {
            delay = 0.75f;
        }
        if (blockSel == null || secondsUsed < delay)
        {
            return false; //skip original
        }

        IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
        if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
        {
            return false;
        }


        EnumHandling handled = EnumHandling.PassThrough;
        IIgnitable ign = block.GetInterface<IIgnitable>(byEntity.World, blockSel.Position);
        ign?.OnTryIgniteBlockOver(byEntity, blockSel.Position, secondsUsed, ref handled);

        if (handled != EnumHandling.PassThrough)
        {
            return false;
        }

        handling = EnumHandling.PreventDefault;

        if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
        {
            return false;
        }

        if (blockSel != null && byEntity.World.Side == EnumAppSide.Server)
        {
            BlockPos bpos = blockSel.Position.AddCopy(blockSel.Face);
            block = byEntity.World.BlockAccessor.GetBlock(bpos);

            if (block.BlockId == 0)
            {
                byEntity.World.BlockAccessor.SetBlock(byEntity.World.GetBlock(new AssetLocation("fire")).BlockId, bpos);
                BlockEntity befire = byEntity.World.BlockAccessor.GetBlockEntity(bpos);
                befire?.GetBehavior<BEBehaviorBurning>()?.OnFirePlaced(blockSel.Face, (byEntity as EntityPlayer).PlayerUID);
            }
        }

        return false;
    }
}