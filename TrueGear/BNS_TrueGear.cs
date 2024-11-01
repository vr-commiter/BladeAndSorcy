using UnityEngine;
using ThunderRoad;
using HarmonyLib;
using System;
using MyTrueGear;
using System.Threading.Tasks;
using System.Threading;
using ThunderRoad.Skill.SpellMerge;
using System.Linq;
using System.Diagnostics;
using ThunderRoad.Skill.SpellPower;
using UnityEngine.Events;

namespace TrueGear
{
    public class BNS_TrueGear : ThunderScript
    {
        private static float lastIntensity;

        private static bool isTwoHandsSpell = false;
        private static bool rightIsSpell = false;
        private static bool leftIsSpell = false;
		private static bool isHeartBeat = false;
		private static bool isFastHeartBeat = false;

		private static string leftSpellName = "";
		private static string rightSpellName = "";

		private readonly int sleepDurationSpellCast = 100;
		private readonly int sleepDurationBowString = 100;
		private readonly int sleepDurationClimb = 100;

		private static bool canDefaultMelee = true;
		private static bool canSpell = true;
		private static int sleepDefaultMelee = 100;
		private static int sleepSpell = 100;

        private static bool isLeftClimbGripping;
        private static bool isRightClimbGripping;

        private static TrueGearMod _TrueGear = null;


        Stopwatch torsoCollisionstopwatch = null;


        public override void ScriptLoaded(ModManager.ModData modData)
        {
            Harmony.CreateAndPatchAll(typeof(BNS_TrueGear));
            _TrueGear = new TrueGearMod();
            _TrueGear.Start();
			CreateEventHooks();
			new Thread(new ThreadStart(BNS_TrueGear.SpellTimer)).Start();
			new Thread(new ThreadStart(BNS_TrueGear.DefaultMeleeTimer)).Start();
            torsoCollisionstopwatch = new Stopwatch();
            base.ScriptLoaded(modData);
        }

        private static void TGOnPlayerEnterArea()
		{
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("EnterWater");
            _TrueGear.Play("EnterWater");
        }
        private static void TGOnPlayerExitArea()
        {
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("ExitWater");
            _TrueGear.Play("ExitWater");
        }



