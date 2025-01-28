using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace SpiteCards.Extensions
{
    // ADD FIELDS TO BLOCK
    [Serializable]
    public class BlockAdditionalData
    {
        public float cheeseGraterRange;
        public float cheeseGraterDuration;
        public float timeOfLastSuccessfulBlock;


        public BlockAdditionalData()
        {
            cheeseGraterRange = 0f;
            cheeseGraterDuration = 0f;
            timeOfLastSuccessfulBlock = -100f;
        }
    }
    public static class BlockExtension
    {
        public static readonly ConditionalWeakTable<Block, BlockAdditionalData> data =
            new ConditionalWeakTable<Block, BlockAdditionalData>();

        public static BlockAdditionalData GetAdditionalData(this Block block)
        {
            return data.GetOrCreateValue(block);
        }

        public static void AddData(this Block block, BlockAdditionalData value)
        {
            try
            {
                data.Add(block, value);
            }
            catch (Exception) { }
        }
    }
    // reset additional block fields when ResetStats is called
    [HarmonyPatch(typeof(Block), "ResetStats")]
    class BlockPatchResetStats
    {
        private static void Prefix(Block __instance)
        {

            __instance.GetAdditionalData().cheeseGraterRange = 0f;

        }
    }
}
