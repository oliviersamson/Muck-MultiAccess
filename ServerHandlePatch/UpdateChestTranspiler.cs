﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterMultiplayer.ServerHandlePatch
{
    [HarmonyPatch(typeof(ServerHandle), "UpdateChest")]
    class UpdateChestTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Match end of method
            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Call));
            codeMatcher = codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Ret));

            // Load chestId, cellId, itemId and amount (local variables at index 0 to 3) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3));

            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Action<int, int, int, int>>(
                (chestId, cellId, itemId, amount) => {

                    if (OtherInput.Instance.currentChest == null)
                    {
                        return;
                    }

                    if (OtherInput.Instance.currentChest.id == chestId)
                    {

                        InventoryItem inventoryItem = null;

                        if (itemId != -1)
                        {
                            inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();
                            inventoryItem.Copy(ItemManager.Instance.allItems[itemId], amount);
                        }

                        if (OtherInput.Instance.craftingState == OtherInput.CraftingState.Chest)
                        {
                            ((ChestUI)OtherInput.Instance.chest).cells[cellId].currentItem = inventoryItem;
                            ((ChestUI)OtherInput.Instance.chest).cells[cellId].UpdateCell();
                        }
                    }
                }));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
