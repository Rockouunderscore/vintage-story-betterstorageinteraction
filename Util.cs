
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace betterstorageinteractions;

public static class Util
{

    public static int TryPutInto(this ItemSlot sourceSlot, ItemSlot sinkSlot, int quantity = 0)
    {

        if (sourceSlot.Itemstack == null)
        {
            return 0;
        }

        if (!(quantity > 0) || quantity > sourceSlot.StackSize)
        {
            quantity = sourceSlot.StackSize;
        }

        if (!(quantity > 0))
        {
            return 0;
        }

        if (sinkSlot.Itemstack == null)
        {
            ItemStack sourceStack = sourceSlot.TakeOut(quantity);
            int amountMoved = sourceStack.StackSize;
            sinkSlot.Itemstack = sourceStack;
            
            return amountMoved;
        }
        
        int mergeableQuantity = sourceSlot.Itemstack.Collectible.GetMergableQuantity(sinkSlot.Itemstack, sourceSlot.Itemstack, EnumMergePriority.AutoMerge);
        if (mergeableQuantity > 0)
        {
            int amount = GameMath.Min(mergeableQuantity, quantity);
            ItemStack sourceStack = sourceSlot.TakeOut(amount);
            int amountMoved = sourceStack.StackSize;
            sinkSlot.Itemstack.StackSize += sourceStack.StackSize;
            
            return amountMoved;
        }
        
        return 0;
    }
    
    public static int TryTakeInto(this ItemSlot sinkSlot, ref ItemStack sourceStack, int amount = 0)
    {

        if (sourceStack == null)
        {
            return 0;
        }

        if (!(amount > 0) || amount > sourceStack.StackSize)
        {
            amount = sourceStack.StackSize;
        }

        if (!(amount > 0))
        {
            return 0;
        }
        

        
        int mergeableQuantity = sourceStack.Collectible.GetMergableQuantity(sinkSlot.Itemstack, sourceStack, EnumMergePriority.AutoMerge);
        int quantity = GameMath.Min(mergeableQuantity, amount);
        
        if (quantity > 0)
        {
            if (sinkSlot.Itemstack == null)
            {
                sinkSlot.Itemstack = sourceStack.GetEmptyClone();
            }
            sinkSlot.Itemstack.StackSize += amount;
            sourceStack.StackSize -= amount;

            if (sourceStack.StackSize <= 0)
            {
                sourceStack = null;
            }
            
            return amount;
        }
        
        return 0;
    }
    
    public static IEnumerable<ItemSlot> GetPlayerItemSlots(IPlayer byPlayer)
    {
        
        ItemSlot handSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (handSlot != null)
        {
            yield return handSlot;
        }
        
        IInventory hotbar = byPlayer.InventoryManager.GetOwnInventory("hotbar");
        if (hotbar != null)
        {
            foreach (ItemSlot slot in hotbar)
            {
                // dont return handSlot twice
                if (slot == handSlot)
                {
                    continue;
                }
                yield return slot;
            }
        }
        
        IInventory backpack = byPlayer.InventoryManager.GetOwnInventory("backpack");
        if (backpack != null)
        {
            foreach (ItemSlot slot in backpack)
            {
                yield return slot;
            }
        }
        
    }
    
    public static IEnumerable<ItemSlot> GetPlayerMatchingItemSlots(IPlayer byPlayer, ItemStack templateStack, bool matchEmpty)
    {
        IEnumerable<ItemSlot> slots = GetPlayerItemSlots(byPlayer);

        foreach (ItemSlot slot in slots)
        {
            if (slot.Itemstack == null)
            {
                if (matchEmpty)
                {
                    yield return slot;
                }
                continue;
            }

            if (templateStack == null)
            {
                yield return slot;
                continue;
            }
            
            int mergeableQuantity = templateStack.Collectible.GetMergableQuantity(templateStack, slot.Itemstack, EnumMergePriority.AutoMerge);
            if (mergeableQuantity > 0)
            {
                yield return slot;
                continue;
            }
        }

    }
}