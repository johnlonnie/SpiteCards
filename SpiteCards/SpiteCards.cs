using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using SpiteCards.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;


namespace SpiteCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class SpiteCards : BaseUnityPlugin
    {
        private const string ModId = "com.My.Mod.Id";
        private const string ModName = "SpiteCards";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "SC";
        public static SpiteCards instance { get; private set; }


        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            instance = this;
            CustomCard.BuildCard<SweetBabyT>();
            CustomCard.BuildCard<WillBuild>();
            CustomCard.BuildCard<RunningAwayWithIt>();
            CustomCard.BuildCard<CheeseGrater>();
        }
    }
}
