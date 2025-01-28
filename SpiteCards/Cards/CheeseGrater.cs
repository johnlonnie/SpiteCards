using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModdingUtils.Extensions;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;
using System.Collections.ObjectModel;
using UnboundLib.Utils;
using SpiteCards.Extensions;
using SpiteCards.MonoBehaviours;
using System.Reflection;
using System.ComponentModel;


namespace SpiteCards.Cards
{
    class CheeseGrater : CustomCard
    {
        private static float rangePerCard = 4f; //set the damage radius
        private static float durationPerCard = 2f;

        private static void makeHierarchyHidden(GameObject obj)
        {
            obj.gameObject.hideFlags = HideFlags.HideAndDontSave;
            foreach (Transform child in obj.transform)
            {
                makeHierarchyHidden(child.gameObject);
            }
        }
        private class CheeseGrateSpawner : MonoBehaviour
        {
            void Start()
            {
                if (!(this.gameObject.GetComponent<SpawnedAttack>().spawner != null))
                {
                    return;
                }

                this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f) * (this.gameObject.GetComponent<SpawnedAttack>().spawner.GetComponent<Block>().GetAdditionalData().cheeseGraterRange / CheeseGrater.rangePerCard);

                this.gameObject.AddComponent<RemoveAfterSeconds>().seconds = 5f;
                this.gameObject.transform.GetChild(1).GetComponent<LineEffect>().SetFieldValue("inited", false);
                typeof(LineEffect).InvokeMember("Init",
                    BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                    null, this.gameObject.transform.GetChild(1).GetComponent<LineEffect>(), new object[] { });
                this.gameObject.transform.GetChild(1).GetComponent<LineEffect>().radius = (CheeseGrater.rangePerCard - 1.4f);
                this.gameObject.transform.GetChild(1).GetComponent<LineEffect>().SetFieldValue("startWidth", 0.5f);
                this.gameObject.transform.GetChild(1).GetComponent<LineEffect>().Play();

            }
        }

        private static GameObject cheeseGrateVisual_ = null;
        private static GameObject cheeseGrateVisual
        {
            get
            {
                if (cheeseGrateVisual_ != null) { return cheeseGrateVisual_; }
                else
                {
                    List<CardInfo> activecards = ((ObservableCollection<CardInfo>)typeof(CardManager).GetField("activeCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).ToList();
                    List<CardInfo> inactivecards = (List<CardInfo>)typeof(CardManager).GetField("inactiveCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    List<CardInfo> allcards = activecards.Concat(inactivecards).ToList();
                    GameObject E_Overpower = allcards.Where(card => card.cardName.ToLower() == "overpower").First().GetComponent<CharacterStatModifiers>().AddObjectToPlayer.GetComponent<SpawnObjects>().objectToSpawn[0];
                    cheeseGrateVisual_ = UnityEngine.GameObject.Instantiate(E_Overpower, new Vector3(0, 100000f, 0f), Quaternion.identity);
                    cheeseGrateVisual_.name = "E_CheeseGrate";
                    DontDestroyOnLoad(cheeseGrateVisual_);
                    foreach (ParticleSystem parts in cheeseGrateVisual_.GetComponentsInChildren<ParticleSystem>())
                    {
                        parts.startColor = Color.magenta;
                    }
                    cheeseGrateVisual_.transform.GetChild(1).GetComponent<LineEffect>().colorOverTime.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.green, 0f) };
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.transform.GetChild(2).gameObject);
                    cheeseGrateVisual_.transform.GetChild(1).GetComponent<LineEffect>().offsetMultiplier = 0f;
                    cheeseGrateVisual_.transform.GetChild(1).GetComponent<LineEffect>().playOnAwake = true;
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<FollowPlayer>());
                    cheeseGrateVisual_.GetComponent<DelayEvent>().time = 0f;
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<SoundImplementation.SoundUnityEventPlayer>());
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<Explosion>());
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<Explosion_Overpower>());
                    //UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<SpawnedAttack>());
                    UnityEngine.GameObject.Destroy(cheeseGrateVisual_.GetComponent<RemoveAfterSeconds>());
                    cheeseGrateVisual_.AddComponent<CheeseGrateSpawner>();
                    return cheeseGrateVisual_;
                }
            }
            set { }
        }

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            //Edits values on card itself, which are then applied to the player in `ApplyCardStats`
            statModifiers.sizeMultiplier = 2.5f;
            statModifiers.gravity = 2f;
            statModifiers.health = 3f;
            statModifiers.secondsToTakeDamageOver = 2f;
            statModifiers.movementSpeed = 0.5f;
            gun.projectileSpeed = 1;
            block.cdMultiplier = 0.5f;


        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //Edits values on player when card is selected
            gunAmmo.maxAmmo = 1;
            if (block.GetAdditionalData().cheeseGraterRange == 0f)
            {
                block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(block.BlockAction, new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(player, block)));
                block.objectsToSpawn.Add(cheeseGrateVisual);
            }

            block.GetAdditionalData().cheeseGraterRange += CheeseGrater.rangePerCard;
            block.GetAdditionalData().cheeseGraterDuration += CheeseGrater.durationPerCard;

        }

        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                if (trigger != BlockTrigger.BlockTriggerType.None)
                {
                    Vector2 pos = block.transform.position;
                    Player[] players = PlayerManager.instance.players.ToArray();

                    for (int i = 0; i < players.Length; i++)
                    {
                        // don't apply the effect to the player who activated it...
                        if (players[i].playerID == player.playerID) { continue; }

                        // apply to players within range, that are within line-of-sight
                        if (Vector2.Distance(pos, players[i].transform.position) < block.GetAdditionalData().cheeseGraterRange && PlayerManager.instance.CanSeePlayer(player.transform.position, players[i]).canSee)
                        {
                            players[i].data.healthHandler.TakeDamage(420f * Vector2.down, players[i].transform.position, players[i].data.weaponHandler.gameObject, players[i], true, true);
                        }
                    }


                }
            };
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //Run when the card is removed from the player
        }

        protected override string GetTitle()
        {
            return "Cheese Grinder";
        }
        protected override string GetDescription()
        {
            return "Big Cat blocks for 420 damage. Sick.";
        }
        protected override GameObject GetCardArt()
        {
            return SpiteCards.CheeseGrinderObj;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = true,
                    stat = "size",
                    amount = "250%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Block Cooldown",
                    amount = "-50%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Health",
                    amount = "3X",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Move Speed",
                    amount = "50%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Ammo",
                    amount = "Reset to 1",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.ColdBlue;
        }
        public override string GetModName()
        {
            return "ModName";
        }
    }
}