        [HarmonyPostfix, HarmonyPatch(typeof(SpellCastCharge), "OnCrystalSlam")]
        private static void SpellCastCharge_OnCrystalSlam_Postfix(SpellCastCharge __instance)
        {
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("CrystalSlam");
            _TrueGear.Play("CrystalSlam");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RagdollHand), "Grab",new Type[] { typeof(Handle), typeof(HandlePose), typeof(float), typeof(bool), typeof(bool) })]
        private static void RagdollHand_Grab_Postfix(RagdollHand __instance)
        {
			if (!__instance.creature.isPlayer)
			{
				return;
			}
            if (__instance.side == Side.Left)
            {
                UnityEngine.Debug.Log("LeftGrab");
                _TrueGear.Play("LeftGrab");
            }
            if (__instance.side == Side.Right)
            {
                UnityEngine.Debug.Log("RightGrab");
                _TrueGear.Play("RightGrab");
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(RagdollHandClimb), "TryGrip")]
        private static void RagdollHandClimb_TryGrip_Prefix(RagdollHandClimb __instance)
        {
            if (!__instance.ragdollHand.creature.isPlayer)
            {
                return;
            }
            if (__instance.ragdollHand.side == Side.Left)
			{
				isLeftClimbGripping = __instance.isGripping;
			}
			else
			{
                isRightClimbGripping = __instance.isGripping;
            }			
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RagdollHandClimb), "TryGrip")]
        private static void RagdollHandClimb_TryGrip_Postfix(RagdollHandClimb __instance)
        {
            if (!__instance.ragdollHand.creature.isPlayer)
            {
                return;
            }
            if (!isLeftClimbGripping && __instance.ragdollHand.side == Side.Left && __instance.isGripping)
			{
                UnityEngine.Debug.Log("LeftClimb");
                _TrueGear.Play("LeftClimb");
            }
            if (!isRightClimbGripping && __instance.ragdollHand.side == Side.Right && __instance.isGripping)
            {
                UnityEngine.Debug.Log("RightClimb");
                _TrueGear.Play("RightClimb");
            }
        }


        [HarmonyPostfix, HarmonyPatch(typeof(InventoryChestHolder), "AddItemToInventory")]
		private static void InventoryChestHolder_AddItemToInventory_Postfix(InventoryChestHolder __instance)
		{
            if (!__instance.IsPlayerCreature)
            {
                return;
            }
            UnityEngine.Debug.Log("--------------------------------");
			UnityEngine.Debug.Log("AddItemToInventory");
            _TrueGear.Play("UnGrabChestInventry");
        }

		[HarmonyPostfix, HarmonyPatch(typeof(InventoryChestHolder), "GrabTryTouchAction")]
        private static void InventoryChestHolder_GrabTryTouchAction_Postfix(InventoryChestHolder __instance,bool __result)
        {
            if (!__instance.IsPlayerCreature)
            {
                return;
            }
            if (__result)
			{
                UnityEngine.Debug.Log("--------------------------------");
                UnityEngine.Debug.Log("GrabChestInventry");
				_TrueGear.Play("GrabChestInventry");
                UnityEngine.Debug.Log(__result);
            }
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(InventoryChestHolder), "OnUnGrab")]
        //private static void InventoryChestHolder_OnUnGrab_Postfix()
        //{
        //    UnityEngine.Debug.Log("--------------------------------");
        //    UnityEngine.Debug.Log("InventoryChestHolderOnUnGrab");
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryChestHolder), "UnGrabTryTouchAction")]
        private static void InventoryChestHolder_UnGrabTryTouchAction_Postfix(InventoryChestHolder __instance)
        {
            if (!__instance.IsPlayerCreature)
            {
                return;
            }
            UnityEngine.Debug.Log("--------------------------------");
            UnityEngine.Debug.Log("UnGrabChestInventry");
            _TrueGear.Play("UnGrabChestInventry");
        }

        public override void ScriptUpdate()
        {
			if (Player.local == null || Player.local.creature == null || !Player.local.creature.initialized)
			{
				return;
			}
			Creature creature = Player.local.creature;
			if (!(creature != null))
			{
				return;
			}
			if (creature.currentHealth < creature.maxHealth * 0.25f && !isFastHeartBeat)
			{
				_TrueGear.StopHeartBeat();
				_TrueGear.StartFastHeartBeat();
				isFastHeartBeat = true;
				isHeartBeat = true;
			}
			else if (creature.currentHealth < creature.maxHealth * 0.35f && !isHeartBeat)
			{
				isHeartBeat = true;
				isFastHeartBeat = false;
				_TrueGear.StartHeartBeat();
				_TrueGear.StopFastHeartBeat();
			}
			else if(creature.currentHealth >= creature.maxHealth * 0.35f && (isHeartBeat || isFastHeartBeat))
			{
				isHeartBeat = false;
				isFastHeartBeat = false;
				_TrueGear.StopHeartBeat();
				_TrueGear.StopFastHeartBeat();
			}
			if (creature.mana != null)
			{
				if (creature.mana.casterLeft != null && creature.mana.casterLeft.telekinesis != null)
				{
					if (creature.mana.casterLeft.telekinesis.catchedHandle != null)
					{
						if (!telekinesisActiveLeft)
						{
							telekinesisActiveLeft = true;
							PerformTelekenesisActivationFeedback(leftHand: true);
						}
					}
					else if (telekinesisActiveLeft)
					{
						telekinesisActiveLeft = false;
					}
					if (creature.mana.casterLeft.telekinesis.pullSpeed > 0f)
					{
						if (!telekinesisPullLeft)
						{
							telekinesisPullLeft = true;
							PerformTelekenesisFeedback(pull: true, leftHand: true);
						}
					}
					else if (telekinesisPullLeft)
					{
						telekinesisPullLeft = false;
					}
					if (creature.mana.casterLeft.telekinesis.repelSpeed > 0f)
					{
						if (!telekinesisRepelLeft)
						{
							telekinesisRepelLeft = true;
							PerformTelekenesisFeedback(pull: false, leftHand: true);
						}
					}
					else if (telekinesisRepelLeft)
					{
						telekinesisRepelLeft = false;
					}
					if (creature.mana.casterLeft.telekinesis.justCatched)
					{
						if (!telekinesisCatchLeftLast)
						{
							telekinesisCatchLeftLast = true;
							_TrueGear.Play("LeftTelekinesisCatch");
						}
					}
					else
					{
						telekinesisCatchLeftLast = false;
					}
				}
				if (creature.mana.casterRight != null && creature.mana.casterRight.telekinesis != null)
				{
					if (creature.mana.casterRight.telekinesis.catchedHandle != null)
					{
						if (!telekinesisActiveRight)
						{
							telekinesisActiveRight = true;
							PerformTelekenesisActivationFeedback(leftHand: false);
						}
					}
					else if (telekinesisActiveRight)
					{
						telekinesisActiveRight = false;
					}
					if (creature.mana.casterRight.telekinesis.pullSpeed > 0f)
					{
						if (!telekinesisPullRight)
						{
							telekinesisPullRight = true;
							PerformTelekenesisFeedback(pull: true, leftHand: false);
						}
					}
					else if (telekinesisPullRight)
					{
						telekinesisPullRight = false;
					}
					if (creature.mana.casterRight.telekinesis.repelSpeed > 0f)
					{
						if (!telekinesisRepelRight)
						{
							telekinesisRepelRight = true;
							PerformTelekenesisFeedback(pull: false, leftHand: false);
						}
					}
					else if (telekinesisRepelRight)
					{
						telekinesisRepelRight = false;
					}
					if (creature.mana.casterRight.telekinesis.justCatched)
					{
						if (!telekinesisCatchRightLast)
						{
							telekinesisCatchRightLast = true;
							_TrueGear.Play("RightTelekinesisCatch");
						}
					}
					else
					{
						telekinesisCatchRightLast = false;
					}
				}
			}
			climbingCheckTimeLeft -= Time.deltaTime;
			if (climbingCheckTimeLeft > 0f)
			{
				return;
			}
			climbingCheckTimeLeft = 300f;
			Item heldItem = creature.equipment.GetHeldItem(Side.Left);
			Item heldItem2 = creature.equipment.GetHeldItem(Side.Right);
			if (Player.local.handLeft.ragdollHand.climb.isGripping || (creature.ragdoll.ik != null && creature.ragdoll.ik.handLeftEnabled && creature.ragdoll.ik.handLeftTarget != null && heldItem == null && creature.equipment.GetHeldHandle(Side.Left) != null && !creature.equipment.GetHeldHandle(Side.Left).customRigidBody.isKinematic && Math.Abs(creature.ragdoll.ik.GetHandPositionWeight(Side.Left) - 1f) < TOLERANCE) || grabbedLadderWithLeftHand)
			{
				if (!leftHandClimbing)
				{
					leftHandClimbing = true;
					PerformClimbingFeedback(Side.Left);
				}
			}
			else
			{
				leftHandClimbing = false;
			}
			if (Player.local.handRight.ragdollHand.climb.isGripping || (creature.ragdoll.ik != null && creature.ragdoll.ik.handRightEnabled && creature.ragdoll.ik.handRightTarget != null && heldItem2 == null && creature.equipment.GetHeldHandle(Side.Right) != null && !creature.equipment.GetHeldHandle(Side.Right).customRigidBody.isKinematic && Math.Abs(creature.ragdoll.ik.GetHandPositionWeight(Side.Right) - 1f) < TOLERANCE) || grabbedLadderWithRightHand)
			{
				if (!rightHandClimbing)
				{
					rightHandClimbing = true;
					PerformClimbingFeedback(Side.Right);
				}
			}
			else
			{
				rightHandClimbing = false;
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SpellCaster), "SetFingersEffect")]
		public static void SpellCaster_SetFingersEffect_PostPatch(SpellCaster __instance, float intensity)
		{
            if (!__instance.ragdollHand.creature.isPlayer)
            {
                return;
            }
            if (isTwoHandsSpell)
			{
				if (leftSpellName == "SpellLightning")
				{
                    int[,] ele = new int[,] { { 0, 1, 4, 5, 8, 9, 12, 13, 16, 17 }, { 2, 3, 6, 7, 110, 11, 14, 15, 18, 19 }, { 100, 101, 104, 105, 108, 109, 112, 113, 116, 117 }, { 102, 103, 106, 107, 110, 111, 114, 115, 118, 119 } };
                    int[] random = new int[] { 3, 3, 3, 3 };
                    _TrueGear.PlayRandom("TwoHandSpellLightning", ele, random);
					return;
                }
				_TrueGear.Play($"TwoHand{leftSpellName}");
				return;
			}
            if (__instance.side == Side.Right)
            {
                rightIsSpell = true;                
            }
            else if (__instance.side == Side.Left)
            {
                leftIsSpell = true;                
            }            
			if (__instance.side != Side.Right && __instance.side != Side.Left)
			{
				rightIsSpell = true;
				leftIsSpell = true;				
			}
            if (lastIntensity == intensity && intensity < 1f && !isTwoHandsSpell && rightIsSpell && leftIsSpell && leftSpellName == rightSpellName)
            {
                isTwoHandsSpell = true;                
            }
            lastIntensity = intensity;
            if (!canSpell)
            {
                return;
            }
            canSpell = false;
            if (isTwoHandsSpell)
			{
				_TrueGear.Play($"TwoHand{leftSpellName}");
			}
			else if (leftIsSpell && rightIsSpell)
			{
				_TrueGear.Play($"LeftAndRight{leftSpellName}");
			}
			else if (leftIsSpell)
			{
				_TrueGear.Play($"Left{leftSpellName}");
			}
			else if (rightIsSpell)
			{
                _TrueGear.Play($"Right{rightSpellName}");
            }
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SpellCaster), "StopFingersEffect")]
		public static void SpellCaster_StopFingersEffect_PostPatch(SpellCaster __instance)
		{
            if (!__instance.ragdollHand.creature.isPlayer)
            {
                return;
            }
            isTwoHandsSpell = false;
			rightIsSpell = false;
			leftIsSpell = false;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SpellMergeData), "Throw")]
		public static void SpellMergeData_Throw_PostPatch(SpellMergeData __instance)
		{
			_TrueGear.Play("SpellThrowingFire");
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SpellMergeLightning), "Update")]
		public static void SpellMergeLightning_Update_PostPatch(SpellMergeLightning __instance)
		{
            if (__instance.beamActive)
			{				
				_TrueGear.Play("SpellThrowingLightning");
			}
		}
		/*
		[HarmonyPostfix, HarmonyPatch(typeof(PlayerControl.Hand), "HapticShort")]
		public static void PlayerControl_HapticShort_PostPatch(PlayerControl.Hand __instance)
		{
			if (!GameManager.options.rumble)
			{
				return;
			}
			if (!canDefaultMelee)
			{
				return;
			}
			canDefaultMelee = false;
			_TrueGear.Play(__instance.side == Side.Left ? "LeftMeleeDefault" : "RightMeleeDefault");
		}
		*/
		[HarmonyPostfix, HarmonyPatch(typeof(PlayerControl.Hand), "HapticPlayClip")]
		public static void PlayerControl_HapticPlayClip_PostPatch(PlayerControl.Hand __instance, GameData.HapticClip hapticClip)
		{
			if (!GameManager.options.rumble)
			{
				return;
			}
			if (hapticClip != Catalog.gameData.haptics.bowShoot)
			{
				return;
			}
			if (!canDefaultMelee)
			{
				return;
			}
			canDefaultMelee = false;
			_TrueGear.Play(__instance.side == Side.Left ? "LeftBowShoot" : "RightBowShoot");
		}

		private static void SpellTimer()
		{
			while (true)
			{
				canSpell = true;
				Thread.Sleep(sleepSpell);
			}
		}

		private static void DefaultMeleeTimer()
		{
			while (true)
			{
				canDefaultMelee = true;
				Thread.Sleep(sleepDefaultMelee);
			}
		}



		// Token: 0x060005DD RID: 1501 RVA: 0x0002744A File Offset: 0x0002564A
		private void CreateEventHooks()
		{
			EventManager.onCreatureSpawn += this.HandlePlayerSpawn;
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x000274D8 File Offset: 0x000256D8
		private void HandlePlayerSpawn(Creature creature)
		{
			if (!creature.isPlayer)
			{
				return;
			}
			EventManager.onEdibleConsumed += this.OnPlayerConsume;
			EventManager.OnSpellUsed += this.OnPlayerUseSpell;
			creature.equipment.OnHolsterInteractedEvent += this.OnPlayerHolsterChanged;
            Player.local.locomotion.OnGroundEvent += this.OnPlayerLanded;
			Player.local.creature.OnDamageEvent += this.OnPlayerDamaged;
			Player.local.creature.OnKillEvent += this.OnPlayerDied;
            Player.local.creature.onEyesEnterUnderwater += TGOnPlayerEnterArea;
            Player.local.creature.onEyesExitUnderwater += TGOnPlayerExitArea;
            Equipment equipment = Player.local.creature.equipment;
			equipment.OnArmourEquippedEvent = (Equipment.OnArmourEquipped)Delegate.Combine(equipment.OnArmourEquippedEvent, new Equipment.OnArmourEquipped(this.OnArmourEquipped));
			Equipment equipment2 = Player.local.creature.equipment;
			equipment2.OnArmourUnEquippedEvent = (Equipment.OnArmourUnEquipped)Delegate.Combine(equipment2.OnArmourUnEquippedEvent, new Equipment.OnArmourUnEquipped(this.OnArmourUnEquipped));
			Player.local.handLeft.ragdollHand.OnGrabEvent += this.OnPlayerGrab;
			Player.local.handLeft.ragdollHand.OnUnGrabEvent += this.OnPlayerUnGrab;
			Player.local.handRight.ragdollHand.OnGrabEvent += this.OnPlayerGrab;
			Player.local.handRight.ragdollHand.OnUnGrabEvent += this.OnPlayerUnGrab;
			Player.local.handLeft.ragdollHand.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartLeft;
			Player.local.handLeft.ragdollHand.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
			Player.local.handRight.ragdollHand.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartRight;
			Player.local.handRight.ragdollHand.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
			foreach (RagdollPart ragdollPart in Player.local.creature.ragdoll.parts)
			{
				UnityEngine.Debug.Log("-----------------------------------");
				UnityEngine.Debug.Log("Collision");
				UnityEngine.Debug.Log(ragdollPart.collisionHandler.name);
				if (ragdollPart.type == RagdollPart.Type.Torso)
				{
					ragdollPart.collisionHandler.OnCollisionStartEvent += this.OnTorsoCollisionStart;
				}
                else if (ragdollPart.type == RagdollPart.Type.Head)
                {
                    ragdollPart.collisionHandler.OnCollisionStartEvent += this.OnTorsoCollisionStart;
                }
                else if (ragdollPart.collisionHandler.name.Contains("LeftForeArm"))
				{
					ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartLeft;
					ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
				}
				else if (ragdollPart.collisionHandler.name.Contains("LeftArm"))
				{
					ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartLeft;
					ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
				}
                else if (ragdollPart.collisionHandler.name.Contains("LeftHand"))
                {
                    ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartLeft;
                    ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
                }
                else if (ragdollPart.collisionHandler.name.Contains("RightForeArm"))
				{
					ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartRight;
					ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
				}
				else if (ragdollPart.collisionHandler.name.Contains("RightArm"))
				{
					ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartRight;
					ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
				}
                else if (ragdollPart.collisionHandler.name.Contains("RightHand"))
                {
                    ragdollPart.collisionHandler.OnCollisionStartEvent += this.PlayerCollisionStartRight;
                    ragdollPart.collisionHandler.OnCollisionStopEvent += this.BodypartStopColliding;
                }
            }
			Player.local.creature.GetComponentInChildren<LiquidReceiver>().OnReceptionEvent += this.OnLiquidIntake;
			UnityEngine.Debug.Log("Events registered!");
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x000278BC File Offset: 0x00025ABC
		private void OnPlayerHolsterChanged(Holder holder, Item item, bool added)
		{
            UnityEngine.Debug.Log("----------------------------");
            UnityEngine.Debug.Log("OnPlayerHolsterChanged");
            UnityEngine.Debug.Log(holder.name);
            if (!added)
			{
				if (holder.name.Contains("HipsLeft") )
				{
                    UnityEngine.Debug.Log("LeftHipSlotOutputItem");
                    _TrueGear.Play("LeftHipSlotOutputItem");
                }

				else if (holder.name.Contains("HipsRight"))
                {
                    UnityEngine.Debug.Log("RightHipSlotOutputItem");
                    _TrueGear.Play("RightHipSlotOutputItem");
                }
                else if (holder.name.Contains("BackLeft"))
                {
                    UnityEngine.Debug.Log("LeftBackSlotOutputItem");
                    _TrueGear.Play("LeftBackSlotOutputItem");
                }
                else if (holder.name.Contains("BackRight"))
                {
                    UnityEngine.Debug.Log("RightBackSlotOutputItem");
                    _TrueGear.Play("RightBackSlotOutputItem");
                }
				return;
			}
            if (holder.name.Contains("HipsLeft"))
            {
                UnityEngine.Debug.Log("LeftHipSlotInputItem");
                _TrueGear.Play("LeftHipSlotInputItem");
            }
            else if (holder.name.Contains("HipsRight"))
            {
                UnityEngine.Debug.Log("RightHipSlotInputItem");
                _TrueGear.Play("RightHipSlotInputItem");
            }
            else if (holder.name.Contains("BackLeft"))
            {
                UnityEngine.Debug.Log("LeftBackSloInputItem");
                _TrueGear.Play("LeftBackSloInputItem");
            }
            else if (holder.name.Contains("BackRight"))
            {
                UnityEngine.Debug.Log("RightBackSlotInputItem");
                _TrueGear.Play("RightBackSlotInputItem");
            }

        }
		
		// Token: 0x060005E2 RID: 1506 RVA: 0x0002796C File Offset: 0x00025B6C
		private void OnPlayerUseSpell(string spellId, Creature creature, Side side)
		{
			if (!creature.isPlayer)
			{
				return;
			}
			if (this.GetPlayerSpellFeedbackType(spellId) == "SlowMotion")
			{
				_TrueGear.Play("SlowMotion");
			}
			else if (side == Side.Left)
			{
				leftSpellName = this.GetPlayerSpellFeedbackType(spellId);
			}
			else if (side == Side.Right)
			{ 
				rightSpellName = this.GetPlayerSpellFeedbackType(spellId);
			}
		}
		
		// Token: 0x060005E3 RID: 1507 RVA: 0x000279B0 File Offset: 0x00025BB0
		private void OnPlayerConsume(Item edible, Creature consumer, EventTime eventTime)
		{
			if (!consumer.isPlayer)
			{
				return;
			}
			_TrueGear.Play("Eat");
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x000279EC File Offset: 0x00025BEC
		private void BodypartStopColliding(CollisionInstance collisionInstance)
		{
			if (collisionInstance.sourceColliderGroup != null && collisionInstance.targetColliderGroup == null && ((collisionInstance.sourceColliderGroup.collisionHandler != null) ? collisionInstance.sourceColliderGroup.collisionHandler.name : "").Contains("Hand"))
			{
				CollisionHandler collisionHandler = collisionInstance.sourceColliderGroup.collisionHandler;
				this.leftHandClimbing = ((collisionHandler.item != null) ? collisionHandler.item.leftPlayerHand : null);
			}
			if (collisionInstance.sourceColliderGroup != null && collisionInstance.targetColliderGroup == null && ((collisionInstance.sourceColliderGroup.collisionHandler != null) ? collisionInstance.sourceColliderGroup.collisionHandler.name : "").Contains("Hand"))
			{
				CollisionHandler collisionHandler2 = collisionInstance.sourceColliderGroup.collisionHandler;
				this.rightHandClimbing = ((collisionHandler2.item != null) ? collisionHandler2.item.rightPlayerHand : null);
			}
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00027B04 File Offset: 0x00025D04
		private void OnLiquidIntake(LiquidData liquid, float dilution, LiquidContainer liquidContainer)
		{
			if (liquid.GetType() != typeof(LiquidPoison))
			{
                UnityEngine.Debug.Log("-----------------------------------");
                UnityEngine.Debug.Log("Drinking");
                _TrueGear.Play("Drinking");
				return;
			}
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("Drinking");
            _TrueGear.Play("Drinking");
		}

        private static bool canTorso = true;
        // Token: 0x060005E6 RID: 1510 RVA: 0x00027B70 File Offset: 0x00025D70
        private void OnTorsoCollisionStart(CollisionInstance collisionInstance)
		{
			if (!canTorso)
			{
				return;
			}
			canTorso = false;
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("OnTorsoCollisionStart");
            UnityEngine.Debug.Log(collisionInstance.ToString());
            float angleForPosition = this.GetAngleForPosition(collisionInstance.contactPoint);
			_TrueGear.PlayAngle("BluntStoneLargeDamage",angleForPosition,0);
			Timer torsoTimer = new Timer(TorsoTimerCallBack,null,70,Timeout.Infinite);
		}
		private static void TorsoTimerCallBack(object o)
		{
            canTorso = true;
        }

		private static bool canRight = true;
        // Token: 0x060005E7 RID: 1511 RVA: 0x00027BAC File Offset: 0x00025DAC
        private void PlayerCollisionStartRight(CollisionInstance collisionInstance)
		{
            if (!canRight)
            {
                return;
            }
            canRight = false;
            string text = (collisionInstance.sourceMaterial != null) ? (collisionInstance.sourceMaterial.id + " " + ((collisionInstance.targetMaterial != null) ? collisionInstance.targetMaterial.id : "")) : "";
			text = this.ReplaceFirst(text, "Flesh", "");
			string playerPunchFeedback = this.GetPlayerPunchFeedback(text);
            UnityEngine.Debug.Log("-----------------------------------");
            UnityEngine.Debug.Log("RightCollision");
            UnityEngine.Debug.Log(text);
            UnityEngine.Debug.Log(playerPunchFeedback);
            _TrueGear.Play($"Right{playerPunchFeedback}");
            Timer rightTimer = new Timer(RightTimerCallBack, null, 70, Timeout.Infinite);
        }
        private static void RightTimerCallBack(object o)
        {
            canRight = true;
        }


        private static bool canLeft = true;
        // Token: 0x060005E8 RID: 1512 RVA: 0x00027C38 File Offset: 0x00025E38
        private void PlayerCollisionStartLeft(CollisionInstance collisionInstance)
		{
            if (!canLeft)
            {
                return;
            }
            canLeft = false;
            string text = (collisionInstance.sourceMaterial != null) ? (collisionInstance.sourceMaterial.id + " " + ((collisionInstance.targetMaterial != null) ? collisionInstance.targetMaterial.id : "")) : "";
			text = this.ReplaceFirst(text, "Flesh", "");
			string playerPunchFeedback = this.GetPlayerPunchFeedback(text);

			UnityEngine.Debug.Log("-----------------------------------");
			UnityEngine.Debug.Log("LeftCollision");
			UnityEngine.Debug.Log(text);
			UnityEngine.Debug.Log(playerPunchFeedback);

            _TrueGear.Play($"Left{playerPunchFeedback}");
            Timer leftTimer = new Timer(LeftTimerCallBack, null, 70, Timeout.Infinite);
        }
        private static void LeftTimerCallBack(object o)
        {
            canLeft = true;
        }

        // Token: 0x060005E9 RID: 1513 RVA: 0x00027CC1 File Offset: 0x00025EC1
        private void OnPlayerUnGrab(Side side, Handle handle, bool throwing, EventTime eventTime)
		{
            if (eventTime == EventTime.OnEnd || handle == null || handle.item == null || handle.item.data == null)
            {
                return;
            }
            if (side == Side.Left)
			{
				this.grabbedWithLeftHand = false;
            }
            else if (side == Side.Right)
            {
                this.grabbedWithRightHand = false;
            }

            if (handle != null)
            {
                if (side == Side.Left)
                {
                    this.grabbedWithLeftHand = false;
                    foreach (CollisionHandler collisionHandler in handle.item.collisionHandlers)
                    {
                        if (collisionHandler != null)
                        {
                            collisionHandler.OnCollisionStartEvent -= this.HeldItemLeftCollisionStart;
                        }
                    }
                    handle.item.OnHeldActionEvent -= this.LeftItemHeldActionEvent;
                    return;
                }
                if (side == Side.Right)
                {
                    this.grabbedWithRightHand = false;
                    foreach (CollisionHandler collisionHandler2 in handle.item.collisionHandlers)
                    {
                        if (collisionHandler2 != null)
                        {
                            collisionHandler2.OnCollisionStartEvent -= this.HeldItemRightCollisionStart;
                        }
                    }
                    handle.item.OnHeldActionEvent -= this.RightItemHeldActionEvent;
                    return;
                }
                
            }

        }

		// Token: 0x060005EA RID: 1514 RVA: 0x00027CC4 File Offset: 0x00025EC4
		private void OnPlayerGrab(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
		{			
			if (eventTime == EventTime.OnEnd || handle == null || handle.item == null || handle.item.data == null)
			{
				return;
			}
            if (handle.interactableId.ToLowerInvariant().Contains("ladder"))
			{
				if (side == Side.Left)
				{
					this.grabbedLadderWithLeftHand = true;
				}
				else
				{
					this.grabbedLadderWithRightHand = true;
				}
			}
            if (handle != null)
			{
				if (!this.bowStringHeld)
				{
					this.bowStringHeld = true;
					this.PerformBowStringFeedback(side);
				}
				if (side == Side.Left && !this.grabbedWithLeftHand)
				{
					this.grabbedWithLeftHand = true;
					foreach (CollisionHandler collisionHandler in handle.item.collisionHandlers)
					{
						if (collisionHandler != null)
						{
							collisionHandler.OnCollisionStartEvent += this.HeldItemLeftCollisionStart;
						}
					}
					handle.item.OnHeldActionEvent += this.LeftItemHeldActionEvent;
					return;
				}
				if (side == Side.Right && !this.grabbedWithRightHand)
				{
					this.grabbedWithRightHand = true;
					foreach (CollisionHandler collisionHandler2 in handle.item.collisionHandlers)
					{
						if (collisionHandler2 != null)
						{
							collisionHandler2.OnCollisionStartEvent += this.HeldItemRightCollisionStart;
						}
					}
					handle.item.OnHeldActionEvent += this.RightItemHeldActionEvent;
					return;
				}
                if (side == Side.Left)
                {
                    _TrueGear.Play("LeftHandPickupItem");
                }
                else if (side == Side.Right)
                {
                    _TrueGear.Play("RightHandPickupItem");
                }
            }
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00027E64 File Offset: 0x00026064
		private void OnArmourEquipped(Wearable slot, Item item)
		{
			RagdollPart.Type type = slot.Part.type;
			if (type == RagdollPart.Type.Torso)
			{
				_TrueGear.Play("EquipChest");
				return;
			}
			if (type == RagdollPart.Type.LeftArm)
			{
				_TrueGear.Play("EquipLeftGauntlets");
				return;
			}
			if (type != RagdollPart.Type.RightArm)
			{
				return;
			}
			_TrueGear.Play("EquipRightGauntlets");
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00027F00 File Offset: 0x00026100
		private void OnArmourUnEquipped(Wearable slot, Item item)
		{
			RagdollPart.Type type = slot.Part.type;
			if (type == RagdollPart.Type.Torso)
			{
				_TrueGear.Play("UnequipChest");
				return;
			}
			if (type == RagdollPart.Type.LeftArm)
			{
				_TrueGear.Play("UnequipLeftGauntlets");
				return;
			}
			if (type != RagdollPart.Type.RightArm)
			{
				return;
			}
			_TrueGear.Play("EquipRightGauntlets");
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00027F9C File Offset: 0x0002619C
		private void OnPlayerDied(CollisionInstance collisionInstance, EventTime eventTime)
		{
			_TrueGear.Play("PlayerDeath");
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00028014 File Offset: 0x00026214
		private void OnPlayerDamaged(CollisionInstance collisionInstance, EventTime eventTime)
		{
			UnityEngine.Debug.Log("-------------------------------------");
			UnityEngine.Debug.Log("Damage");

			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			UnityEngine.Debug.Log("1111111111111111111111111");
            float angleForPosition = this.GetAngleForPosition(collisionInstance.contactPoint);
			//_TrueGear.PlayAngle("DafaultDamage", angleForPosition, 0.45f);
			var damageType = this.GetPlayerMeleeFeedbackType1(collisionInstance.damageStruct.damageType, collisionInstance.sourceMaterial, collisionInstance.targetMaterial);
            UnityEngine.Debug.Log($"{damageType},{angleForPosition},0");
			if (damageType == "LightningDamage")
			{
                int[,] ele = new int[,] { { 0, 1, 4, 5, 8, 9, 12, 13, 16, 17 }, { 2, 3, 6, 7, 110, 11, 14, 15, 18, 19 }, { 100, 101, 104, 105, 108, 109, 112, 113, 116, 117 }, { 102, 103, 106, 107, 110, 111, 114, 115, 118, 119 } };
                int[] random = new int[] { 3, 3, 3, 3 };
                _TrueGear.PlayRandom("LightningDamage", ele, random);
            }
			else
            {
                _TrueGear.PlayAngle(damageType, angleForPosition, 0);
            }

        }

		// Token: 0x060005EF RID: 1519 RVA: 0x00028070 File Offset: 0x00026270
		private void OnPlayerLanded(Locomotion locomotion, Vector3 groundPoint, Vector3 velocity, Collider groundCollider)
		{
			if (velocity.magnitude > Player.local.creature.data.playerFallDamageCurve.GetFirstTime())
			{
				
				_TrueGear.Play("FallDamage");
			}
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x000280C4 File Offset: 0x000262C4
		private void HeldItemRightCollisionStart(CollisionInstance collisionInstance)
		{
			UnityEngine.Debug.Log("--------------------------------");
			UnityEngine.Debug.Log("HeldItemRightCollisionStart");
			UnityEngine.Debug.Log(collisionInstance.damageStruct.penetration);

			RagdollPart hitRagdollPart = collisionInstance.damageStruct.hitRagdollPart;
			if (((hitRagdollPart != null) ? hitRagdollPart.ragdoll.creature : null) != null)
			{
                UnityEngine.Debug.Log("RightHandMeleeHit");
                _TrueGear.Play("RightHandMeleeHit");
                return;
			}
            UnityEngine.Debug.Log("1111111111111111111111111");
            string playerMeleeFeedbackType = this.GetPlayerMeleeFeedbackType(collisionInstance.damageStruct.damageType, collisionInstance.sourceMaterial, collisionInstance.targetMaterial);
            UnityEngine.Debug.Log(playerMeleeFeedbackType);
            _TrueGear.Play($"Right{playerMeleeFeedbackType}");
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00028140 File Offset: 0x00026340
		private void HeldItemLeftCollisionStart(CollisionInstance collisionInstance)
		{
            UnityEngine.Debug.Log("--------------------------------");
            UnityEngine.Debug.Log("HeldItemLeftCollisionStart");
            UnityEngine.Debug.Log(collisionInstance.damageStruct.penetration);
            RagdollPart hitRagdollPart = collisionInstance.damageStruct.hitRagdollPart;
			if (((hitRagdollPart != null) ? hitRagdollPart.ragdoll.creature : null) != null)
			{
                UnityEngine.Debug.Log("LeftHandMeleeHit");
                _TrueGear.Play("LeftHandMeleeHit");
                return;
			}
            UnityEngine.Debug.Log("1111111111111111111111111");
            string playerMeleeFeedbackType = this.GetPlayerMeleeFeedbackType(collisionInstance.damageStruct.damageType, collisionInstance.sourceMaterial, collisionInstance.targetMaterial);
			_TrueGear.Play($"Left{playerMeleeFeedbackType}");
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x000281BC File Offset: 0x000263BC
		private void RightItemHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
		{
			if (action == Interactable.Action.UseStart)
			{
				this.rightItemUseStarted = true;
				Player local = Player.local;
				if (local.creature != null)
				{
					Item heldItem = local.creature.equipment.GetHeldItem(Side.Right);
					if (heldItem != null && heldItem.name.Contains("Grapple") && !this.noExplosionFeedback)
					{
						this.noExplosionFeedback = true;
						this.ExplosionWaitFunc();
						return;
					}
				}
			}
			else
			{
				if (action == Interactable.Action.UseStop)
				{
					this.rightItemUseStarted = false;
					return;
				}
				if (action != Interactable.Action.Ungrab)
				{
					return;
				}
				this.rightItemUseStarted = false;
            }
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00028244 File Offset: 0x00026444
		private void LeftItemHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
		{
			if (action == Interactable.Action.UseStart)
			{
				this.leftItemUseStarted = true;
				Player local = Player.local;
				if (local.creature != null)
				{
					Item heldItem = local.creature.equipment.GetHeldItem(Side.Left);
					if (heldItem != null && heldItem.name.Contains("Grapple") && !this.noExplosionFeedback)
					{
						this.noExplosionFeedback = true;
						this.ExplosionWaitFunc();
						return;
					}
				}
			}
			else
			{
				if (action == Interactable.Action.UseStop)
				{
					this.leftItemUseStarted = false;
					return;
				}
				if (action != Interactable.Action.Ungrab)
				{
					return;
				}
				this.leftItemUseStarted = false;

            }
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x000282CC File Offset: 0x000264CC
		private async void PerformTelekenesisActivationFeedback(bool leftHand)
		{
			while (((telekinesisActiveLeft && leftHand) || (telekinesisActiveRight && !leftHand)) && (!leftHand || !Player.local.creature.equipment.GetHeldItem(Side.Left)) && (leftHand || !Player.local.creature.equipment.GetHeldItem(Side.Right)))
			{
				_TrueGear.Play(leftHand ? "LeftTelekinesisActive" : "RightTelekinesisActive");
				await Task.Delay(sleepDurationSpellCast);
			}
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x0002830C File Offset: 0x0002650C
		private async void PerformTelekenesisFeedback(bool pull, bool leftHand)
		{
			string feedback = (pull ? "TelekinesisPull" : "TelekinesisRepel");
			while (((telekinesisPullRight && pull) || (telekinesisRepelRight && !pull)) && (!leftHand || !Player.local.creature.equipment.GetHeldItem(Side.Left)) && (leftHand || !Player.local.creature.equipment.GetHeldItem(Side.Right)))
			{
				_TrueGear.Play(leftHand ? $"Left{feedback}" : $"Right{feedback}");
				await Task.Delay(sleepDurationSpellCast);
			}
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00028354 File Offset: 0x00026554
		private async void PerformBowStringFeedback(Side side)
		{
			UnityEngine.Debug.Log("come PerformBowStringFeedback");
			Item heldWeapon = Player.local.creature.equipment.GetHeldWeapon(Side.Left);
			Item heldWeapon2 = Player.local.creature.equipment.GetHeldWeapon(Side.Right);
			BowString bowString = ((heldWeapon != null) ? heldWeapon.GetComponentInChildren<BowString>() : null) ?? ((heldWeapon2 != null) ? heldWeapon2.GetComponentInChildren<BowString>() : null);
            UnityEngine.Debug.Log("heldWeapon :" + (heldWeapon == null));
            UnityEngine.Debug.Log("heldWeapon2 :" + (heldWeapon2 == null));
            UnityEngine.Debug.Log("bowstring :" + (bowString == null));
            UnityEngine.Debug.Log("bowStringHeld :" + bowStringHeld);
			while (bowString && bowStringHeld)
			{
				if (Player.local.creature == null || bowString.item.handlers.Count == 0)
				{
                    UnityEngine.Debug.Log("come bowStringHeld false");
                    bowStringHeld = false;
					break;
				}
				heldWeapon = Player.local.creature.equipment.GetHeldWeapon(Side.Left);
				heldWeapon2 = Player.local.creature.equipment.GetHeldWeapon(Side.Right);
				UnityEngine.Debug.Log("isPulling" + bowString.isPulling);
				if ((heldWeapon == null && heldWeapon2 == null) || (heldWeapon != null && heldWeapon.GetComponentInChildren<BowString>() == null && heldWeapon2 != null && heldWeapon2.GetComponentInChildren<BowString>() == null))
				{
                    UnityEngine.Debug.Log("break11111");
                    break;
				}
				if (heldWeapon == heldWeapon2 && bowString.isPulling)
				{
                    UnityEngine.Debug.Log("LeftBowPull or RightBowPull");
                    _TrueGear.Play(side == Side.Left ? "LeftBowPull" : "RightBowPull");
				}
				await Task.Delay(sleepDurationBowString);
			}
            bowStringHeld = false;
            UnityEngine.Debug.Log("Out!!!!!");
        }

		// Token: 0x060005F7 RID: 1527 RVA: 0x00028394 File Offset: 0x00026594
		private async void PerformClimbingFeedback(Side side)
		{
			if (side == Side.Left)
			{
				float lastIntensity = 0f;
				while (leftHandClimbing)
				{
					float num = Math.Abs(Player.local.handLeft.transform.position.y - Player.local.head.transform.position.y);
					if (Math.Abs(lastIntensity - num) >= 0.03f)
					{
						lastIntensity = num;						
						_TrueGear.Play("LeftClimb");
					}
					await Task.Delay(sleepDurationClimb);
				}
				leftHandClimbing = false;
			}
			float lastIntensity2 = 0f;
			while (rightHandClimbing)
			{
				float num2 = Math.Abs(Player.local.handRight.transform.position.y - Player.local.head.transform.position.y);
				if (Math.Abs(lastIntensity2 - num2) >= 0.03f)
				{
					lastIntensity2 = num2;
					_TrueGear.Play("RightClimb");
				}
				await Task.Delay(sleepDurationClimb);
			}
			rightHandClimbing = false;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x000283D4 File Offset: 0x000265D4
		private async void ExplosionWaitFunc()
		{
			Player local = Player.local;
			while (true)
			{
				await Task.Delay(5000);
				if (local.creature == null || local.locomotion == null)
				{
					break;
				}
				Item heldItem = local.creature.equipment.GetHeldItem(Side.Right);
				if (!(heldItem != null) || !heldItem.name.Contains("Grapple") || !rightItemUseStarted)
				{
					Item heldItem2 = local.creature.equipment.GetHeldItem(Side.Left);
					if (!(heldItem2 != null) || !heldItem2.name.Contains("Grapple") || !leftItemUseStarted)
					{
						break;
					}
				}
			}
			noExplosionFeedback = false;
		}

		

		// Token: 0x060005FB RID: 1531 RVA: 0x00028590 File Offset: 0x00026790
		private string ReplaceFirst(string text, string search, string replace)
		{
			int num = text.IndexOf(search, StringComparison.Ordinal);
			if (num < 0)
			{
				return text;
			}
			return text.Substring(0, num) + replace + text.Substring(num + search.Length);
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x000285C8 File Offset: 0x000267C8
		private float GetAngleForPosition(Vector3 pos)
		{
			float num = 0f;
			if (Player.local != null && Player.local.creature != null && Player.local.creature.initialized)
			{
				num = Vector3.SignedAngle(Player.local.creature.transform.forward.ToXZ(), pos.ToXZ() - Player.local.creature.transform.position.ToXZ(), Vector3.up);
				if (num < 0f)
				{
					num = -180f - num;
				}
				else
				{
					num = 180f - num;
				}
				num += 180f;
			}
			return num;
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x0002867C File Offset: 0x0002687C
		private string GetPlayerSpellFeedbackType(string spellID)
		{
			spellID = spellID.ToLowerInvariant();
			if (spellID == "fire")
			{
				return "SpellFire";
			}
			if (spellID == "lightning")
			{
				return "SpellLightning";
			}
			if (spellID == "gravity")
			{
				return "SpellGravity";
			}
			if (spellID == "telekinesis")
			{
				return "TelekinesisActive";
			}
			if (spellID == "slowtime")
			{
				return "SlowMotion";
			}
			return "SpellDrain";
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x000286E4 File Offset: 0x000268E4
		private string GetPlayerMeleeFeedbackType(DamageType damageType, MaterialData sourceMaterialData, MaterialData targetMaterialData)
		{
			string text = string.Empty;
			string a = string.Empty;
			if (sourceMaterialData != null)
			{
				text = sourceMaterialData.id;
			}
			if (targetMaterialData != null)
			{
				a = targetMaterialData.id;
			}
			if (damageType == DamageType.Pierce)
			{
				if (text == "Blade" || text == "Metal")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalPierce";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeBladeWoodPierce";
					}
					if (a == "Flesh")
					{
						return "MeleeBladeFleshPierce";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeBladeFabricPierce";
					}
					return "MeleeBladeStonePierce";
				}
				else if (text == "Wood")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeWoodMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodWoodBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeWoodFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeWoodFabricBlunt";
					}
					return "MeleeWoodStoneBlunt";
				}
				else if (text == "Flesh")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "PunchMetal";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "PunchWood";
					}
					if (a == "Flesh")
					{
						return "PunchFlesh";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "PunchFabric";
					}
					if (a == "Stone" || a == "Glass")
					{
						return "PunchStone";
					}
					return "PunchOther";
				}
				else if (text.Contains("Fire"))
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalPierce";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeBladeFabricPierce";
					}
					if (a == "Flesh")
					{
						return "MeleeBladeFabricPierce";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeBladeFabricPierce";
					}
					if (a == "Stone")
					{
						return "MeleeBladeFabricPierce";
					}
					return "MeleeBladeStonePierce";
				}
				else
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodStoneBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeStoneFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeStoneFabricBlunt";
					}
					return "MeleeStoneStoneBlunt";
				}
			}
			else if (damageType == DamageType.Slash)
			{
				if (text == "Blade" || text == "Metal" || text == "SpellFire")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalSlash";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeBladeWoodSlash";
					}
					if (a == "Flesh")
					{
						return "MeleeBladeFleshSlash";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeBladeFabricSlash";
					}
					return "MeleeBladeStoneSlash";
				}
				else if (text == "Wood")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeWoodMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodWoodBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeWoodFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeWoodFabricBlunt";
					}
					return "MeleeWoodStoneBlunt";
				}
				else if (text == "Flesh")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "PunchMetal";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "PunchWood";
					}
					if (a == "Flesh")
					{
						return "PunchFlesh";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "PunchFabric";
					}
					if (a == "Stone" || a == "Glass")
					{
						return "PunchStone";
					}
					return "PunchOther";
				}
				else
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodStoneBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeStoneFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeStoneFabricBlunt";
					}
					return "MeleeStoneStoneBlunt";
				}
			}
			else
			{
				if (damageType == DamageType.Energy)
				{
					return "FireDamage";
				}
                if (damageType == DamageType.Unknown)
				{
					return "DefaultDamage";
				}
				if (text == "Blade" || text == "Metal" || text == "SpellFire")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeBladeWoodBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeBladeFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeBladeFabricBlunt";
					}
					return "MeleeBladeStoneBlunt";
				}
				else if (text == "Wood")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeWoodMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodWoodBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeWoodFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeWoodFabricBlunt";
					}
					return "MeleeWoodStoneBlunt";
				}
				else if (text == "Flesh")
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "PunchMetal";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "PunchWood";
					}
					if (a == "Flesh")
					{
						return "PunchFlesh";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "PunchFabric";
					}
					if (a == "Stone" || a == "Glass")
					{
						return "PunchStone";
					}
					return "PunchOther";
				}
				else
				{
					if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
					{
						return "MeleeBladeMetalBlunt";
					}
					if (a == "Wood" || a == "Arrow")
					{
						return "MeleeWoodStoneBlunt";
					}
					if (a == "Flesh")
					{
						return "MeleeStoneFleshBlunt";
					}
					if (a == "Fabric" || a == "Leather" || a == "Rope")
					{
						return "MeleeStoneFabricBlunt";
					}
					if (a == "Projectile" && text == "Flesh")
					{
						return "BluntFleshLargeDamage";
					}
					return "MeleeStoneStoneBlunt";
				}				
			}
		}

        private string GetPlayerMeleeFeedbackType1(DamageType damageType, MaterialData sourceMaterialData, MaterialData targetMaterialData)
        {
            string text = string.Empty;
            string a = string.Empty;
            if (sourceMaterialData != null)
            {
                text = sourceMaterialData.id;
            }
            if (targetMaterialData != null)
            {
                a = targetMaterialData.id;
            }
            if (damageType == DamageType.Pierce)
            {
                if (text == "Blade" || text == "Metal")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalPierce";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeBladeWoodPierce";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeBladeFleshPierce";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeBladeFabricPierce";
                    }
                    return "MeleeBladeStonePierce";
                }
                else if (text == "Wood")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeWoodMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodWoodBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeWoodFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeWoodFabricBlunt";
                    }
                    return "MeleeWoodStoneBlunt";
                }
                else if (text == "Flesh")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "PunchMetal";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "PunchWood";
                    }
                    if (a == "Flesh")
                    {
                        return "PunchFlesh";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "PunchFabric";
                    }
                    if (a == "Stone" || a == "Glass")
                    {
                        return "PunchStone";
                    }
                    return "PunchOther";
                }
                else if (text.Contains("Fire"))
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalPierce";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeBladeFabricPierce";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeBladeFabricPierce";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeBladeFabricPierce";
                    }
                    if (a == "Stone")
                    {
                        return "MeleeBladeFabricPierce";
                    }
                    return "MeleeBladeStonePierce";
                }
                else
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodStoneBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeStoneFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeStoneFabricBlunt";
                    }
                    return "MeleeStoneStoneBlunt";
                }
            }
            else if (damageType == DamageType.Slash)
            {
                if (text == "Blade" || text == "Metal" || text == "SpellFire")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalSlash";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeBladeWoodSlash";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeBladeFleshSlash";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeBladeFabricSlash";
                    }
                    return "MeleeBladeStoneSlash";
                }
                else if (text == "Wood")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeWoodMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodWoodBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeWoodFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeWoodFabricBlunt";
                    }
                    return "MeleeWoodStoneBlunt";
                }
                else if (text == "Flesh")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "PunchMetal";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "PunchWood";
                    }
                    if (a == "Flesh")
                    {
                        return "PunchFlesh";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "PunchFabric";
                    }
                    if (a == "Stone" || a == "Glass")
                    {
                        return "PunchStone";
                    }
                    return "PunchOther";
                }
                else
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodStoneBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeStoneFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeStoneFabricBlunt";
                    }
                    return "MeleeStoneStoneBlunt";
                }
            }
            else
            {
                if (damageType == DamageType.Energy)
                {
                    return "FireDamage";
                }
                if (damageType == DamageType.Unknown)
                {
                    return "DefaultDamage";
                }
                if (text == "Blade" || text == "Metal" || text == "SpellFire")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeBladeWoodBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeBladeFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeBladeFabricBlunt";
                    }
                    return "MeleeBladeStoneBlunt";
                }
                else if (text == "Wood")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeWoodMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodWoodBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeWoodFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeWoodFabricBlunt";
                    }
                    return "MeleeWoodStoneBlunt";
                }
                else if (text == "Flesh")
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "PunchMetal";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "PunchWood";
                    }
                    if (a == "Flesh")
                    {
                        return "PunchFlesh";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "PunchFabric";
                    }
                    if (a == "Stone" || a == "Glass")
                    {
                        return "PunchStone";
                    }
                    return "PunchOther";
                }
                else
                {
                    if (a == "Blade" || a == "Metal" || a == "Chainmail" || a == "Plate" || a == "Lightsaber")
                    {
                        return "MeleeBladeMetalBlunt";
                    }
                    if (a == "Wood" || a == "Arrow")
                    {
                        return "MeleeWoodStoneBlunt";
                    }
                    if (a == "Flesh")
                    {
                        return "MeleeStoneFleshBlunt";
                    }
                    if (a == "Fabric" || a == "Leather" || a == "Rope")
                    {
                        return "MeleeStoneFabricBlunt";
                    }
                    if (a == "Projectile" && text == "Flesh")
                    {
                        return "BluntFleshLargeDamage";
                    }
                    return "LightningDamage";
                }
            }
        }

        // Token: 0x060005FF RID: 1535 RVA: 0x000290C0 File Offset: 0x000272C0
        private string GetPlayerPunchFeedback(string material)
		{
			if (string.IsNullOrEmpty(material))
			{
				return "PunchOther";
			}
			if (material.Contains("Wood"))
			{
				return "PunchWood";
			}
			if (material.Contains("Stone") || material.Contains("Glass"))
			{
				return "PunchStone";
			}
			if (material.Contains("Metal"))
			{
				return "PunchMetal";
			}
			if (material.Contains("Fabric"))
			{
				return "PunchFabric";
			}
			if (material.Contains("Flesh") || material.Contains("Sand"))
			{
				return "PunchFlesh";
			}
			return "PunchOther";
		}

		// Token: 0x040004EB RID: 1259
		private bool leftHandClimbing;

		// Token: 0x040004EC RID: 1260
		private bool rightHandClimbing;

		// Token: 0x040004ED RID: 1261
		private bool grabbedLadderWithLeftHand;

		// Token: 0x040004EE RID: 1262
		private bool grabbedLadderWithRightHand;

		// Token: 0x040004EF RID: 1263
		private bool bowStringHeld;

		// Token: 0x040004F0 RID: 1264
		private bool grabbedWithRightHand;

		// Token: 0x040004F1 RID: 1265
		private bool grabbedWithLeftHand;

		// Token: 0x040004F2 RID: 1266
		private bool noExplosionFeedback;

		// Token: 0x040004F3 RID: 1267
		private bool rightItemUseStarted;

		// Token: 0x040004F4 RID: 1268
		private bool leftItemUseStarted;

		// Token: 0x040004F5 RID: 1269
		private bool telekinesisActiveLeft;

		// Token: 0x040004F6 RID: 1270
		private bool telekinesisPullLeft;

		// Token: 0x040004F7 RID: 1271
		private bool telekinesisRepelLeft;

		// Token: 0x040004F8 RID: 1272
		private bool telekinesisCatchLeftLast;

		// Token: 0x040004F9 RID: 1273
		private bool telekinesisActiveRight;

		// Token: 0x040004FA RID: 1274
		private bool telekinesisPullRight;

		// Token: 0x040004FB RID: 1275
		private bool telekinesisRepelRight;

		// Token: 0x040004FC RID: 1276
		private bool telekinesisCatchRightLast;

		// Token: 0x040004FD RID: 1277
		private float climbingCheckTimeLeft;		

		// Token: 0x04000501 RID: 1281
		private readonly float TOLERANCE = 0.0001f;

		private static bool tgConnectedToAreaEvents = false;


    }
}









