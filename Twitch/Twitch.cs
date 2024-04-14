using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.TwitchStates;
using KinematicCharacterController;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using Twitch.Unlockables;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace Twitch
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.rob.Twitch", "Twitch", "2.3.1")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Twitch : BaseUnityPlugin
    {
        public static Twitch Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            ReadConfig();
            Assets.PopulateAssets();
            RegisterStates();
            Twitch.CreatePrefab();
            RegisterBuff();
            RegisterUnlockables();
            RegisterCharacter();
            RegisterHooks();
        }

        private void ReadConfig()
        {
            Twitch.how = Config.Bind(new ConfigDefinition("01 - General Settings", "HOW"), false, new ConfigDescription("HOW IS THIS RAT", null, Array.Empty<object>()));
            Twitch.boom = Config.Bind(new ConfigDefinition("01 - General Settings", "Boom"), false, new ConfigDescription("Enables Bazooka", null, Array.Empty<object>()));
        }

        private void RegisterUnlockables()
        {
            LanguageAPI.Add("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Twitch: Mastery");
            LanguageAPI.Add("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Twitch, beat the game or obliterate on Monsoon.");
            LanguageAPI.Add("ROB_TWITCH_MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Twitch: Mastery");
            LanguageAPI.Add("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_NAME", "Twitch: Pest of Aphelia");
            LanguageAPI.Add("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_DESC", "As Twitch, get killed by a Clay Dunestrider.");
            LanguageAPI.Add("ROB_TWITCH_TARUNLOCKABLE_UNLOCKABLE_NAME", "Twitch: Pest of Aphelia");
            LanguageAPI.Add("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_NAME", "Twitch: Pestilence");
            LanguageAPI.Add("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_DESC", "As Twitch, expunge 40 stacks of venom on a single target.");
            LanguageAPI.Add("ROB_TWITCH_SIMPLEUNLOCKABLE_UNLOCKABLE_NAME", "Twitch: Pestilence");
            MasteryUnlockableDef = UnlockableAPI.AddUnlockable<MasteryUnlockable>();
            TarUnlockableDef = UnlockableAPI.AddUnlockable<TarUnlockable>();
            SimpleUnlockableDef = UnlockableAPI.AddUnlockable<SimpleUnlockable>();
        }

        private void RegisterDot()
        {
        }

        private void RegisterHooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig.Invoke(self);
            bool flag = self && self.HasBuff(Twitch.venomDebuff);
            if (flag)
            {
                float num = 1f - 0.035f * (float)self.GetBuffCount(Twitch.venomDebuff);
                bool flag2 = num < 0.1f;
                if (flag2)
                {
                    num = 0.1f;
                }
                Reflection.SetPropertyValue(self, "attackSpeed", self.attackSpeed * num);
                Reflection.SetPropertyValue(self, "armor", self.armor - 1.5f * (float)self.GetBuffCount(Twitch.venomDebuff));
            }
            bool flag3 = self && self.HasBuff(Twitch.ambushBuff);
            if (flag3)
            {
                Reflection.SetPropertyValue(self, "attackSpeed", self.attackSpeed + 1f);
            }
            bool flag4 = self && self.HasBuff(Twitch.ambushBuff);
            if (flag4)
            {
                int buffCount = self.GetBuffCount(Twitch.expungedDebuff);
                float num2 = 1f - 0.045f * (float)buffCount;
                bool flag5 = num2 < 0.1f;
                if (flag5)
                {
                    num2 = 0.1f;
                }
                Reflection.SetPropertyValue(self, "attackSpeed", self.attackSpeed * num2);
                Reflection.SetPropertyValue(self, "armor", self.armor - (float)(5 * buffCount));
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di)
        {
            bool flag = di.attacker != null;
            bool flag2 = flag;
            if (flag2)
            {
                bool flag3 = self != null;
                bool flag4 = flag3;
                if (flag4)
                {
                    bool flag5 = self.GetComponent<CharacterBody>() != null;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        bool flag7 = di.damageType.HasFlag(DamageType.BlightOnHit);
                        bool flag8 = flag7;
                        if (flag8)
                        {
                            bool flag9 = di.attacker.GetComponent<CharacterBody>();
                            bool flag10 = flag9;
                            if (flag10)
                            {
                                bool flag11 = di.attacker.GetComponent<CharacterBody>().baseNameToken == "TWITCH_NAME";
                                bool flag12 = flag11;
                                if (flag12)
                                {
                                    di.damageType = DamageType.Generic;
                                    bool flag13 = !self.GetComponent<CharacterBody>().HasBuff(Twitch.expungedDebuff);
                                    if (flag13)
                                    {
                                        self.GetComponent<CharacterBody>().AddTimedBuff(Twitch.venomDebuff, 5f * di.procCoefficient);
                                    }
                                    bool flag14 = di.attacker.GetComponent<TwitchController>();
                                    bool flag15 = flag14;
                                    if (flag15)
                                    {
                                        di.attacker.GetComponent<TwitchController>().RefundCooldown(di.procCoefficient);
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool flag16 = di.damageType.HasFlag(DamageType.PoisonOnHit);
                            bool flag17 = flag16;
                            if (flag17)
                            {
                                bool flag18 = di.attacker.GetComponent<CharacterBody>();
                                bool flag19 = flag18;
                                if (flag19)
                                {
                                    bool flag20 = di.attacker.GetComponent<CharacterBody>().baseNameToken == "TWITCH_NAME";
                                    bool flag21 = flag20;
                                    if (flag21)
                                    {
                                        di.damageType = DamageType.Generic;
                                        Util.PlaySound(Sounds.TwitchExpungeHit, self.gameObject);
                                        bool flag22 = !self.GetComponent<CharacterBody>().HasBuff(Twitch.expungedDebuff);
                                        bool flag23 = flag22;
                                        if (flag23)
                                        {
                                            CharacterBody component = self.GetComponent<CharacterBody>();
                                            bool flag24 = component.HasBuff(Twitch.venomDebuff);
                                            if (flag24)
                                            {
                                                int buffCount = component.GetBuffCount(Twitch.venomDebuff);
                                                for (int i = 0; i < buffCount; i++)
                                                {
                                                    component.AddBuff(Twitch.expungedDebuff);
                                                    component.RemoveBuff(Twitch.venomDebuff);
                                                    di.damage += di.attacker.GetComponent<CharacterBody>().damage * TwitchExpunge.damageBonus;
                                                }
                                                EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/BeetleSpitExplosion"), new EffectData
                                                {
                                                    origin = self.transform.position,
                                                    scale = (float)buffCount
                                                }, true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            orig.Invoke(self, di);
        }

        private void RegisterBuff()
        {
            var venomDebuff = ScriptableObject.CreateInstance<BuffDef>();
            venomDebuff.buffColor = Twitch.poisonColor;
            venomDebuff.canStack = true;
            venomDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffBleedingIcon.tif").WaitForCompletion();
            venomDebuff.isDebuff = true;
            venomDebuff.name = "TwitchVenomDebuff";
            Twitch.venomDebuff = venomDebuff;
            ContentAddition.AddBuffDef(venomDebuff);

            var ambushBuff = ScriptableObject.CreateInstance<BuffDef>();
            ambushBuff.buffColor = Twitch.characterColor;
            ambushBuff.canStack = false;
            ambushBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png").WaitForCompletion();
            ambushBuff.isDebuff = false;
            ambushBuff.name = "TwitchAmbushBuff";
            Twitch.ambushBuff = ambushBuff;
            ContentAddition.AddBuffDef(ambushBuff);

            var expungedDebuff = ScriptableObject.CreateInstance<BuffDef>();
            expungedDebuff.buffColor = Twitch.characterColor;
            expungedDebuff.canStack = true;
            expungedDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion();
            expungedDebuff.isDebuff = true;
            expungedDebuff.name = "TwitchExpungedDebuff";
            Twitch.expungedDebuff = expungedDebuff;
            ContentAddition.AddBuffDef(expungedDebuff);
        }

        // Token: 0x0600005D RID: 93 RVA: 0x000052F0 File Offset: 0x000034F0
        private static GameObject CreateModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);
            return Assets.MainAssetBundle.LoadAsset<GameObject>("mdlTwitch");
        }

        // Token: 0x0600005E RID: 94 RVA: 0x00005364 File Offset: 0x00003564
        internal static void CreatePrefab()
        {
            Twitch.characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "TwitchBody", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "CreatePrefab", 305);
            Twitch.characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = Twitch.characterPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;
            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;
            GameObject gameObject4 = Twitch.CreateModel(Twitch.characterPrefab);
            Transform transform = gameObject4.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;
            CharacterDirection component = Twitch.characterPrefab.GetComponent<CharacterDirection>();
            component.moveVector = Vector3.zero;
            component.targetTransform = gameObject.transform;
            component.overrideAnimatorForwardTransform = null;
            component.rootMotionAccumulator = null;
            component.modelAnimator = gameObject4.GetComponentInChildren<Animator>();
            component.driveFromRootRotation = false;
            component.turnSpeed = 720f;
            CharacterBody component2 = Twitch.characterPrefab.GetComponent<CharacterBody>();
            component2.name = "TwitchBody";
            component2.baseNameToken = "TWITCH_NAME";
            component2.subtitleNameToken = "TWITCH_SUBTITLE";
            component2.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            component2.rootMotionInMainState = false;
            component2.mainRootSpeed = 0f;
            component2.baseMaxHealth = 90f;
            component2.levelMaxHealth = 24f;
            component2.baseRegen = 0.5f;
            component2.levelRegen = 0.25f;
            component2.baseMaxShield = 0f;
            component2.levelMaxShield = 0f;
            component2.baseMoveSpeed = 7f;
            component2.levelMoveSpeed = 0f;
            component2.baseAcceleration = 80f;
            component2.baseJumpPower = 15f;
            component2.levelJumpPower = 0f;
            component2.baseDamage = 15f;
            component2.levelDamage = 3f;
            component2.baseAttackSpeed = 1f;
            component2.levelAttackSpeed = 0.02f;
            component2.baseCrit = 1f;
            component2.levelCrit = 0f;
            component2.baseArmor = 0f;
            component2.levelArmor = 0f;
            component2.baseJumpCount = 1;
            component2.sprintingSpeedMultiplier = 1.5f;
            component2.wasLucky = false;
            component2.hideCrosshair = false;
            component2.aimOriginTransform = gameObject3.transform;
            component2.hullClassification = HullClassification.Human;
            component2.portraitIcon = Assets.charPortrait;
            component2.isChampion = false;
            component2.currentVehicle = null;
            component2.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().preferredPodPrefab;
            component2.preferredInitialStateType = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().preferredInitialStateType;
            component2.skinIndex = 0U;
            CharacterMotor component3 = Twitch.characterPrefab.GetComponent<CharacterMotor>();
            component3.walkSpeedPenaltyCoefficient = 1f;
            component3.characterDirection = component;
            component3.muteWalkMotion = false;
            component3.mass = 100f;
            component3.airControl = 0.25f;
            component3.disableAirControlUntilCollision = false;
            component3.generateParametersOnAwake = true;
            InputBankTest component4 = Twitch.characterPrefab.GetComponent<InputBankTest>();
            component4.moveVector = Vector3.zero;
            CameraTargetParams component5 = Twitch.characterPrefab.GetComponent<CameraTargetParams>();
            component5.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams;
            component5.cameraPivotTransform = null;
            component5.recoil = Vector2.zero;
            //component5.cameraParams.data.idealLocalCameraPos = Vector3.zero;
            component5.dontRaycastToPivot = false;
            ModelLocator component6 = Twitch.characterPrefab.GetComponent<ModelLocator>();
            component6.modelTransform = transform;
            component6.modelBaseTransform = gameObject.transform;
            component6.dontReleaseModelOnDeath = false;
            component6.autoUpdateModelTransform = true;
            component6.dontDetatchFromParent = false;
            component6.noCorpse = false;
            component6.normalizeToFloor = true;
            component6.preserveModel = false;
            ChildLocator component7 = gameObject4.GetComponent<ChildLocator>();
            CharacterModel characterModel = gameObject4.AddComponent<CharacterModel>();
            characterModel.body = component2;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = gameObject4.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = gameObject4.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = component7.FindChild("CheeseModel").GetComponentInChildren<MeshRenderer>().material,
                    renderer = component7.FindChild("CheeseModel").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = component7.FindChild("Gun").GetComponentInChildren<MeshRenderer>().material,
                    renderer = component7.FindChild("Gun").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = component7.FindChild("Bazooka").GetComponentInChildren<MeshRenderer>().material,
                    renderer = component7.FindChild("Bazooka").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = component7.FindChild("Shotgun").GetComponentInChildren<MeshRenderer>().material,
                    renderer = component7.FindChild("Shotgun").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            Reflection.SetFieldValue(characterModel, "mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());
            bool flag = Twitch.characterPrefab.GetComponent<TeamComponent>() != null;
            TeamComponent component8;
            if (flag)
            {
                component8 = Twitch.characterPrefab.GetComponent<TeamComponent>();
            }
            else
            {
                component8 = Twitch.characterPrefab.GetComponent<TeamComponent>();
            }
            component8.hideAllyCardDisplay = false;
            component8.teamIndex = TeamIndex.None;
            HealthComponent component9 = Twitch.characterPrefab.GetComponent<HealthComponent>();
            component9.health = 120f;
            component9.shield = 0f;
            component9.barrier = 0f;
            component9.magnetiCharge = 0f;
            component9.body = null;
            component9.dontShowHealthbar = false;
            component9.globalDeathEventChanceCoefficient = 1f;
            Twitch.characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            Twitch.characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;
            CharacterDeathBehavior component10 = Twitch.characterPrefab.GetComponent<CharacterDeathBehavior>();
            component10.deathStateMachine = Twitch.characterPrefab.GetComponent<EntityStateMachine>();
            component10.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));
            SfxLocator component11 = Twitch.characterPrefab.GetComponent<SfxLocator>();
            component11.deathSound = "Play_ui_player_death";
            component11.barkSound = "";
            component11.openSound = "";
            component11.landingSound = "Play_char_land";
            component11.fallDamageSound = "Play_char_land_fall_damage";
            component11.aliveLoopStart = "";
            component11.aliveLoopStop = "";
            Rigidbody component12 = Twitch.characterPrefab.GetComponent<Rigidbody>();
            component12.mass = 100f;
            component12.drag = 0f;
            component12.angularDrag = 0f;
            component12.useGravity = false;
            component12.isKinematic = true;
            component12.interpolation = RigidbodyInterpolation.None;
            component12.collisionDetectionMode = CollisionDetectionMode.Discrete;
            component12.constraints = RigidbodyConstraints.None;
            CapsuleCollider component13 = Twitch.characterPrefab.GetComponent<CapsuleCollider>();
            component13.isTrigger = false;
            component13.material = null;
            component13.center = new Vector3(0f, 0f, 0f);
            component13.radius = 0.5f;
            component13.height = 1.82f;
            component13.direction = 1;
            KinematicCharacterMotor component14 = Twitch.characterPrefab.GetComponent<KinematicCharacterMotor>();
            component14.CharacterController = component3;
            component14.Capsule = component13;
            component14.Rigidbody = component12;
            component13.radius = 0.5f;
            component13.height = 1.82f;
            component13.center = new Vector3(0f, 0f, 0f);
            component13.material = null;
            component14.DetectDiscreteCollisions = false;
            component14.GroundDetectionExtraDistance = 0f;
            component14.MaxStepHeight = 0.2f;
            component14.MinRequiredStepDepth = 0.1f;
            component14.MaxStableSlopeAngle = 55f;
            component14.MaxStableDistanceFromLedge = 0.5f;
            component14.PreventSnappingOnLedges = false;
            component14.MaxStableDenivelationAngle = 55f;
            component14.RigidbodyInteractionType = RigidbodyInteractionType.None;
            component14.PreserveAttachedRigidbodyMomentum = true;
            component14.HasPlanarConstraint = false;
            component14.PlanarConstraintAxis = Vector3.up;
            component14.StepHandling = StepHandlingMethod.None;
            component14.LedgeHandling = true;
            component14.InteractiveRigidbodyHandling = true;
            component14.SafeMovement = false;
            HurtBoxGroup hurtBoxGroup = gameObject4.AddComponent<HurtBoxGroup>();
            HurtBox hurtBox = gameObject4.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            hurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            hurtBox.healthComponent = component9;
            hurtBox.isBullseye = true;
            hurtBox.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox.hurtBoxGroup = hurtBoxGroup;
            hurtBox.indexInGroup = 0;
            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                hurtBox
            };
            hurtBoxGroup.mainHurtBox = hurtBox;
            hurtBoxGroup.bullseyeCount = 1;
            FootstepHandler footstepHandler = gameObject4.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");
            RagdollController ragdollController = gameObject4.AddComponent<RagdollController>();
            ragdollController.bones = null;
            ragdollController.componentsToDisableOnRagdoll = null;
            AimAnimator aimAnimator = gameObject4.AddComponent<AimAnimator>();
            aimAnimator.inputBank = component4;
            aimAnimator.directionComponent = component;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;
            Twitch.characterPrefab.AddComponent<TwitchController>();
        }

        // Token: 0x0600005F RID: 95 RVA: 0x00005EC8 File Offset: 0x000040C8
        private void FindComponents(GameObject obj)
        {
            bool flag = obj;
            if (flag)
            {
                Debug.Log("Listing components on " + obj.name);
                foreach (Component component in obj.GetComponentsInChildren<Component>())
                {
                    bool flag2 = component;
                    if (flag2)
                    {
                        Debug.Log(component.gameObject.name + " has component " + component.GetType().Name);
                    }
                }
            }
        }

        // Token: 0x06000060 RID: 96 RVA: 0x00005F48 File Offset: 0x00004148
        private void RegisterCharacter()
        {
            characterDisplay = PrefabAPI.InstantiateClone(Twitch.characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "TwitchDisplay", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 673);
            characterDisplay.AddComponent<NetworkIdentity>();
            characterDisplay.AddComponent<MenuAnim>();
            foreach (ParticleSystem particleSystem in Twitch.characterPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                bool flag = particleSystem.transform.parent.name == "GrenadeFlash";
                if (flag)
                {
                    particleSystem.gameObject.AddComponent<TwitchGrenadeTicker>();
                }
            }
            Twitch.boltProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/SyringeProjectile"), "Prefabs/Projectiles/CrossbowBoltProjectile", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 688);
            GameObject gameObject = PrefabAPI.InstantiateClone(Assets.arrowModel, "TwitchArrowModel", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 690);
            gameObject.AddComponent<NetworkIdentity>();
            gameObject.AddComponent<ProjectileGhostController>();
            Twitch.boltProjectile.GetComponent<ProjectileController>().ghostPrefab = gameObject;
            Twitch.boltProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed *= 1.5f;
            Twitch.boltProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            Twitch.boltProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            Twitch.boltProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.BlightOnHit;
            Twitch.expungeProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/SyringeProjectile"), "Prefabs/Projectiles/ExpungeProjectile", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 703);
            Twitch.expungeProjectile.transform.localScale *= 1.5f;
            Twitch.expungeProjectile.GetComponent<ProjectileSimple>().velocity *= 0.75f;
            Twitch.expungeProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            Twitch.expungeProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            Twitch.expungeProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.PoisonOnHit;
            GameObject gameObject2 = PrefabAPI.InstantiateClone(Assets.knifeModel, "TwitchArrowModel", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 712);
            gameObject2.AddComponent<NetworkIdentity>();
            gameObject2.AddComponent<ProjectileGhostController>();
            gameObject2.transform.GetChild(0).localScale *= 2f;
            Twitch.expungeProjectile.GetComponent<ProjectileController>().ghostPrefab = gameObject2;
            Twitch.venomPool = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/CrocoLeapAcid"), "Prefabs/Projectiles/VenomPool", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 720);
            Twitch.venomPool.GetComponent<ProjectileDamage>().damageType = DamageType.BlightOnHit;
            Twitch.venomPool.GetComponent<ProjectileController>().procCoefficient = 0.6f;
            Twitch.caskProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile"), "Prefabs/Projectiles/VenomCaskProjectile", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 725);
            Twitch.caskProjectile.transform.localScale *= 0.5f;
            Twitch.caskProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            Twitch.caskProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.BlightOnHit;
            Twitch.caskProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            Twitch.caskProjectile.GetComponent<ProjectileSimple>().enableVelocityOverLifetime = Resources.Load<GameObject>("Prefabs/Projectiles/BeetleQueenSpit").GetComponent<ProjectileSimple>().enableVelocityOverLifetime;
            Twitch.caskProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = Resources.Load<GameObject>("Prefabs/Projectiles/BeetleQueenSpit").GetComponent<ProjectileSimple>().velocity;
            Twitch.caskProjectile.GetComponent<ProjectileSimple>().velocityOverLifetime = Resources.Load<GameObject>("Prefabs/Projectiles/BeetleQueenSpit").GetComponent<ProjectileSimple>().velocityOverLifetime;
            ProjectileImpactExplosion component = Twitch.caskProjectile.GetComponent<ProjectileImpactExplosion>();
            ProjectileImpactExplosion component2 = Resources.Load<GameObject>("Prefabs/Projectiles/BeetleQueenSpit").GetComponent<ProjectileImpactExplosion>();
            component.lifetimeExpiredSoundString = "";
            component.blastDamageCoefficient = 1f;
            component.blastProcCoefficient = 1f;
            component.blastRadius = 8f;
            component.bonusBlastForce = Vector3.zero;
            component.falloffModel = BlastAttack.FalloffModel.None;
            component.childrenProjectilePrefab = Twitch.venomPool;
            component.fireChildren = component2.fireChildren;
            component.childrenDamageCoefficient = component2.childrenDamageCoefficient;
            component.childrenCount = component2.childrenCount;
            component.impactEffect = component2.impactEffect;
            component.maxAngleOffset = component2.maxAngleOffset;
            component.minAngleOffset = component2.minAngleOffset;
            component.destroyOnEnemy = true;
            component.destroyOnWorld = true;
            component.explosionSoundString = Sounds.TwitchCaskHit;
            GameObject gameObject3 = PrefabAPI.InstantiateClone(Assets.caskModel, "VenomCaskModel", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 754);
            gameObject3.AddComponent<NetworkIdentity>();
            gameObject3.AddComponent<ProjectileGhostController>();
            Twitch.caskProjectile.GetComponent<ProjectileController>().ghostPrefab = gameObject3;
            Twitch.grenadeProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile"), "Prefabs/Projectiles/TwitchGrenadeProjectile", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 760);
            Twitch.grenadeProjectile.AddComponent<TwitchGrenadeMain>();
            Twitch.grenadeProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            Twitch.grenadeProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
            Twitch.grenadeProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            ProjectileImpactExplosion component3 = Twitch.grenadeProjectile.GetComponent<ProjectileImpactExplosion>();
            component3.blastDamageCoefficient = 1f;
            component3.blastProcCoefficient = 1f;
            component3.blastRadius = 12f;
            component3.bonusBlastForce = Vector3.zero;
            component3.falloffModel = BlastAttack.FalloffModel.None;
            component3.lifetime = 4f;
            component3.timerAfterImpact = false;
            component3.lifetimeExpiredSoundString = "";
            GameObject gameObject4 = PrefabAPI.InstantiateClone(Assets.grenadeModel, "TwitchFragGrenadeModel", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 777);
            gameObject4.AddComponent<NetworkIdentity>();
            gameObject4.AddComponent<ProjectileGhostController>();
            gameObject4.AddComponent<TwitchGrenadeController>();
            Twitch.grenadeProjectile.GetComponent<ProjectileController>().ghostPrefab = gameObject4;
            Twitch.bazookaProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile"), "Prefabs/Projectiles/TwitchBazookaProjectile", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 784);
            Twitch.bazookaProjectile.transform.localScale *= 3f;
            Twitch.bazookaProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            Twitch.bazookaProjectile.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
            Twitch.bazookaProjectile.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = 1f;
            Twitch.bazookaProjectile.GetComponent<ProjectileImpactExplosion>().blastRadius = 8f;
            Twitch.bazookaProjectile.GetComponent<ProjectileImpactExplosion>().lifetimeAfterImpact = 0f;
            Twitch.bazookaProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = Resources.Load<GameObject>("Prefabs/Projectiles/LemurianBigFireball").GetComponent<ProjectileImpactExplosion>().impactEffect;
            GameObject gameObject5 = PrefabAPI.InstantiateClone(Assets.bazookaRocketModel, "BazookaRocketModel", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 797);
            gameObject5.AddComponent<NetworkIdentity>();
            gameObject5.AddComponent<ProjectileGhostController>();
            gameObject5.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 90f, 0f);
            gameObject5.transform.GetChild(0).localScale *= 0.35f;
            gameObject5.transform.GetChild(0).GetChild(0).gameObject.AddComponent<SeparateFromParent>();
            Twitch.bazookaProjectile.GetComponent<ProjectileController>().ghostPrefab = gameObject5;
            Twitch.laserTracer = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar"), "TwitchLaserTracer", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 808);
            bool flag2 = !Twitch.laserTracer.GetComponent<EffectComponent>();
            if (flag2)
            {
                Twitch.laserTracer.AddComponent<EffectComponent>();
            }
            bool flag3 = !Twitch.laserTracer.GetComponent<VFXAttributes>();
            if (flag3)
            {
                Twitch.laserTracer.AddComponent<VFXAttributes>();
            }
            bool flag4 = !Twitch.laserTracer.GetComponent<NetworkIdentity>();
            if (flag4)
            {
                Twitch.laserTracer.AddComponent<NetworkIdentity>();
            }
            foreach (LineRenderer lineRenderer in Twitch.laserTracer.GetComponentsInChildren<LineRenderer>())
            {
                bool flag5 = lineRenderer;
                if (flag5)
                {
                    Material material = Instantiate<Material>(lineRenderer.material);
                    material.SetColor("_TintColor", Twitch.characterColor);
                    lineRenderer.material = material;
                    lineRenderer.startColor = Twitch.characterColor;
                    lineRenderer.endColor = Twitch.characterColor;
                }
            }
            foreach (Light light in Twitch.laserTracer.GetComponentsInChildren<Light>())
            {
                bool flag6 = light;
                if (flag6)
                {
                    light.color = Twitch.characterColor;
                }
            }
            foreach (MeshRenderer meshRenderer in Twitch.laserTracer.GetComponentsInChildren<MeshRenderer>())
            {
                bool flag7 = meshRenderer;
                if (flag7)
                {
                    meshRenderer.enabled = false;
                }
            }

            LaserTracerMaterial = Twitch.laserTracer.transform.Find("BeamObject").GetComponentInChildren<ParticleSystemRenderer>().trailMaterial;

            bool flag8 = Twitch.characterPrefab;
            if (flag8)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.characterPrefab, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 837);
            }
            bool flag9 = characterDisplay;
            if (flag9)
            {
                PrefabAPI.RegisterNetworkPrefab(characterDisplay, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 838);
            }
            bool flag10 = Twitch.boltProjectile;
            if (flag10)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.boltProjectile, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 839);
            }
            bool flag11 = Twitch.expungeProjectile;
            if (flag11)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.expungeProjectile, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 840);
            }
            bool flag12 = Twitch.caskProjectile;
            if (flag12)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.caskProjectile, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 841);
            }
            bool flag13 = Twitch.venomPool;
            if (flag13)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.venomPool, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 842);
            }
            bool flag14 = Twitch.bazookaProjectile;
            if (flag14)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.bazookaProjectile, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 843);
            }
            bool flag15 = Twitch.grenadeProjectile;
            if (flag15)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.grenadeProjectile, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 844);
            }
            bool flag16 = Twitch.laserTracer;
            if (flag16)
            {
                PrefabAPI.RegisterNetworkPrefab(Twitch.laserTracer, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "RegisterCharacter", 845);
            }
            ProjectileCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(Twitch.boltProjectile);
                list.Add(Twitch.bazookaProjectile);
                list.Add(Twitch.caskProjectile);
                list.Add(Twitch.venomPool);
                list.Add(Twitch.expungeProjectile);
                list.Add(Twitch.grenadeProjectile);
            };
            ContentAddition.AddEffect(Twitch.laserTracer);
            string text = "Twitch is a fragile rat who relies on good positioning and using his powerful debuff to shred and debilitate priority targets.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            text = text + "< ! > Venom stacks are only soft capped by your attack speed, making it an invaluable stat." + Environment.NewLine + Environment.NewLine;
            text = text + "< ! > Crossbow hits reduce the cooldown of Ambush, rewarding aggressive play and well positioned piercing shots. " + Environment.NewLine + Environment.NewLine;
            text = text + "< ! > Proper usage of Ambush is key to succees, as it's both your only defensive and strongest offensive tool" + Environment.NewLine + Environment.NewLine;
            text = text + "< ! > Try and save Expunge for when venom is stacked high, as you can only use it once per enemy.</color>" + Environment.NewLine + Environment.NewLine;
            LanguageAPI.Add("TWITCH_NAME", "Twitch");
            LanguageAPI.Add("TWITCH_DESCRIPTION", text);
            LanguageAPI.Add("TWITCH_SUBTITLE", "The Plague Rat");
            LanguageAPI.Add("TWITCH_LORE", "\n''They threw this away? But it’s so shiny!''\n\nA plague rat by birth, a connoisseur of filth by passion, Twitch is a paranoid and mutated rat that walks upright and roots through the dregs of the planet for treasures only he truly values. Armed with a chem-powered crossbow, Twitch is not afraid to get his paws dirty as he builds a throne of refuse in his kingdom of filth, endlessly plotting the downfall of humanity.");
            LanguageAPI.Add("TWITCH_OUTRO_FLAVOR", "..and so he left, adorned with a great deal of plundered treasures.");
            SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            survivorDef.cachedName = "TWITCH_NAME";
            survivorDef.descriptionToken = "TWITCH_DESCRIPTION";
            survivorDef.primaryColor = Twitch.characterColor;
            survivorDef.bodyPrefab = Twitch.characterPrefab;
            survivorDef.displayPrefab = characterDisplay;
            survivorDef.desiredSortPosition = 20;
            ContentAddition.AddBody(Twitch.characterPrefab);
            ContentAddition.AddSurvivorDef(survivorDef);
            //ItemDisplaySetup();
            RoR2Application.onLoad += ItemDisplaySetup;
            SkillSetup();
            CreateMaster();
            SkinSetup();
        }

        // Token: 0x06000061 RID: 97 RVA: 0x00006B24 File Offset: 0x00004D24
        private void SkinSetup()
        {
            GameObject gameObject = Twitch.characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel component = gameObject.GetComponent<CharacterModel>();
            ModelSkinController modelSkinController = gameObject.AddComponent<ModelSkinController>();
            SkinnedMeshRenderer fieldValue = Reflection.GetFieldValue<SkinnedMeshRenderer>(component, "mainSkinnedMeshRenderer");
            LanguageAPI.Add("TWITCHBODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("TWITCHBODY_SIMPLE_SKIN_NAME", "Simple");
            LanguageAPI.Add("TWITCHBODY_TAR_SKIN_NAME", "Tarrat");
            LanguageAPI.Add("TWITCHBODY_TUNDRA_SKIN_NAME", "Tundra");
            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(0.22f, 0.27f, 0.2f), new Color(0.74f, 0.65f, 0.52f), new Color(0.2f, 0.16f, 0.16f), new Color(0.1f, 0.14f, 0.13f));
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = fieldValue.sharedMesh
                }
            };
            skinDefInfo.Name = "TWITCHBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "TWITCHBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = component.baseRendererInfos;
            skinDefInfo.RootObject = gameObject;
            skinDefInfo.UnlockableDef = null;
            CharacterModel.RendererInfo[] rendererInfos = skinDefInfo.RendererInfos;
            CharacterModel.RendererInfo[] array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            Material material = array[0].defaultMaterial;
            bool flag = material;
            if (flag)
            {
                material = Instantiate<Material>(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Assets.MainAssetBundle.LoadAsset<Material>("matTwitch").GetColor("_Color"));
                material.SetTexture("_MainTex", Assets.MainAssetBundle.LoadAsset<Material>("matTwitch").GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.black);
                material.SetFloat("_EmPower", 0f);
                material.SetTexture("_EmTex", Assets.MainAssetBundle.LoadAsset<Material>("matTwitch").GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0f);
                array[0].defaultMaterial = material;
            }
            skinDefInfo.RendererInfos = array;
            SkinDef skinDef = LoadoutAPI.CreateNewSkinDef(skinDefInfo);
            LoadoutAPI.SkinDefInfo skinDefInfo2 = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo2.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo2.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo2.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo2.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo2.Icon = LoadoutAPI.CreateSkinIcon(new Color(0.23f, 0.32f, 0.21f), new Color(1f, 1f, 1f), new Color(0.17f, 0.14f, 0.12f), new Color(0.13f, 0.18f, 0.13f));
            skinDefInfo2.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = fieldValue.sharedMesh
                }
            };
            skinDefInfo2.Name = "TWITCHBODY_SIMPLE_SKIN_NAME";
            skinDefInfo2.NameToken = "TWITCHBODY_SIMPLE_SKIN_NAME";
            skinDefInfo2.RendererInfos = component.baseRendererInfos;
            skinDefInfo2.RootObject = gameObject;
            skinDefInfo2.UnlockableDef = SimpleUnlockableDef;
            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            material = array[0].defaultMaterial;
            bool flag2 = material;
            if (flag2)
            {
                material = Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.simpleSkinMat.GetTexture("_MainTex"));
                array[0].defaultMaterial = material;
            }
            skinDefInfo2.RendererInfos = array;
            SkinDef skinDef2 = LoadoutAPI.CreateNewSkinDef(skinDefInfo2);
            LoadoutAPI.SkinDefInfo skinDefInfo3 = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo3.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo3.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo3.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo3.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo3.Icon = LoadoutAPI.CreateSkinIcon(new Color(0.28f, 0.29f, 0.27f), new Color(0.76f, 0.76f, 0.16f), new Color(0.03f, 0.03f, 0.07f), new Color(0.09f, 0.09f, 0.09f));
            skinDefInfo3.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = fieldValue.sharedMesh
                }
            };
            skinDefInfo3.Name = "TWITCHBODY_TAR_SKIN_NAME";
            skinDefInfo3.NameToken = "TWITCHBODY_TAR_SKIN_NAME";
            skinDefInfo3.RendererInfos = component.baseRendererInfos;
            skinDefInfo3.RootObject = gameObject;
            skinDefInfo3.UnlockableDef = TarUnlockableDef;
            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            material = array[0].defaultMaterial;
            bool flag3 = material;
            if (flag3)
            {
                material = Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.tarSkinMat.GetTexture("_MainTex"));
                material.SetTexture("_EmTex", Assets.tarSkinMat.GetTexture("_EmissionMap"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 5f);
                array[0].defaultMaterial = material;
            }
            skinDefInfo3.RendererInfos = array;
            SkinDef skinDef3 = LoadoutAPI.CreateNewSkinDef(skinDefInfo3);
            LoadoutAPI.SkinDefInfo skinDefInfo4 = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo4.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo4.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo4.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo4.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo4.Icon = LoadoutAPI.CreateSkinIcon(new Color(0.88f, 0.88f, 0.88f), new Color(0.53f, 0.5f, 0.64f), new Color(0.22f, 0.18f, 0.28f), new Color(0.22f, 0.2f, 0.19f));
            skinDefInfo4.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = fieldValue.sharedMesh
                }
            };
            skinDefInfo4.Name = "TWITCHBODY_TUNDRA_SKIN_NAME";
            skinDefInfo4.NameToken = "TWITCHBODY_TUNDRA_SKIN_NAME";
            skinDefInfo4.RendererInfos = component.baseRendererInfos;
            skinDefInfo4.RootObject = gameObject;
            skinDefInfo4.UnlockableDef = MasteryUnlockableDef;
            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            material = array[0].defaultMaterial;
            bool flag4 = material;
            if (flag4)
            {
                material = Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.tundraSkinMat.GetTexture("_MainTex"));
                array[0].defaultMaterial = material;
            }
            skinDefInfo4.RendererInfos = array;
            SkinDef skinDef4 = LoadoutAPI.CreateNewSkinDef(skinDefInfo4);
            modelSkinController.skins = new SkinDef[]
            {
                skinDef,
                skinDef4,
                skinDef2,
                skinDef3
            };
        }

        // Token: 0x06000062 RID: 98 RVA: 0x000072C4 File Offset: 0x000054C4
        private void ItemDisplaySetup()
        {
            PopulateFromBody("Commando");
            PopulateFromBody("Croco");

            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Jetpack,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBugWings"),
                            childName = "Spine2",
                            localPos = new Vector3(0f, -12.2f, -23.7f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(12f, 12f, 12f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.GoldGat,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGoldGat"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(50.5f, 5.9f, 0f),
                            localAngles = new Vector3(68f, 90f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.BFG,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBFG"),
                            childName = "Spine2",
                            localPos = new Vector3(20.5f, -3f, 0f),
                            localAngles = new Vector3(0f, 0f, -58f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.CritGlasses,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGlasses"),
                            childName = "Head",
                            localPos = new Vector3(0f, 0f, 27.8f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySyringeCluster"),
                            childName = "Neck",
                            localPos = new Vector3(0f, 17.7f, 0f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Behemoth,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBehemoth"),
                            childName = "Weapon",
                            localPos = new Vector3(78f, 34.4f, 0f),
                            localAngles = new Vector3(-90f, 180f, 90f),
                            localScale = new Vector3(12f, 12f, 12f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Missile,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMissileLauncher"),
                            childName = "R_Clavicle",
                            localPos = new Vector3(-41.8f, -14.4f, 10.9f),
                            localAngles = new Vector3(180f, 0f, 60f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Dagger,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDagger"),
                            childName = "Spine2",
                            localPos = new Vector3(11f, 0f, 6.3f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(75f, 75f, 75f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Hoof,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayHoof"),
                            childName = "L_Knee",
                            localPos = new Vector3(-21.3f, -7.9f, 0.9f),
                            localAngles = new Vector3(-24f, 96f, -2f),
                            localScale = new Vector3(12f, 12f, 6f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ChainLightning,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayUkulele"),
                            childName = "Weapon",
                            localPos = new Vector3(34.1f, 0f, 0f),
                            localAngles = new Vector3(90f, 90f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.GhostOnKill,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMask"),
                            childName = "Head",
                            localPos = new Vector3(0f, 3.6f, 23.6f),
                            localAngles = new Vector3(-36f, 0f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Mushroom,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMushroom"),
                            childName = "Tail5",
                            localPos = new Vector3(11f, 3.6f, 0f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.AttackSpeedOnCrit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWolfPelt"),
                            childName = "Head",
                            localPos = new Vector3(0f, 8.8f, 15.9f),
                            localAngles = new Vector3(12f, 0f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BleedOnHit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayTriTip"),
                            childName = "Neck",
                            localPos = new Vector3(13.5f, 29.9f, 0f),
                            localAngles = new Vector3(52f, -90f, -90f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.WardOnLevel,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWarbanner"),
                            childName = "Spine1",
                            localPos = new Vector3(0f, -4.6f, 27.8f),
                            localAngles = new Vector3(0f, 0f, -90f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.HealOnCrit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayScythe"),
                            childName = "Spine2",
                            localPos = new Vector3(-5.1f, 22.2f, -18.2f),
                            localAngles = new Vector3(-34f, -60f, 48f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.HealWhileSafe,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySnail"),
                            childName = "Tail10",
                            localPos = new Vector3(7f, 0f, -2f),
                            localAngles = new Vector3(0f, 0f, -90f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Clover,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayClover"),
                            childName = "L_Ear",
                            localPos = new Vector3(25.2f, -3.8f, 0f),
                            localAngles = new Vector3(0f, 0f, -90f),
                            localScale = new Vector3(44f, 44f, 44f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BarrierOnOverHeal,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAegis"),
                            childName = "R_Elbow",
                            localPos = new Vector3(-37.5f, -7.2f, 0f),
                            localAngles = new Vector3(0f, -90f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.GoldOnHit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBoneCrown"),
                            childName = "Head",
                            localPos = new Vector3(0f, 6.8f, 8f),
                            localAngles = new Vector3(22f, 0f, 0f),
                            localScale = new Vector3(75f, 75f, 75f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.WarCryOnMultiKill,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayPauldron"),
                            childName = "R_Shoulder",
                            localPos = new Vector3(-9.3f, -7.5f, 0f),
                            localAngles = new Vector3(150f, -90f, 0f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SprintArmor,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBuckler"),
                            childName = "L_Elbow",
                            localPos = new Vector3(14.5f, 11.1f, -5.1f),
                            localAngles = new Vector3(-114f, 22f, 156f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.IceRing,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayIceRing"),
                            childName = "R_Elbow",
                            localPos = new Vector3(-15.5f, -0.2f, 0.7f),
                            localAngles = new Vector3(8f, 90f, -45f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.FireRing,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFireRing"),
                            childName = "R_Elbow",
                            localPos = new Vector3(-18.8f, 0.3f, 0.7f),
                            localAngles = new Vector3(8f, 90f, 0f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.UtilitySkillMagazine,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "L_Shoulder",
                            localPos = new Vector3(1.2f, 2.8f, -15.8f),
                            localAngles = new Vector3(-118f, -1.5f, 12f),
                            localScale = new Vector3(98f, 98f, 98f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "R_Shoulder",
                            localPos = new Vector3(6.6f, 8.2f, 14.7f),
                            localAngles = new Vector3(-218f, -175f, 172f),
                            localScale = new Vector3(98f, 98f, 98f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.JumpBoost,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWaxBird"),
                            childName = "Head",
                            localPos = new Vector3(0.36f, -31f, -5.44f),
                            localAngles = new Vector3(24f, 0f, 0f),
                            localScale = new Vector3(76f, 76f, 76f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ArmorReductionOnHit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWarhammer"),
                            childName = "R_Hand",
                            localPos = new Vector3(-8.4f, 2.8f, -49.4f),
                            localAngles = new Vector3(180f, 0f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.NearbyDamageBonus,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDiamond"),
                            childName = "Weapon",
                            localPos = new Vector3(0f, 0f, 0f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ArmorPlate,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayRepulsionArmorPlate"),
                            childName = "Spine2",
                            localPos = new Vector3(10.3f, -21f, 8f),
                            localAngles = new Vector3(-36f, 25f, 9f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.CommandMissile,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMissileRack"),
                            childName = "Spine1",
                            localPos = new Vector3(5f, 11f, -20f),
                            localAngles = new Vector3(81f, 94f, -78f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Feather,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFeather"),
                            childName = "Spine1",
                            localPos = new Vector3(8.5f, 0f, 0f),
                            localAngles = new Vector3(-90f, -90f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Crowbar,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayCrowbar"),
                            childName = "Neck",
                            localPos = new Vector3(0f, 16.9f, -16.7f),
                            localAngles = new Vector3(0f, 180f, 90f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.FallBoots,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGravBoots"),
                            childName = "R_Foot",
                            localPos = new Vector3(0f, -4.9f, -4.1f),
                            localAngles = new Vector3(14f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGravBoots"),
                            childName = "L_Foot",
                            localPos = new Vector3(1.8f, 5.1f, 3.4f),
                            localAngles = new Vector3(14f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ExecuteLowHealthElite,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGuillotine"),
                            childName = "Neck",
                            localPos = new Vector3(-37f, 1.16f, 7.35f),
                            localAngles = new Vector3(-25f, 67f, 56f),
                            localScale = new Vector3(12f, 12f, 12f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.EquipmentMagazine,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBattery"),
                            childName = "Spine2",
                            localPos = new Vector3(-15f, -2f, -23f),
                            localAngles = new Vector3(-50f, -30f, -118f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.NovaOnHeal,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDevilHorns"),
                            childName = "L_Ear",
                            localPos = new Vector3(-12.2f, -11.3f, -0.7f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDevilHorns"),
                            childName = "R_Ear",
                            localPos = new Vector3(5.8f, 7.5f, 1.1f),
                            localAngles = new Vector3(180f, 388f, -90f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Infusion,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayInfusion"),
                            childName = "Pelvis",
                            localPos = new Vector3(-15.43f, 2.42f, 4.39f),
                            localAngles = new Vector3(-34f, -54f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Medkit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMedkit"),
                            childName = "Spine1",
                            localPos = new Vector3(-21.71f, -9.5f, -8.86f),
                            localAngles = new Vector3(-90f, -180f, 75f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Bandolier,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBandolier"),
                            childName = "Spine1",
                            localPos = new Vector3(-1.8f, 5.2f, -1.2f),
                            localAngles = new Vector3(-39f, 79f, -72f),
                            localScale = new Vector3(75f, 75f, 75f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BounceNearby,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayHook"),
                            childName = "R_Clavicle",
                            localPos = new Vector3(-25.01f, -19.13f, 2.69f),
                            localAngles = new Vector3(38f, -28f, 68f),
                            localScale = new Vector3(0.5f, 0.5f, 0.5f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.IgniteOnKill,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGasoline"),
                            childName = "Spine1",
                            localPos = new Vector3(-10.61f, -10.5f, -23.26f),
                            localAngles = new Vector3(-90f, 0f, -60f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.StunChanceOnHit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayStunGrenade"),
                            childName = "R_Hip",
                            localPos = new Vector3(12.2f, 2.42f, 7.95f),
                            localAngles = new Vector3(0f, -90f, 64f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Firework,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFirework"),
                            childName = "Spine1",
                            localPos = new Vector3(-9f, -2f, -24f),
                            localAngles = new Vector3(-84f, -96f, 104f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.LunarDagger,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayLunarDagger"),
                            childName = "R_Hand",
                            localPos = new Vector3(-19.1f, 2.8f, 22f),
                            localAngles = new Vector3(0f, 198f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Knurl,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayKnurl"),
                            childName = "Spine2",
                            localPos = new Vector3(16.33f, -9.61f, 18.24f),
                            localAngles = new Vector3(24f, -32f, -16f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BeetleGland,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBeetleGland"),
                            childName = "L_Hip",
                            localPos = new Vector3(-15.9f, -12.5f, 0f),
                            localAngles = new Vector3(0f, -16f, -90f),
                            localScale = new Vector3(6f, 6f, 6f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SprintBonus,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySoda"),
                            childName = "Spine1",
                            localPos = new Vector3(-21.65f, -3.88f, -20.38f),
                            localAngles = new Vector3(-50f, -20f, 52f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SecondarySkillMagazine,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDoubleMag"),
                            childName = "Weapon",
                            localPos = new Vector3(10f, 16f, 0f),
                            localAngles = new Vector3(90f, 90f, 0f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.StickyBomb,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayStickyBomb"),
                            childName = "Spine2",
                            localPos = new Vector3(-2.2f, 26.1f, -13f),
                            localAngles = new Vector3(34f, 12f, -50f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.TreasureCache,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayKey"),
                            childName = "R_Hand",
                            localPos = new Vector3(9.05f, -4.67f, -7.79f),
                            localAngles = new Vector3(71f, -16f, 0f),
                            localScale = new Vector3(88f, 88f, 88f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BossDamageBonus,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAPRound"),
                            childName = "Spine1",
                            localPos = new Vector3(-16f, -5.3f, -20.5f),
                            localAngles = new Vector3(-79f, -3f, 49f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SlowOnHit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBauble"),
                            childName = "Tail8",
                            localPos = new Vector3(-1.6f, -21.2f, 29.1f),
                            localAngles = new Vector3(0f, -114f, 18f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ExtraLife,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayHippo"),
                            childName = "L_Coat2",
                            localPos = new Vector3(0f, 3.1f, 0f),
                            localAngles = new Vector3(-90f, 90f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.KillEliteFrenzy,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBrainstalk"),
                            childName = "Head",
                            localPos = new Vector3(0f, 4.9f, 20.3f),
                            localAngles = new Vector3(38f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.RepeatHeal,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayCorpseFlower"),
                            childName = "R_Ear",
                            localPos = new Vector3(-29f, 2.27f, 0f),
                            localAngles = new Vector3(0f, 0f, 90f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.AutoCastEquipment,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFossil"),
                            childName = "R_Coat2",
                            localPos = new Vector3(-8.78f, -3.16f, -12.5f),
                            localAngles = new Vector3(0f, 0f, -86f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.IncreaseHealing,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAntler"),
                            childName = "L_Ear",
                            localPos = new Vector3(-5.9f, 0f, 0f),
                            localAngles = new Vector3(24f, 106f, 6f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAntler"),
                            childName = "R_Ear",
                            localPos = new Vector3(5.9f, 0f, 0f),
                            localAngles = new Vector3(-20f, 284f, 174f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.TitanGoldDuringTP,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGoldHeart"),
                            childName = "Spine2",
                            localPos = new Vector3(3.36f, -8.7f, 24.65f),
                            localAngles = new Vector3(0f, 32f, -90f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SprintWisp,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBrokenMask"),
                            childName = "L_Shoulder",
                            localPos = new Vector3(13.1f, 4.9f, 5.1f),
                            localAngles = new Vector3(-44f, 0f, 90f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BarrierOnKill,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBrooch"),
                            childName = "R_Hand",
                            localPos = new Vector3(1.86f, -9f, 2.83f),
                            localAngles = new Vector3(184f, 88f, 24f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.TPHealingNova,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGlowFlower"),
                            childName = "Tail9",
                            localPos = new Vector3(5f, 2.3f, -1f),
                            localAngles = new Vector3(-90f, 0f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.LunarUtilityReplacement,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBirdFoot"),
                            childName = "R_Hip",
                            localPos = new Vector3(8.9f, 14.7f, 1.5f),
                            localAngles = new Vector3(180f, 0f, 90f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Thorns,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayRazorwireLeft"),
                            childName = "L_Shoulder",
                            localPos = new Vector3(-4.9f, 0f, -3.5f),
                            localAngles = new Vector3(0f, 74f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.LunarPrimaryReplacement,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBirdEye"),
                            childName = "L_Eye",
                            localPos = new Vector3(0f, 0f, 0f),
                            localAngles = new Vector3(90f, 0f, 58f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBirdEye"),
                            childName = "R_Eye",
                            localPos = new Vector3(0f, 0f, 0f),
                            localAngles = new Vector3(-90f, 0f, -58f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.NovaOnLowHealth,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayJellyGuts"),
                            childName = "Tail6",
                            localPos = new Vector3(10.5f, 1.7f, -8.1f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(6f, 6f, 6f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.LunarTrinket,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBeads"),
                            childName = "Tail5",
                            localPos = new Vector3(8.4f, 1.9f, 0f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(100f, 100f, 100f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Plant,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayInterstellarDeskPlant"),
                            childName = "Tail10",
                            localPos = new Vector3(0f, 5.93f, -2.51f),
                            localAngles = new Vector3(-90f, 0f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Bear,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBear"),
                            childName = "R_Coat2",
                            localPos = new Vector3(0f, -3.6f, 0f),
                            localAngles = new Vector3(90f, 90f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.DeathMark,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayDeathMark"),
                            childName = "R_Hand",
                            localPos = new Vector3(-15.27f, 12.34f, 0f),
                            localAngles = new Vector3(0f, 0f, 180f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ExplodeOnDeath,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWilloWisp"),
                            childName = "Spine2",
                            localPos = new Vector3(-0.66f, 20.88f, -23.37f),
                            localAngles = new Vector3(24f, 16f, 34f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Seed,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySeed"),
                            childName = "R_Coat2",
                            localPos = new Vector3(3.55f, -0.79f, 11f),
                            localAngles = new Vector3(-14f, 23f, -62f),
                            localScale = new Vector3(3f, 3f, 3f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.SprintOutOfCombat,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWhip"),
                            childName = "Spine1",
                            localPos = new Vector3(-23.1f, -11f, -8.1f),
                            localAngles = new Vector3(-2f, 164f, 11f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = JunkContent.Items.CooldownOnCrit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySkull"),
                            childName = "L_Hand",
                            localPos = new Vector3(-2.3f, 3.8f, 0f),
                            localAngles = new Vector3(204f, 90f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Phasing,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayStealthkit"),
                            childName = "R_Elbow",
                            localPos = new Vector3(-8.4f, -5.8f, 0f),
                            localAngles = new Vector3(0f, 90f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.PersonalShield,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayShieldGenerator"),
                            childName = "R_Hip",
                            localPos = new Vector3(6.75f, 8f, -3.48f),
                            localAngles = new Vector3(180f, 90f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ShockNearby,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayTeslaCoil"),
                            childName = "R_Elbow",
                            localPos = new Vector3(-8.3f, 0f, 6.49f),
                            localAngles = new Vector3(90f, 0f, 0f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ShieldOnly,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayShieldBug"),
                            childName = "Head",
                            localPos = new Vector3(7.1f, 7.16f, 20.8f),
                            localAngles = new Vector3(0f, -90f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayShieldBug"),
                            childName = "Head",
                            localPos = new Vector3(-6.55f, 7f, 20.8f),
                            localAngles = new Vector3(32f, -90f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.AlienHead,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAlienHead"),
                            childName = "Nose",
                            localPos = new Vector3(9.65f, 8.96f, 0.17f),
                            localAngles = new Vector3(-50f, 90f, 0f),
                            localScale = new Vector3(64f, 64f, 64f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.HeadHunter,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySkullCrown"),
                            childName = "Head",
                            localPos = new Vector3(0f, 9.1f, 18.1f),
                            localAngles = new Vector3(14f, 0f, 0f),
                            localScale = new Vector3(32f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.EnergizedOnEquipmentUse,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWarHorn"),
                            childName = "Head",
                            localPos = new Vector3(1.73f, -7.61f, 34.68f),
                            localAngles = new Vector3(206f, 0f, 0f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.FlatHealth,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySteakCurved"),
                            childName = "Jaw",
                            localPos = new Vector3(11f, -5f, 0.2f),
                            localAngles = new Vector3(-115f, -86f, 85f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Pearl,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayPearl"),
                            childName = "R_Hand",
                            localPos = new Vector3(-46f, 0f, 9.2f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(20f, 20f, 20f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.ShinyPearl,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("ShinyPearl"),
                            childName = "R_Hand",
                            localPos = new Vector3(-46f, 0f, 9.2f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(20f, 20f, 20f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.BonusGoldPackOnKill,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayTome"),
                            childName = "L_Hip",
                            localPos = new Vector3(-7.79f, -6.18f, -6.72f),
                            localAngles = new Vector3(-20f, -12f, 90f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Squid,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySquidTurret"),
                            childName = "Tail7",
                            localPos = new Vector3(11.6f, 6.8f, -0.2f),
                            localAngles = new Vector3(0f, 78f, 0f),
                            localScale = new Vector3(6f, 6f, 6f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Icicle,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFrostRelic"),
                            childName = "Spine2",
                            localPos = new Vector3(-48f, -32f, -88f),
                            localAngles = new Vector3(90f, 0f, 0f),
                            localScale = new Vector3(1f, 1f, 1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.Talisman,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayTalisman"),
                            childName = "Spine2",
                            localPos = new Vector3(48f, -32f, -88f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(1f, 1f, 1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.LaserTurbine,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayLaserTurbine"),
                            childName = "Spine2",
                            localPos = new Vector3(-48f, 32f, -88f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(1f, 1f, 1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Items.FocusConvergence,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFocusedConvergence"),
                            childName = "Spine2",
                            localPos = new Vector3(48f, 32f, -88f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(0.1f, 0.1f, 0.1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = JunkContent.Items.Incubator,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayAncestralIncubator"),
                            childName = "Neck",
                            localPos = new Vector3(0f, 16.2f, -2.4f),
                            localAngles = new Vector3(-12f, 0f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Fruit,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayFruit"),
                            childName = "Spine2",
                            localPos = new Vector3(6f, -1.3f, 24.5f),
                            localAngles = new Vector3(-34f, -84f, 82f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.AffixRed,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteHorn"),
                            childName = "Head",
                            localPos = new Vector3(6.7f, 6.2f, 19.4f),
                            localAngles = new Vector3(56f, 0f, 0f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteHorn"),
                            childName = "Head",
                            localPos = new Vector3(-6.1f, 5.5f, 18.3f),
                            localAngles = new Vector3(46f, 22f, 45f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.AffixBlue,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteRhinoHorn"),
                            childName = "Head",
                            localPos = new Vector3(0f, 1.7f, 31f),
                            localAngles = new Vector3(-45f, 0f, 0f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteRhinoHorn"),
                            childName = "Head",
                            localPos = new Vector3(0f, 7.46f, 22.19f),
                            localAngles = new Vector3(-45f, 0f, 0f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.AffixWhite,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteIceCrown"),
                            childName = "Head",
                            localPos = new Vector3(0f, 12.7f, 20.1f),
                            localAngles = new Vector3(-76f, 0f, 0f),
                            localScale = new Vector3(2f, 2f, 2f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.AffixPoison,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteUrchinCrown"),
                            childName = "Head",
                            localPos = new Vector3(0f, 9f, 19.1f),
                            localAngles = new Vector3(-64f, 0f, 0f),
                            localScale = new Vector3(5f, 5f, 5f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.AffixHaunted,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEliteStealthCrown"),
                            childName = "Head",
                            localPos = new Vector3(0f, 12.6f, 18.8f),
                            localAngles = new Vector3(-75f, 0f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.CritOnUse,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayNeuralImplant"),
                            childName = "Head",
                            localPos = new Vector3(0f, 0f, 48f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(32f, 32f, 32f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.DroneBackup,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayRadio"),
                            childName = "Spine1",
                            localPos = new Vector3(20f, -9.8f, -5.9f),
                            localAngles = new Vector3(0f, 86f, 0f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Lightning,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayLightningArmRight"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(23.8f, -7.7f, 9.6f),
                            localAngles = new Vector3(17f, 22f, -36f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.BurnNearby,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayPotion"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(25f, 7.9f, -3.4f),
                            localAngles = new Vector3(0f, 0f, -90f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.CrippleWard,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEffigy"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(15.6f, 0.8f, -8.8f),
                            localAngles = new Vector3(-18f, 194f, 48f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.QuestVolatileBattery,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayBatteryArray"),
                            childName = "Spine2",
                            localPos = new Vector3(0f, 27f, -16.1f),
                            localAngles = new Vector3(48f, 0f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.GainArmor,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayElephantFigure"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(20f, 9f, -8f),
                            localAngles = new Vector3(18f, 24f, -48f),
                            localScale = new Vector3(48f, 48f, 48f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Recycle,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayRecycler"),
                            childName = "Spine1",
                            localPos = new Vector3(21.25f, -12.14f, -9.25f),
                            localAngles = new Vector3(7f, 5f, 0f),
                            localScale = new Vector3(4f, 4f, 4f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.FireBallDash,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayEgg"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(19.3f, 7f, -5f),
                            localAngles = new Vector3(42f, 0f, 0f),
                            localScale = new Vector3(24f, 24f, 24f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Cleanse,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayWaterPack"),
                            childName = "Spine1",
                            localPos = new Vector3(0f, -15.5f, -24.5f),
                            localAngles = new Vector3(0f, 180f, 0f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Tonic,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayTonic"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(22.66f, 9.83f, -5.04f),
                            localAngles = new Vector3(28f, 121f, 35f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Gateway,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayVase"),
                            childName = "L_Clavicle",
                            localPos = new Vector3(20.5f, 11.7f, -12f),
                            localAngles = new Vector3(-21f, 66f, -48f),
                            localScale = new Vector3(16f, 16f, 16f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Meteor,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayMeteor"),
                            childName = "Spine2",
                            localPos = new Vector3(0f, 10.5f, -128f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(1f, 1f, 1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Saw,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplaySawmerang"),
                            childName = "Spine2",
                            localPos = new Vector3(0f, 10.5f, -80f),
                            localAngles = new Vector3(90f, 0f, 0f),
                            localScale = new Vector3(0.2f, 0.2f, 0.2f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Blackhole,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayGravCube"),
                            childName = "Spine2",
                            localPos = new Vector3(0f, 10.5f, -128f),
                            localAngles = new Vector3(0f, 0f, 0f),
                            localScale = new Vector3(1f, 1f, 1f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2Content.Equipment.Scanner,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = LoadDisplay("DisplayScanner"),
                            childName = "Spine1",
                            localPos = new Vector3(31.7f, -6.92f, -10.78f),
                            localAngles = new Vector3(-35f, -78f, -18f),
                            localScale = new Vector3(8f, 8f, 8f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            var characterModel = Twitch.characterPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>();

            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = "idrs" + "TwitchBody";

            itemDisplayRuleSet.keyAssetRuleGroups = itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();

            characterModel.itemDisplayRuleSet = itemDisplayRuleSet;
        }

        internal static GameObject LoadDisplay(string name)
        {
            if (itemDisplayPrefabs.ContainsKey(name.ToLowerInvariant()))
            {
                if (itemDisplayPrefabs[name.ToLowerInvariant()]) return itemDisplayPrefabs[name.ToLowerInvariant()];
            }

            Debug.LogError("Could not find display prefab " + name);

            return null;
        }

        private static void PopulateFromBody(string bodyName)
        {
            ItemDisplayRuleSet itemDisplayRuleSet = Resources.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName + "Body").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            ItemDisplayRuleSet.KeyAssetRuleGroup[] item = itemDisplayRuleSet.keyAssetRuleGroups;

            for (int i = 0; i < item.Length; i++)
            {
                ItemDisplayRule[] rules = item[i].displayRuleGroup.rules;

                for (int j = 0; j < rules.Length; j++)
                {
                    GameObject followerPrefab = rules[j].followerPrefab;
                    if (followerPrefab)
                    {
                        string name = followerPrefab.name;
                        string key = name?.ToLower();
                        if (!itemDisplayPrefabs.ContainsKey(key))
                        {
                            itemDisplayPrefabs[key] = followerPrefab;
                        }
                    }
                }
            }
        }

        // Token: 0x06000065 RID: 101 RVA: 0x0000D2DC File Offset: 0x0000B4DC
        private void SkillSetup()
        {
            foreach (GenericSkill obj in Twitch.characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                DestroyImmediate(obj);
            }
            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        // Token: 0x06000066 RID: 102 RVA: 0x0000D334 File Offset: 0x0000B534
        private void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(TwitchFireBolt));
            LoadoutAPI.AddSkill(typeof(TwitchFireSMG));
            LoadoutAPI.AddSkill(typeof(TwitchFireShotgun));
            LoadoutAPI.AddSkill(typeof(TwitchChargeBazooka));
            LoadoutAPI.AddSkill(typeof(TwitchFireBazooka));
            LoadoutAPI.AddSkill(typeof(TwitchExpunge));
            LoadoutAPI.AddSkill(typeof(TwitchAmbush));
            LoadoutAPI.AddSkill(typeof(TwitchThrowBomb));
            LoadoutAPI.AddSkill(typeof(TwitchThrowGrenade));
            LoadoutAPI.AddSkill(typeof(TwitchCheese));
            LoadoutAPI.AddSkill(typeof(TwitchScurry));
        }

        // Token: 0x06000067 RID: 103 RVA: 0x0000D3F4 File Offset: 0x0000B5F4
        private void PassiveSetup()
        {
            SkillLocator component = Twitch.characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("TWITCH_PASSIVE_NAME", "Deadly Venom");
            LanguageAPI.Add("TWITCH_PASSIVE_DESCRIPTION", "Certain attacks apply a <style=cIsHealing>stacking venom</style>, lowering <style=cIsUtility>attack speed</style> and <style=cIsUtility>armor</style>.");
            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "TWITCH_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "TWITCH_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        // Token: 0x06000068 RID: 104 RVA: 0x0000D46C File Offset: 0x0000B66C
        private void PrimarySetup()
        {
            SkillLocator component = Twitch.characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("KEYWORD_VENOMOUS", "<style=cKeywordName>Venomous</style><style=cSub>Reduce <style=cIsUtility>attack speed</style> and <style=cIsUtility>armor</style> by a small amount per stack.</style>");
            LanguageAPI.Add("TWITCH_PRIMARY_CROSSBOW_NAME", "Crossbow");
            LanguageAPI.Add("TWITCH_PRIMARY_CROSSBOW_DESCRIPTION", "<style=cIsHealing>Venomous</style>. Fire a tipped arrow, dealing <style=cIsDamage>225% damage</style>. <style=cIsUtility>Reduce Ambush cooldown on hit.</style>");
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchFireBolt));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Any;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon1;
            skillDef.skillDescriptionToken = "TWITCH_PRIMARY_CROSSBOW_DESCRIPTION";
            skillDef.skillName = "TWITCH_PRIMARY_CROSSBOW_NAME";
            skillDef.skillNameToken = "TWITCH_PRIMARY_CROSSBOW_NAME";
            skillDef.keywordTokens = new string[]
            {
                "KEYWORD_VENOMOUS"
            };
            LoadoutAPI.AddSkillDef(skillDef);
            component.primary = Twitch.characterPrefab.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue(component.primary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.primary.skillFamily;
            
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("TWITCH_PRIMARY_SMG_NAME", "Tommy Gun");
            LanguageAPI.Add("TWITCH_PRIMARY_SMG_DESCRIPTION", "<style=cIsHealing>Venomous</style>. Fire a hail of venom-soaked bullets, dealing <style=cIsDamage>3x85% damage</style>.");
            skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchFireSMG));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Any;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon1b;
            skillDef.skillDescriptionToken = "TWITCH_PRIMARY_SMG_DESCRIPTION";
            skillDef.skillName = "TWITCH_PRIMARY_SMG_NAME";
            skillDef.skillNameToken = "TWITCH_PRIMARY_SMG_NAME";
            skillDef.keywordTokens = new string[]
            {
                "KEYWORD_VENOMOUS"
            };
            LoadoutAPI.AddSkillDef(skillDef);
            Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("TWITCH_PRIMARY_SHOTGUN_NAME", "Street Sweeper");
            LanguageAPI.Add("TWITCH_PRIMARY_SHOTGUN_DESCRIPTION", "<style=cIsHealing>Venomous</style>. Fire a close range burst of venom-soaked shells, dealing <style=cIsDamage>4x90% damage</style>.");
            skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchFireShotgun));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Any;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon1c;
            skillDef.skillDescriptionToken = "TWITCH_PRIMARY_SHOTGUN_DESCRIPTION";
            skillDef.skillName = "TWITCH_PRIMARY_SHOTGUN_NAME";
            skillDef.skillNameToken = "TWITCH_PRIMARY_SHOTGUN_NAME";
            skillDef.keywordTokens = new string[]
            {
                "KEYWORD_VENOMOUS"
            };
            LoadoutAPI.AddSkillDef(skillDef);
            Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            bool value = Twitch.boom.Value;
            if (value)
            {
                LanguageAPI.Add("TWITCH_PRIMARY_BAZOOKA_NAME", "Bazooka");
                LanguageAPI.Add("TWITCH_PRIMARY_BAZOOKA_DESCRIPTION", "Charge up and fire a <style=cIsUtility>rocket</style> that deals <style=cIsDamage>50-750% damage</style> based on charge. <style=cIsDamage>Direct hits deal triple damage</style>.");
                skillDef = ScriptableObject.CreateInstance<SkillDef>();
                skillDef.activationState = new SerializableEntityStateType(typeof(TwitchChargeBazooka));
                skillDef.activationStateMachineName = "Weapon";
                skillDef.baseMaxStock = 1;
                skillDef.baseRechargeInterval = 0f;
                skillDef.beginSkillCooldownOnSkillEnd = false;
                skillDef.canceledFromSprinting = false;
                skillDef.fullRestockOnAssign = true;
                skillDef.interruptPriority = InterruptPriority.Any;
                skillDef.isCombatSkill = true;
                skillDef.mustKeyPress = false;
                skillDef.cancelSprintingOnActivation = true;
                skillDef.rechargeStock = 1;
                skillDef.requiredStock = 1;
                skillDef.stockToConsume = 1;
                skillDef.icon = Assets.icon1d;
                skillDef.skillDescriptionToken = "TWITCH_PRIMARY_BAZOOKA_DESCRIPTION";
                skillDef.skillName = "TWITCH_PRIMARY_BAZOOKA_NAME";
                skillDef.skillNameToken = "TWITCH_PRIMARY_BAZOOKA_NAME";
                LoadoutAPI.AddSkillDef(skillDef);
                Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
                skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
                {
                    skillDef = skillDef,
                    viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
                };
            }
        }

        // Token: 0x06000069 RID: 105 RVA: 0x0000DA14 File Offset: 0x0000BC14
        private void SecondarySetup()
        {
            SkillLocator component = Twitch.characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("TWITCH_SECONDARY_CASK_NAME", "Venom Cask");
            LanguageAPI.Add("TWITCH_SECONDARY_CASK_DESCRIPTION", "<style=cIsHealing>Venomous</style>. Hurl a cask of <style=cIsUtility>venom</style> that explodes for <style=cIsDamage>300% damage</style> and leaves a pool of <style=cIsUtility>venom</style>.");
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchThrowBomb));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 8f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon2;
            skillDef.skillDescriptionToken = "TWITCH_SECONDARY_CASK_DESCRIPTION";
            skillDef.skillName = "TWITCH_SECONDARY_CASK_NAME";
            skillDef.skillNameToken = "TWITCH_SECONDARY_CASK_NAME";
            skillDef.keywordTokens = new string[]
            {
                "KEYWORD_VENOMOUS"
            };
            LoadoutAPI.AddSkillDef(skillDef);
            component.secondary = Twitch.characterPrefab.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue(component.secondary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.secondary.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("TWITCH_SECONDARY_FRAG_NAME", "Hand Grenade");
            LanguageAPI.Add("TWITCH_SECONDARY_FRAG_DESCRIPTION", "Throw a grenade that deals <style=cIsDamage>750% damage</style>. <style=cIsUtility>Hold</style> to let the grenade cook before throwing, <style=cIsHealth>but don't hold it for too long</style>!");
            skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchThrowGrenade));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 2;
            skillDef.baseRechargeInterval = 12f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon2b;
            skillDef.skillDescriptionToken = "TWITCH_SECONDARY_FRAG_DESCRIPTION";
            skillDef.skillName = "TWITCH_SECONDARY_FRAG_NAME";
            skillDef.skillNameToken = "TWITCH_SECONDARY_FRAG_NAME";
            LoadoutAPI.AddSkillDef(skillDef);
            Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        // Token: 0x0600006A RID: 106 RVA: 0x0000DCEC File Offset: 0x0000BEEC
        private void UtilitySetup()
        {
            SkillLocator component = Twitch.characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("TWITCH_UTILITY_AMBUSH_NAME", "Ambush");
            LanguageAPI.Add("TWITCH_UTILITY_AMBUSH_DESCRIPTION", "Turn <style=cIsUtility>invisible</style> for 6 seconds. Upon exiting stealth, gain <style=cIsDamage>bonus attack speed</style> and <style=cIsUtility>piercing shots</style> for 5 seconds.");
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchAmbush));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 24f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = false;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = false;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon3;
            skillDef.skillDescriptionToken = "TWITCH_UTILITY_AMBUSH_DESCRIPTION";
            skillDef.skillName = "TWITCH_UTILITY_AMBUSH_NAME";
            skillDef.skillNameToken = "TWITCH_UTILITY_AMBUSH_NAME";
            LoadoutAPI.AddSkillDef(skillDef);
            component.utility = Twitch.characterPrefab.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue(component.utility, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.utility.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            Twitch.ambushDef = skillDef;
            LanguageAPI.Add("TWITCH_UTILITY_AMBUSHACTIVE_NAME", "Spray and Pray");
            LanguageAPI.Add("TWITCH_UTILITY_AMBUSHACTIVE_DESCRIPTION", "Gain <style=cIsDamage>bonus attack speed</style> and <style=cIsUtility>piercing shots</style> for 5 seconds upon exiting stealth.");
            SkillDef skillDef2 = ScriptableObject.CreateInstance<SkillDef>();
            skillDef2.activationState = new SerializableEntityStateType(typeof(TwitchAmbush));
            skillDef2.activationStateMachineName = "Weapon";
            skillDef2.baseMaxStock = 0;
            skillDef2.baseRechargeInterval = 0f;
            skillDef2.beginSkillCooldownOnSkillEnd = false;
            skillDef2.canceledFromSprinting = false;
            skillDef2.fullRestockOnAssign = false;
            skillDef2.interruptPriority = InterruptPriority.Any;
            skillDef2.isCombatSkill = false;
            skillDef2.mustKeyPress = false;
            skillDef2.cancelSprintingOnActivation = false;
            skillDef2.rechargeStock = 0;
            skillDef2.requiredStock = 100;
            skillDef2.stockToConsume = 0;
            skillDef2.icon = Assets.icon3b;
            skillDef2.skillDescriptionToken = "TWITCH_UTILITY_AMBUSHACTIVE_DESCRIPTION";
            skillDef2.skillName = "TWITCH_UTILITY_AMBUSHACTIVE_NAME";
            skillDef2.skillNameToken = "TWITCH_UTILITY_AMBUSHACTIVE_NAME";
            LoadoutAPI.AddSkillDef(skillDef2);
            Twitch.ambushActiveDef = skillDef2;
            LanguageAPI.Add("TWITCH_UTILITY_SCURRY_NAME", "Scurry");
            LanguageAPI.Add("TWITCH_UTILITY_SCURRY_DESCRIPTION", "Turn <style=cIsUtility>invisible</style> and <style=cIsUtility>dash</style> a short distance. Gain <style=cIsDamage>bonus attack speed</style> and <style=cIsUtility>piercing shots</style> for 4 seconds.");
            SkillDef skillDef3 = ScriptableObject.CreateInstance<SkillDef>();
            skillDef3.activationState = new SerializableEntityStateType(typeof(TwitchScurry));
            skillDef3.activationStateMachineName = "Body";
            skillDef3.baseMaxStock = 1;
            skillDef3.baseRechargeInterval = 6f;
            skillDef3.beginSkillCooldownOnSkillEnd = false;
            skillDef3.canceledFromSprinting = false;
            skillDef3.fullRestockOnAssign = true;
            skillDef3.interruptPriority = InterruptPriority.Skill;
            skillDef3.isCombatSkill = false;
            skillDef3.mustKeyPress = false;
            skillDef3.cancelSprintingOnActivation = false;
            skillDef3.rechargeStock = 1;
            skillDef3.requiredStock = 1;
            skillDef3.stockToConsume = 1;
            skillDef3.icon = Assets.icon3c;
            skillDef3.skillDescriptionToken = "TWITCH_UTILITY_SCURRY_DESCRIPTION";
            skillDef3.skillName = "TWITCH_UTILITY_SCURRY_NAME";
            skillDef3.skillNameToken = "TWITCH_UTILITY_SCURRY_NAME";
            LoadoutAPI.AddSkillDef(skillDef3);
            Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef3,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("TWITCH_UTILITY_CHEESE_NAME", "Cheese");
            LanguageAPI.Add("TWITCH_UTILITY_CHEESE_DESCRIPTION", "Relax and have some <style=cIsUtility>cheese</style>. <style=cIsHealing>Restores 100% of your max health</style>.");
            SkillDef skillDef4 = ScriptableObject.CreateInstance<SkillDef>();
            skillDef4.activationState = new SerializableEntityStateType(typeof(TwitchCheese));
            skillDef4.activationStateMachineName = "Body";
            skillDef4.baseMaxStock = 1;
            skillDef4.baseRechargeInterval = 32f;
            skillDef4.beginSkillCooldownOnSkillEnd = false;
            skillDef4.canceledFromSprinting = true;
            skillDef4.fullRestockOnAssign = true;
            skillDef4.interruptPriority = InterruptPriority.Skill;
            skillDef4.isCombatSkill = false;
            skillDef4.mustKeyPress = false;
            skillDef4.cancelSprintingOnActivation = true;
            skillDef4.rechargeStock = 1;
            skillDef4.requiredStock = 1;
            skillDef4.stockToConsume = 1;
            skillDef4.icon = Assets.icon3d;
            skillDef4.skillDescriptionToken = "TWITCH_UTILITY_CHEESE_DESCRIPTION";
            skillDef4.skillName = "TWITCH_UTILITY_CHEESE_NAME";
            skillDef4.skillNameToken = "TWITCH_UTILITY_CHEESE_NAME";
            LoadoutAPI.AddSkillDef(skillDef4);
            Array.Resize(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef4,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        // Token: 0x0600006B RID: 107 RVA: 0x0000E228 File Offset: 0x0000C428
        private void SpecialSetup()
        {
            SkillLocator component = Twitch.characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("KEYWORD_INFECTION", "<style=cKeywordName>Infectious</style><style=cSub>Reduce <style=cIsUtility>attack speed</style> and <style=cIsUtility>armor</style> by a large amount per stack <style=cIsHealth>permanently</style>.");
            LanguageAPI.Add("TWITCH_SPECIAL_EXPUNGE_NAME", "Expunge");
            LanguageAPI.Add("TWITCH_SPECIAL_EXPUNGE_DESCRIPTION", "<style=cIsDamage>Infectious</style>. Throw an <style=cIsUtility>infected knife</style> that deals <style=cIsDamage>400(+70 per venom stack)% damage</style>. Target becomes <style=cIsHealth>immune to venom</style>.");
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.activationState = new SerializableEntityStateType(typeof(TwitchExpunge));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 8f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.PrioritySkill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.icon4;
            skillDef.skillDescriptionToken = "TWITCH_SPECIAL_EXPUNGE_DESCRIPTION";
            skillDef.skillName = "TWITCH_SPECIAL_EXPUNGE_NAME";
            skillDef.skillNameToken = "TWITCH_SPECIAL_EXPUNGE_NAME";
            skillDef.keywordTokens = new string[]
            {
                "KEYWORD_INFECTION"
            };
            LoadoutAPI.AddSkillDef(skillDef);
            component.special = Twitch.characterPrefab.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue(component.special, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.special.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        // Token: 0x0600006C RID: 108 RVA: 0x0000E3D0 File Offset: 0x0000C5D0
        private void CreateMaster()
        {
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Charactermasters/CommandoMonsterMaster"), "TwitchMonsterMaster", true, "C:\\Users\\rseid\\Documents\\ror2mods\\Twitch\\Twitch\\Twitch.cs", "CreateMaster", 4097);
            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };
            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = Twitch.characterPrefab;
        }

        // Token: 0x04000075 RID: 117
        public const string MODUID = "com.rob.Twitch";

        // Token: 0x04000076 RID: 118
        public static GameObject characterPrefab;

        // Token: 0x04000077 RID: 119
        public GameObject characterDisplay;

        // Token: 0x04000078 RID: 120
        public GameObject doppelganger;

        // Token: 0x04000079 RID: 121
        public static SkillDef ambushDef;

        // Token: 0x0400007A RID: 122
        public static SkillDef ambushActiveDef;

        // Token: 0x0400007B RID: 123
        public static SkillDef ambushRecastDef;

        // Token: 0x0400007C RID: 124
        public static GameObject boltProjectile;

        // Token: 0x0400007D RID: 125
        public static GameObject bulletProjectile;

        // Token: 0x0400007E RID: 126
        public static GameObject expungeProjectile;

        // Token: 0x0400007F RID: 127
        public static GameObject caskProjectile;

        // Token: 0x04000080 RID: 128
        public static GameObject venomPool;

        // Token: 0x04000081 RID: 129
        public static GameObject bazookaProjectile;

        // Token: 0x04000082 RID: 130
        public static GameObject grenadeProjectile;

        // Token: 0x04000083 RID: 131
        public static GameObject laserTracer;

        public static readonly Color characterColor = new Color(0.16f, 0.34f, 0.04f);
        public static readonly Color laserTracerColor = new Color(1.6f, 3.4f, 0.4f);

        private static readonly Color poisonColor = new Color(0.36f, 0.54f, 0.24f);

        private static Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();

        internal ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        public static ConfigEntry<bool> how;

        private static ConfigEntry<bool> boom;

        internal static BuffDef venomDebuff;
        internal static BuffDef ambushBuff;
        internal static BuffDef expungedDebuff;

        public UnlockableDef MasteryUnlockableDef { get; private set; }
        public UnlockableDef TarUnlockableDef { get; private set; }
        public UnlockableDef SimpleUnlockableDef { get; private set; }
        public static Material LaserTracerMaterial { get; private set; }
    }
}
