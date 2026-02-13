using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Rust;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Plugins;
using UnityEngine.UI;
using CompanionServer;
using ConVar;

namespace Oxide.Plugins {
    [Info("Buildarin - the builder's plugin", "Daladirn", "0.1.0")]
    [Description("A plugin to supply you everything you might need in day to day building")]
    class Buildarin : RustPlugin {
        public static Buildarin Instance;
        private List<int> blueprints = new List<int>();
        private Config config = new Config();

        [PluginReference] private Plugin ImageLibrary;

        private void Init() {
            Puts("A Buildaring baby plugin is born!");
        }

        class Config {
            public class ItemStack {
                public string Name {get;}
                public int Count {get;}

                public ItemStack(string name, int count) {
                    Name = name;
                    Count = count;
                }
            }

            public List<ItemStack> craftItemList {get;} = new List<ItemStack>{
                new ItemStack("propanetank", 100000), 
                new ItemStack("gears", 100000), 
                new ItemStack("metalpipe", 100000),
                new ItemStack("metalspring", 100000), 
                new ItemStack("metalblade", 100000), 
                new ItemStack("riflebody", 100000), 
                new ItemStack("roadsigns", 100000),
                new ItemStack("rope", 100000), 
                new ItemStack("semibody", 100000), 
                new ItemStack("sewingkit", 100000),
                new ItemStack("smgbody", 100000),
                new ItemStack("tarp", 100000), 
                new ItemStack("techparts", 100000), 
                new ItemStack("sheetmetal", 100000),
                new ItemStack("targeting.computer", 100000),
                new ItemStack("cctv.camera", 100000),
                new ItemStack("scrap", 100000),
                new ItemStack("cloth", 100000),
                new ItemStack("leather", 100000),
                new ItemStack("fat.animal", 100000),
                new ItemStack("bone.fragments", 100000),
                new ItemStack("lowgradefuel", 100000),
                new ItemStack("gunpowder", 100000),
                new ItemStack("charcoal", 100000),
                new ItemStack("sulfur", 100000),
                new ItemStack("explosives", 100000),
                new ItemStack("can.tuna.empty", 100000),
                new ItemStack("stash.small", 100000),
                new ItemStack("grenade.beancan", 100000),
                new ItemStack("syringe.medical", 100000),
                new ItemStack("spear.wooden", 100000),
                new ItemStack("electric.rf.broadcaster", 100000),
                new ItemStack("electric.rf.receiver", 100000),
                new ItemStack("ladder.wooden.wall", 100000),
                new ItemStack("wood", 100000),
                new ItemStack("stones", 100000),
                new ItemStack("metal.fragments", 100000),
                new ItemStack("metal.refined", 100000),
            };
            public List<ItemStack> baseBeltItems {get;} = new List<ItemStack>{
                new ItemStack("building.planner", 1),
                new ItemStack("hammer", 1),
            };
        }

        public static Dictionary<int, List<BlockInfo>> BuildingImages = new Dictionary<int, List<BlockInfo>> {
            [0]  = new List<BlockInfo> { },
            [1] = new List<BlockInfo> {
                new("Wood", "https://i.ibb.co/ZCM6JnN/Wood-Default.png", 0),
                new("Frontier", "https://i.ibb.co/nqc19R5z/Wood-Frontier.png", 10232),
                new("Gingerbread", "https://i.ibb.co/Wp5QK9Q8/Wood-Gingerbread.png", 2),
            },
            [2] = new List<BlockInfo> {
                new("Stone", "https://i.ibb.co/8nXJcF8S/Stone-Default.png", 0),
                new("Adobe", "https://i.ibb.co/C5NMMFXV/Stone-Adobe.png", 10220),
                new("Brick", "https://i.ibb.co/chQYYfPJ/Stone-Brick.png", 10223),
                new("Brutalist", "https://i.ibb.co/kV4ZV5dV/Stone-Burtalist.png", 10225),
            },
            [3] = new List<BlockInfo> {
                new("Metal", "https://i.ibb.co/KzNn20KX/Metal-Default.png", 0),
                new("Container", "https://i.ibb.co/1tCxxvVB/Metal-Container.png", 10221, new List<SkinColor> {
                    new("0.863 0.863 0.863 1", 6),
                    new("0.813 0.457 0.133 1", 5),
                    new("0.414 0.164 0.109 1", 4),
                    new("0.566 0.285 0.828 1", 3),
                    new("0.449 0.711 0.344 1", 2),
                    new("0.375 0.555 0.738 1", 1),
                    new("0.656 0.605 0.559 1", 16),
                    new("0.207 0.336 0.371 1", 15),
                    new("0.336 0.324 0.309 1", 14),
                    new("0.836 0.66 0.219 1", 13),
                    new("0.773 0.527 0.387 1", 12),
                    new("0.723 0.293 0.18 1", 11),
                    new("0.238 0.344 0.195 1", 10),
                    new("0.195 0.219 0.332 1", 9),
                    new("0.398 0.332 0.277 1", 8),
                    new("0.195 0.195 0.18 1", 7),
                }),
            },
            [4] = new List<BlockInfo> {
                new("TopTier", "https://i.ibb.co/ynSywVn5/HQM-Default.png", 0),
                new("SpaceStation", "https://i.ibb.co/BHxqshkx/HQM-Space-Station.png", 10430),
            }
        };

        public class BlockInfo {
            public string Title;
            public string Url;
            public ulong SkinId;
            public List<SkinColor> Colors;

            public BlockInfo(string title, string url, ulong skinId) {
                Title = title;
                Url = url;
                SkinId = skinId;
            }

            public BlockInfo(string title, string url, ulong skinId, List<SkinColor> colors) {
                Title = title;
                Url = url;
                SkinId = skinId;
                Colors = colors;
            }
        };

        public class SkinColor {
            public string RGBA;
            public uint ColorId;

            public SkinColor(string rgba, uint colorId) {
                RGBA = rgba;
                ColorId = colorId;
            }
        };

        public class CustomPlayer : FacepunchBehaviour {
            public static Dictionary<BasePlayer, CustomPlayer> Players = new Dictionary<BasePlayer, CustomPlayer>();

            public BasePlayer BasePlayer {get; set;}
            public Ui Ui {get; set;}
            public bool IsAdmin {get; set;}
            public bool IsGod {get; set;}
            public bool IsDurability {get; set;}
            public bool IsAmmoInfinite {get; set;}
            public bool IsStability {get; set;}
            public bool IsCrosshair {get; set;}
            public int BuildingGrade {get; set;}
            public Dictionary<int, ulong> BuildingSkins;
            public Dictionary<ulong, uint> BuildingSkinColors;

            public CustomPlayer(Plugin imageLibrary, BasePlayer basePlayer) {
                BasePlayer = basePlayer;
                Ui = new Ui(imageLibrary, this);
                Ui.RenderGradeUi();
                IsAdmin = basePlayer.IsAdmin;
                IsGod = true;
                IsDurability = false;
                IsAmmoInfinite = true;
                IsStability = true;
                IsCrosshair = false;
                BuildingGrade = 0;
                BuildingSkins = new Dictionary<int, ulong> {
                    {0, 0},
                    {1, 0},
                    {2, 0},
                    {3, 0},
                    {4, 0},
                };

                BuildingSkinColors = new Dictionary<ulong, uint> { };
                foreach (var list in BuildingImages) {
                    foreach (var skin in list.Value.Where(x => x.Colors != null)) {
                        BuildingSkinColors.Add(skin.SkinId, skin.Colors[0].ColorId);
                    }
                }
            }

            public static bool TryGetPlayer(BasePlayer basePlayer, out CustomPlayer customPlayer) {
                return Players.TryGetValue(basePlayer, out customPlayer);
            }

            public static bool HasPlayer(BasePlayer basePlayer) {
                if (basePlayer == null) return false;
                return Players.ContainsKey(basePlayer);
            }

            public void SetBuildingGradeAndUpdateUi(int newGrade) {
                BuildingGrade = newGrade;
                Ui.RenderGradeUi();
                if (Ui.OpenPanels.Contains(Ui.PanelNames.MainMenu)) Ui.RenderMainMenuUi();
            }

            public void SetBuildSkinAndUpdateUi(int grade, ulong skin) {
                BuildingSkins[grade] = skin;
                if (Ui.OpenPanels.Contains(Ui.PanelNames.MainMenu)) Ui.RenderMainMenuUi();
            }

            public void SetBuildColorAndUpdateUi(ulong skin, uint color) {
                BuildingSkinColors[skin] = color;
                if (Ui.OpenPanels.Contains(Ui.PanelNames.MainMenu)) Ui.RenderMainMenuUi();
            }

            public void Destroy() {
                Players.Remove(BasePlayer);
                Destroy(this);
            }
        }

        #region Global Hooks

        private void OnPlayerConnected(BasePlayer player) {
            if (player == null) return;

            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) {
                customPlayer = new CustomPlayer(ImageLibrary, player);
                CustomPlayer.Players[player] = customPlayer;
            }
            
            timer.Repeat(600, 0, () =>
            {
                foreach (var customPlayer in CustomPlayer.Players)
                {
                    RefreshItems(customPlayer.Key);
                    SetupUserBaseItems(player);
                }
            });

            timer.Once(5f, () => UpdateItems(player));

            UnlockAllBPs(player);
            DisableWorkbenchRequirements(player);
            RefillStats(customPlayer);
        }

        private void OnPlayerDisconnected(BasePlayer player) {
            if (player == null) return;

            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            customPlayer.Destroy();
        }
        
        private void OnPlayerRespawned(BasePlayer player) { 
            if (player == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            NextTick(() => {
                RefillStats(customPlayer);
                UpdateItems(player);
                SetupUserBaseItems(player);
            });
        }
        
        private void OnServerInitialized() {
            foreach (BasePlayer player in BasePlayer.activePlayerList) {
                OnPlayerConnected(player);
            }

            InitImageLibrary();
            SetupBlueprints();
            SetupQuickDespawn();
            SetupBedLimit();
        }

        private void InitImageLibrary() {
            if (ImageLibrary == null) {
                PrintError("[ImageLibrary] not found!");
                return;
            }

            foreach (var list in BuildingImages) {
                foreach (var info in list.Value.Where(x => !string.IsNullOrEmpty(x.Url))) {
                    ImageLibrary.Call("AddImage", info.Url, info.Title);
                }
            }
            ImageLibrary.Call("AddImage", "https://i.ibb.co/96Y3bhS/Circle.png", "Circle");
        }
        
        private void OnPlayerInput(BasePlayer player, InputState input) {  
            if (player == null || input == null) return;

            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            CheckLazer(customPlayer, input);
            CheckMenuInputAndToggle(customPlayer, input);
            
        }
        
        private void OnEntityBuilt(Planner plan, GameObject gameObject) {
            if (plan == null || gameObject == null) return;

            var player = plan?.GetOwnerPlayer();
            if (player == null) return;

            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            UpdateBuildingGrade(customPlayer, gameObject);
            HandleBuiltEntityStability(customPlayer, gameObject);
        }

        private void OnServerCommand(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.cmd.FullName == "inventory.lighttoggle") RotatePlayerBuildingGrade(customPlayer);
        }

        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info) {
            if (IsDecay(info) || IsPreventedByGodMode(entity, info)) return false;
            return null;
        }

        private void OnPlayerMetabolize(PlayerMetabolism metabolism, BaseCombatEntity entity, float delta) {
            CustomPlayer customPlayer;
            if (entity is BasePlayer && CustomPlayer.TryGetPlayer((BasePlayer)entity, out customPlayer)) HandleGodlyMetabolism(customPlayer, metabolism);
        }

        // Prevents paying for deployables
        object OnPayForPlacement(BasePlayer player, Planner planner, Construction construction) {
            if (CustomPlayer.HasPlayer(player)) return false;
            return null;
        }

        private void OnEntitySpawned(BaseNetworkable entity) {
            HandleCorpse(entity);
        }

        private void OnItemCraftCancelled(ItemCraftTask task) {
            CancelCraftResourceRefund(task);
        }

        private void OnLoseCondition(Item item, ref float amount)
        {
            if (item == null) return;
            var player = item.GetOwnerPlayer() ?? item.GetRootContainer()?.GetOwnerPlayer();
            if (player == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            if (!customPlayer.IsDurability) {
                amount = 0;
                item.condition = item.maxCondition;
            }
        }

        private void OnWeaponFired(BaseProjectile projectile, BasePlayer player) {
            if (player == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            HandleInfiniteAmmo(customPlayer, projectile);
        }

        private void OnRocketLaunched(BasePlayer player) {
            if (player == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            HandleInfiniteRockets(customPlayer);
        }

        private void OnMeleeThrown(BasePlayer player, Item item) {
            if (player == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(player, out customPlayer)) return;

            HandleInfiniteThrownWeapons(customPlayer, item);
        }

        #endregion

        #region Console Commands

        [ConsoleCommand("buildarin.menu")]
        private void CommandMenu(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.Args.Length > 0) {
                switch (arg.Args[0]) {
                    case "main": 
                        customPlayer.Ui.RenderMainMenuUi();
                        break;
                }
            }
        }
        
        [ConsoleCommand("buildarin.grade")]
        private void CommmandGrade(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.Args.Length > 0) {
                switch (arg.Args[0]) {
                    case "twig": 
                        customPlayer.SetBuildingGradeAndUpdateUi(0);
                        break;
                    case "wood": 
                        customPlayer.SetBuildingGradeAndUpdateUi(1);
                        break;
                    case "stone": 
                        customPlayer.SetBuildingGradeAndUpdateUi(2);
                        break;
                    case "metal": 
                        customPlayer.SetBuildingGradeAndUpdateUi(3);
                        break;
                    case "hqm": 
                        customPlayer.SetBuildingGradeAndUpdateUi(4);
                        break;
                }
            }
        }

        [ConsoleCommand("buildarin.skin")]
        private void CommmandSkin(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.Args.Length > 0) {
                if (ulong.TryParse(arg.Args[0], out ulong skinId)) {
                    customPlayer.SetBuildSkinAndUpdateUi(customPlayer.BuildingGrade, skinId);
                }
            }
        }

        [ConsoleCommand("buildarin.color")]
        private void CommmandBuildColor(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.Args.Length > 0) {
                if (uint.TryParse(arg.Args[0], out uint colorId)) {
                    customPlayer.SetBuildColorAndUpdateUi(customPlayer.BuildingSkins[customPlayer.BuildingGrade], colorId);
                }
            }
        }

        [ConsoleCommand("buildarin.crosshair")]
        private void CommandCrosshair(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsCrosshair = !customPlayer.IsCrosshair;
            if (customPlayer.IsCrosshair) {
                customPlayer.Ui.RenderCrosshairUi();
            } else {
                customPlayer.Ui.RemoveCrosshairUi();
            }
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.noclip")]
        private void CommandNoclip(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            ConsoleNetwork.SendClientCommand(customPlayer.BasePlayer.net.connection, "noclip");
            timer.Once(0.075f, customPlayer.Ui.RenderMainMenuUi);
        }

        [ConsoleCommand("buildarin.godmode")]
        private void CommandGodMode(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsGod = !customPlayer.IsGod;
            if (customPlayer.IsGod) RefillStats(customPlayer);
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.durability")]
        private void CommandDurability(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsDurability = !customPlayer.IsDurability;
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.infiniteammo")]
        private void CommandInfiniteAmmo(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsAmmoInfinite = !customPlayer.IsAmmoInfinite;
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.downgrade")]
        private void CommandDowngrade(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            DowngradeBuildingGrade(customPlayer);
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.upgrade")]
        private void CommandUpgrade(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            UpgradeBuildingGrade(customPlayer);
            customPlayer.Ui.RenderMainMenuUi();
        }

        [ConsoleCommand("buildarin.stability")]
        private void CommandStability(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsStability = !customPlayer.IsStability;
            customPlayer.Ui.RenderMainMenuUi();
        }

        #endregion
        
        #region Remove Dead Bodies Methods 

        private void HandleCorpse(BaseNetworkable entity) {
            if (entity is PlayerCorpse || entity is DroppedItemContainer) entity.Invoke(() => entity.Kill(BaseNetworkable.DestroyMode.None), 0.1f);
        }

        #endregion

        #region Infinite Resources Methods
        
        private void CancelCraftResourceRefund(ItemCraftTask task) {
            HashSet<string> itemNameSet = config.craftItemList.Select(item => item.Name).ToHashSet();
            foreach (var takenItem in task.takenItems) {
                if (itemNameSet.Contains(takenItem.info.shortname)) {
                    timer.Once(0.01f, () => {
                        if (takenItem != null) {
                            takenItem.RemoveFromContainer();
                            takenItem.Remove();
                        }
                    });
                }
            }
        }

        #endregion

        #region Stability Methods

        private void HandleBuiltEntityStability(CustomPlayer customPlayer, GameObject gameObject) {
            if (customPlayer.IsStability) return;
            var buildingBlock = gameObject.GetComponent<BuildingBlock>();
            if (buildingBlock == null || buildingBlock.OwnerID == 0) return;
            buildingBlock.grounded = true;
        }

        #endregion

        #region GodMode Methods

        private bool IsPreventedByGodMode(BaseCombatEntity entity, HitInfo info) {
            CustomPlayer customPlayer;
            return entity is BasePlayer && CustomPlayer.TryGetPlayer((BasePlayer)entity, out customPlayer) && customPlayer.IsGod && !info.damageTypes.Has(DamageType.Suicide);
        }

        private void HandleGodlyMetabolism(CustomPlayer customPlayer, PlayerMetabolism metabolism) {
            if (customPlayer.IsGod) {
                metabolism.hydration.SetValue(250);
                metabolism.calories.SetValue(500);
                metabolism.temperature.Set(20);
                metabolism.wetness.SetValue(0);
                metabolism.bleeding.SetValue(0);
                metabolism.oxygen.SetValue(3);
                metabolism.radiation_poison.SetValue(0);
                metabolism.radiation_level.SetValue(0);
            }
        }

        #endregion

        #region Infinite Ammo Methods

        private void HandleInfiniteAmmo(CustomPlayer customPlayer, BaseProjectile projectile) {
            if (!customPlayer.IsAmmoInfinite) return;

            var heldEntity = projectile.GetItem();
            heldEntity.condition = heldEntity.info.condition.max;

            if (projectile.primaryMagazine.contents > 0) return;

            projectile.primaryMagazine.contents = projectile.primaryMagazine.capacity;
            projectile.SendNetworkUpdateImmediate();
        }

        private void HandleInfiniteRockets(CustomPlayer customPlayer) {
            if (!customPlayer.IsAmmoInfinite) return;

            var heldEntity = customPlayer.BasePlayer.GetActiveItem();
            if (heldEntity == null) return;
//            heldEntity.condition = heldEntity.info.condition.max;

            var weapon = heldEntity.GetHeldEntity() as BaseProjectile;
            if (weapon == null || weapon.primaryMagazine.contents > 0) return;

            weapon.primaryMagazine.contents = weapon.primaryMagazine.capacity;
            weapon.SendNetworkUpdateImmediate();
        }

        private void HandleInfiniteThrownWeapons(CustomPlayer customPlayer, Item item) {
            if (!customPlayer.IsAmmoInfinite) return;

            var newMelee = ItemManager.CreateByItemID(item.info.itemid, item.amount, item.skin);
            newMelee._condition = item._condition;

            customPlayer.BasePlayer.GiveItem(newMelee, BaseEntity.GiveItemReason.PickedUp);
        }

        #endregion

        #region Decay Methods

        private bool IsDecay(HitInfo info) {
            return info.damageTypes.Has(DamageType.Decay);
        }

        #endregion

        #region Building Grade Methods

        void UpdateBuildingGrade(CustomPlayer customPlayer, GameObject gameObject) {
            var buildingBlock = gameObject.GetComponent<BuildingBlock>();
            if (buildingBlock == null) return;

            var skin = customPlayer.BuildingSkins[customPlayer.BuildingGrade];
            buildingBlock.skinID = skin;
            buildingBlock.SetGrade((BuildingGrade.Enum)customPlayer.BuildingGrade);
            buildingBlock.SetHealthToMax();
            buildingBlock.StartBeingRotatable();
            buildingBlock.SendNetworkUpdate();
            buildingBlock.UpdateSkin();
            if (customPlayer.BuildingSkinColors.TryGetValue(skin, out uint color)) {
                buildingBlock.SetCustomColour(Convert.ToUInt32(color));
            }
            buildingBlock.ResetUpkeepTime();
            buildingBlock.GetBuilding()?.Dirty();
        }

        void RotatePlayerBuildingGrade(CustomPlayer customPlayer) {
            customPlayer.SetBuildingGradeAndUpdateUi((customPlayer.BuildingGrade + 1) % 5);
        }

        void UpgradeBuildingGrade(CustomPlayer customPlayer) {
            RaycastHit hit;
            if (!UnityEngine.Physics.Raycast(customPlayer.BasePlayer.eyes.HeadRay(), out hit, 100f, LayerMask.GetMask("Construction"))) return;

            var entity = hit.GetEntity();

            if (entity is BuildingBlock buildingBlock) {
                if (buildingBlock == null) return;
                var grade = buildingBlock.grade;

                var newGrade = grade == BuildingGrade.Enum.TopTier ? grade : grade + 1;
                var newSkin = customPlayer.BuildingSkins[(int)newGrade];
                buildingBlock.skinID = newSkin;
                buildingBlock.SetGrade(newGrade);
                buildingBlock.SetHealthToMax();
                buildingBlock.StartBeingRotatable();
                buildingBlock.SendNetworkUpdate();
                buildingBlock.UpdateSkin();
                if (customPlayer.BuildingSkinColors.TryGetValue(newSkin, out uint newColor)) {
                    buildingBlock.SetCustomColour(Convert.ToUInt32(newColor));
                }
                buildingBlock.ResetUpkeepTime();
                buildingBlock.GetBuilding()?.Dirty();
            }
        }

        void DowngradeBuildingGrade(CustomPlayer customPlayer) {
            RaycastHit hit;
            if (!UnityEngine.Physics.Raycast(customPlayer.BasePlayer.eyes.HeadRay(), out hit, 100f, LayerMask.GetMask("Construction"))) return;

            var entity = hit.GetEntity();

            if (entity is BuildingBlock buildingBlock) {
                if (buildingBlock == null) return;
                var grade = buildingBlock.grade;
                if (grade == BuildingGrade.Enum.Twigs) return;


                var newGrade = grade == BuildingGrade.Enum.Twigs ? grade : grade - 1;
                var newSkin = customPlayer.BuildingSkins[(int)newGrade];
                buildingBlock.skinID = newSkin;
                buildingBlock.SetGrade(newGrade);
                buildingBlock.SetHealthToMax();
                buildingBlock.StartBeingRotatable();
                buildingBlock.SendNetworkUpdate();
                buildingBlock.UpdateSkin();
                if (customPlayer.BuildingSkinColors.TryGetValue(newSkin, out uint newColor)) {
                    buildingBlock.SetCustomColour(Convert.ToUInt32(newColor));
                }
                buildingBlock.ResetUpkeepTime();
                buildingBlock.GetBuilding()?.Dirty();
            }
        }

        #endregion

        #region Infinite Resources Methods

        private void RefreshItems(BasePlayer player) {
            for (var i = 0; i < config.craftItemList.Count; i++) {
                Item item = player.inventory.containerMain.GetSlot(24 + i);
                if (item == null) continue;
                item.RemoveFromContainer();
                item.Remove();
            }
            UpdateItems(player);
        }

        private void UpdateItems(BasePlayer player) {
            player.inventory.containerMain.capacity = 24 + config.craftItemList.Count;
            for (var i = 0; i < config.craftItemList.Count; i++) {
                var item = ItemManager.CreateByName(config.craftItemList[i].Name, config.craftItemList[i].Count);
                if (item == null) continue;
                if (!item.MoveToContainer(player.inventory.containerMain, 24 + i, true, true)) {
                    item.Remove();
                }
            }
        }

        #endregion

        #region Unlock Blooprints Methods

        private void UnlockAllBPs(BasePlayer player) {
            var PersistantPlayerInfo = player.PersistantPlayerInfo;
            foreach (var blueprint in blueprints) {
                if (PersistantPlayerInfo.unlockedItems.Contains(blueprint)) continue;
                PersistantPlayerInfo.unlockedItems.Add(blueprint);
            }
            
            player.PersistantPlayerInfo = PersistantPlayerInfo;
            player.SendNetworkUpdateImmediate();
            player.ClientRPCPlayer(null, player, "UnlockedBlueprint", 0);
        }

        #endregion

        #region No Workbench Required/Instant Craft Methods

        private void DisableWorkbenchRequirements(BasePlayer player) {
            player.ClientRPCPlayer(null, player, "craftMode", 1);
        }

        private void SetupBlueprints() {
            foreach (ItemBlueprint bp in ItemManager.GetBlueprints()) {
                blueprints.Add(bp.targetItem.itemid);
                bp.workbenchLevelRequired = 0;            
                bp.time = 0f;
            }          
        }

        #endregion

        #region  Default Items on Respawn Methods

        private void SetupUserBaseItems(BasePlayer player) {   
            for (var i = 0; i < 6; i++) {
                Item item = player.inventory.containerBelt.GetSlot(i);
                if (item == null) continue;
                if (item.name != "building.planner" || item.name != "hammer") return;
                item.RemoveFromContainer();
                item.Remove();
            }

            player.inventory.containerBelt.Clear();
            for (var i = 0; i < config.baseBeltItems.Count && i < 6; i++) {
                var item = ItemManager.CreateByName(config.baseBeltItems[i].Name, config.baseBeltItems[i].Count);
                if (item == null) continue;
                if (!item.MoveToContainer(player.inventory.containerBelt, i, false, false)) {
                    item.Remove();
                }
            }
        }

        #endregion

        #region Quick despawn Methods

        private void SetupQuickDespawn() {
            foreach (ItemDefinition itemDefinition in ItemManager.itemDictionary.Values) {
                itemDefinition.quickDespawn = true;
            }
        }

        #endregion

        #region Imma firin' mah lazer Methods

        private Dictionary<string, int> heldEntityLayers = new Dictionary<string, int> {
            {"hammer.entity", LayerMask.GetMask("Construction", "Default", "Deployed", "Resource", "Terrain", "Water", "World", "Tree")},
            {"wiretool.entity", LayerMask.GetMask("Deployed")},
            {"pipetool.entity", LayerMask.GetMask("Deployed")},
            {"hosetool.entity", LayerMask.GetMask("Deployed")},
        };

        private BaseEntity GetRaycastEntity(BasePlayer player, string heldEntityName) {   
            int layers;
            if (!heldEntityLayers.TryGetValue(heldEntityName, out layers)) return null;
            RaycastHit hit;
            UnityEngine.Physics.Raycast(player.eyes.HeadRay(), out hit, 100f, layers);
            
            var ent = hit.GetEntity();
            if (ent is not BaseEntity) return null;

            return ent;
        }

        private void CheckLazer(CustomPlayer customPlayer, InputState input) {
            BaseEntity entity;

            if (customPlayer.BasePlayer.GetHeldEntity() != null && input.IsDown(BUTTON.SPRINT) && (entity = GetRaycastEntity(customPlayer.BasePlayer, customPlayer.BasePlayer.GetHeldEntity().ShortPrefabName)) != null) {
                customPlayer.Ui.RenderEntityNameUi(entity.ShortPrefabName);

                if (input.WasJustPressed(BUTTON.RELOAD)) {
                    entity.Kill();
                }
            } else {
                customPlayer.Ui.RemoveEntityNameUi();
            }

            if (customPlayer.BasePlayer.GetHeldEntity() != null && customPlayer.BasePlayer.GetHeldEntity().ShortPrefabName == "hammer.entity" && input.WasJustPressed(BUTTON.FIRE_PRIMARY)) {
                if (input.IsDown(BUTTON.SPRINT)) {
                    UpgradeBuildingGrade(customPlayer);
                } else if (input.IsDown(BUTTON.DUCK)) {
                    DowngradeBuildingGrade(customPlayer);
                }
            }
        }

        #endregion

        #region Full HP/Food/Water Methods

        private void RefillStats(CustomPlayer customPlayer) {
            if (customPlayer.BasePlayer) {
                customPlayer.BasePlayer.SetHealth(customPlayer.BasePlayer.MaxHealth());
                customPlayer.BasePlayer.metabolism.hydration.SetValue(250);
                customPlayer.BasePlayer.metabolism.calories.SetValue(500);
            }
        }

        #endregion

        #region Bed Limit Methods

        private void SetupBedLimit() {
            ConVar.Server.max_sleeping_bags = -1;
            ConVar.Server.respawnAtDeathPosition = true;
        }

        #endregion

        #region Main Menu Methods

        private void CheckMenuInputAndToggle(CustomPlayer customPlayer, InputState input) {
            if (input.WasJustPressed(BUTTON.FIRE_THIRD)) {
                Ui playerUi = customPlayer.Ui;
                if (!customPlayer.Ui.OpenPanels.Contains(Ui.PanelNames.CursorLayer)) {
                    playerUi.InstantiateMenuUi();
                } else {
                    playerUi.RemoveMenuUi();
                }
            }
        }

        #endregion

        public class Ui {
            public static class PanelNames {
                public const string BuildGrade = "BuildGrade";
                public const string Crosshair = "Crosshair";
                public const string EntityName = "EntityName";
                public const string CursorLayer = "CursorLayer";
                public const string MainMenu = "MainMenu";
                public const string MenuNavigation = "MenuNavigation";
                public const string LeftPanel = "LeftPanel";
                public const string BuildSkinPanel = "BuildSkinPanel";
                public const string RightPanel = "RightPanel";
            }

            private class SpriteImage {
                public string Image;
                public string Url;
                public string Png;
                public string Sprite;
                public string Material;
                public string Color;
                public int? ItemId;
                public ulong? SkinId;

               public SpriteImage() { }
            }

            private class ButtonContent : SpriteImage {
                public string Text;

               public ButtonContent() { }

               public ButtonContent(string text) {
                  Text = text;
               }
            }

            public enum BuildGradeIDs {
                Twig = 642482233,
                Wood = -151838493,
                Stone = -2099697608,
                Metal = 69511070,
                HQM = 317398316,
            }

            public HashSet<string> OpenPanels {get;} = new HashSet<string>();
            private CustomPlayer _customPlayer;
            private Plugin _imageLibrary;
            private int[] buildGrades = {(int)BuildGradeIDs.Twig, (int)BuildGradeIDs.Wood, (int)BuildGradeIDs.Stone, (int)BuildGradeIDs.Metal, (int)BuildGradeIDs.HQM};

            public Ui(Plugin imageLibrary, CustomPlayer customPlayer) {
                _customPlayer = customPlayer;
                _imageLibrary = imageLibrary;
            }

            public void Init() {
            }

            public void RenderGradeUi() {     
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.BuildGrade);
                CuiElementContainer pageContainer = CreateElementContainer("Hud", PanelNames.BuildGrade, "0 0 0 0", "0.7 0.067", "0.8 0.067", false);
                for (int i = 0; i < buildGrades.Length; i++) {
                    var spriteImage = new SpriteImage();
                    spriteImage.ItemId = buildGrades[i];
                    CreatePanel(ref pageContainer, PanelNames.BuildGrade, i == _customPlayer.BuildingGrade ? "0.24 0.43 0.64 0.9" : "0.5 0.5 0.5 0.4", 0.25 * i + " 0",  0.25 * i + " 0", "-30 -30");
                    CreateSprite(ref pageContainer, PanelNames.BuildGrade, spriteImage,  i == _customPlayer.BuildingGrade ? "1 1 1 1" : "1 1 1 0.8", 0.25 * i + " 0", 0.25 * i + " 0", "-30 -30", null);
                }
                
                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                OpenPanels.Add(PanelNames.BuildGrade);
            }

            public void RemoveGradeUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.BuildGrade);
                OpenPanels.Remove(PanelNames.BuildGrade);
            }

            public void RenderCrosshairUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.Crosshair);
                CuiElementContainer pageContainer = CreateElementContainer("Hud", PanelNames.Crosshair, "0 0 0 0",  "0.472 0.45", "0.528 0.55", false);

                pageContainer.Add(new CuiPanel {
                    Image = {Color = "1 0 0 1"},
                    RectTransform = {AnchorMin = "0.456 0.488", AnchorMax = "0.516 0.489"}
                }, PanelNames.Crosshair);

                pageContainer.Add(new CuiPanel {
                    Image = {Color = "1 0 0 1"},
                    RectTransform = {AnchorMin = "0.488 0.456", AnchorMax = "0.489 0.516"}
                }, PanelNames.Crosshair);

                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                OpenPanels.Add(PanelNames.Crosshair);
            }

            public void RemoveCrosshairUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.Crosshair);
                OpenPanels.Remove(PanelNames.Crosshair);
            }

            public void RenderEntityNameUi(string text) {     
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.EntityName);
                CuiElementContainer pageContainer = CreateElementContainer("Hud", PanelNames.EntityName, "0 0 0 0",  "0 0.4", "0.48 0.6", false);
                CreateLabel(ref pageContainer, PanelNames.EntityName, "1 1 1 1", text, 12, "0 0", "1 1", TextAnchor.MiddleRight);

                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                OpenPanels.Add(PanelNames.EntityName);
            }

            public void RemoveEntityNameUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.EntityName);
                OpenPanels.Remove(PanelNames.EntityName);
            }

            private void InstantiateCursorLayer() {
                RemoveCursorLayer();
                CuiElementContainer cursorLayerContainer = CreateElementContainer("Overlay", PanelNames.CursorLayer, "0 0 0 0.7", "0 0", "1 1", true);
                CuiHelper.AddUi(_customPlayer.BasePlayer, cursorLayerContainer);
            }

            private void RemoveCursorLayer() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.CursorLayer);
            }

            public void InstantiateMenuUi() {
                InstantiateCursorLayer();
                OpenPanels.Add(PanelNames.CursorLayer);
                RenderMainMenuUi();
            }

            public void RemoveMenuUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.MainMenu);
                RemoveCursorLayer();
                OpenPanels.Remove(PanelNames.CursorLayer);
                OpenPanels.Remove(PanelNames.MainMenu);
            }

            public void RenderMainMenuUi() {    
                OpenPanels.Add(PanelNames.MainMenu);

                Grid grid = new Grid(15, 6, 0.01175f, 0.01375f); 
                GridCoordinates gridCoordinates;
                CuiElementContainer pageContainer = CreateElementContainer("Overlay", PanelNames.MainMenu, "0 0 0 0", "0 0", "1 1", false);
                CreatePanel(ref pageContainer, PanelNames.MainMenu, "0.1 0.1 0.1 0.7", "0.0 0.925", "1 1");
                CreateLabel(ref pageContainer, PanelNames.MainMenu, "1 1 1 1", "Main Menu", 16, "0.0 0.925", "1 1", TextAnchor.MiddleCenter);

                CuiElementContainer leftPanelContainer = CreateElementContainer(PanelNames.MainMenu, PanelNames.LeftPanel, "0 0 0 0", "0.05 0.3", "0.475 0.7", true);
                gridCoordinates = grid.GetGridCoordinates(1, 1, 3, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.BuildingGrade == 0, new ButtonContent("Twig"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.grade twig");
                gridCoordinates = grid.GetGridCoordinates(4, 1, 3, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.BuildingGrade == 1, new ButtonContent("Wood"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.grade wood");
                gridCoordinates = grid.GetGridCoordinates(7, 1, 3, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.BuildingGrade == 2, new ButtonContent("Stone"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.grade stone");
                gridCoordinates = grid.GetGridCoordinates(10, 1, 3, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.BuildingGrade == 3, new ButtonContent("Metal"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.grade metal");
                gridCoordinates = grid.GetGridCoordinates(13, 1, 3, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.BuildingGrade == 4, new ButtonContent("Armor"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.grade hqm");
                gridCoordinates = grid.GetGridCoordinates(1, 2, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, false, new ButtonContent("Downgrade"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.downgrade");
                gridCoordinates = grid.GetGridCoordinates(1, 3, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, false, new ButtonContent("Upgrade"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.upgrade");
                gridCoordinates = grid.GetGridCoordinates(1, 4, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, false, new ButtonContent("Select Spawnable"), gridCoordinates.aMin, gridCoordinates.aMax, null);
                gridCoordinates = grid.GetGridCoordinates(1, 5, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.IsStability, new ButtonContent("Building Stability"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.stability");
                gridCoordinates = grid.GetGridCoordinates(1, 6, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, false, new ButtonContent("Resources Needed"), gridCoordinates.aMin, gridCoordinates.aMax, null);
                gridCoordinates = grid.GetGridCoordinates(6, 2, 10, 5);
                CreatePanel(ref leftPanelContainer, PanelNames.LeftPanel, "0.1 0.1 0.1 0.7", gridCoordinates.aMin, gridCoordinates.aMax);

                CuiElementContainer rightPanelContainer = CreateElementContainer(PanelNames.MainMenu, PanelNames.RightPanel, "0 0 0 0", "0.525 0.3", "0.95 0.7", true);
                gridCoordinates = grid.GetGridCoordinates(1, 2, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, _customPlayer.IsCrosshair, new ButtonContent("Crosshair"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.crosshair");
                gridCoordinates = grid.GetGridCoordinates(1, 3, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, _customPlayer.BasePlayer.IsFlying, new ButtonContent("Noclip (Fly)"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.noclip");
                gridCoordinates = grid.GetGridCoordinates(1, 4, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, _customPlayer.IsAmmoInfinite, new ButtonContent("Infinite Ammo"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.infiniteammo");
                gridCoordinates = grid.GetGridCoordinates(1, 5, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, _customPlayer.IsDurability, new ButtonContent("Weapon Durability"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.durability");
                gridCoordinates = grid.GetGridCoordinates(1, 6, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, _customPlayer.IsGod, new ButtonContent("God Mode"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.godmode");
                gridCoordinates = grid.GetGridCoordinates(6, 2, 10, 5);
                CreatePanel(ref rightPanelContainer, PanelNames.RightPanel, "0.1 0.1 0.1 0.7", gridCoordinates.aMin, gridCoordinates.aMax);

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.MainMenu);
                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, leftPanelContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, rightPanelContainer);
                RenderBuildSkinPanel();
            }

            public void RenderBuildSkinPanel () {
                GridCoordinates gridCoordinates;
                CuiElementContainer buildSkinPanelContainer = CreateElementContainer(PanelNames.MainMenu, PanelNames.BuildSkinPanel, "0 0 0 0", "0.05 0.17", "0.475 0.29", true);
                Grid buildSkinGrid = new Grid(16, 3, 0.01175f, 0.05f);

                var buildingGradeSkins = BuildingImages[_customPlayer.BuildingGrade];
                var selectedSkin = _customPlayer.BuildingSkins[_customPlayer.BuildingGrade];

                var spriteImage = new ButtonContent();
                for (int i = 0; i < buildingGradeSkins.Count; i++) {
                    var buildingSkin = buildingGradeSkins[i];
                    spriteImage.Image = buildingSkin.Title;
                    gridCoordinates = buildSkinGrid.GetGridCoordinates(i * 2 + 1, 2, 2, 2);
                    CreateMenuButton(ref buildSkinPanelContainer, PanelNames.BuildSkinPanel, selectedSkin == buildingSkin.SkinId, spriteImage, gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.skin " + buildingSkin.SkinId);
                }

                var selectedColoredSkin = buildingGradeSkins.FirstOrDefault(buildingSkin => buildingSkin.SkinId == selectedSkin && buildingSkin.Colors != null);
                if (selectedColoredSkin != null) {
                     var colors = selectedColoredSkin.Colors;
                    for (int i = 0; i < colors.Count; i++) {
                        gridCoordinates = buildSkinGrid.GetGridCoordinates(i + 1, 1, 1, 1);
                        CreateColorButton(ref buildSkinPanelContainer, PanelNames.BuildSkinPanel, colors[i].ColorId == _customPlayer.BuildingSkinColors[selectedSkin], colors[i].RGBA, gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.color " + colors[i].ColorId);
                    }
                }

                CuiHelper.AddUi(_customPlayer.BasePlayer, buildSkinPanelContainer);
            }
            
            static public CuiElementContainer CreateElementContainer(string parent, string panelName, string color, string aMin, string aMax, bool useCursor) {
                var NewElement = new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = {Color = color},
                            RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                            CursorEnabled = useCursor,
                        },
                        new CuiElement().Parent = parent,
                        panelName
                    }
                };
                return NewElement;
            }

            static private void CreatePanel(ref CuiElementContainer container, string panel, string color, string aMin, string aMax) {
                container.Add(new CuiPanel
                {
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Image = {Color = color}
                },
                panel);
            }

            static private void CreatePanel(ref CuiElementContainer container, string panel, string color, string aMin, string aMax, string offsetMin) {
                container.Add(new CuiPanel
                {
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax, OffsetMin = offsetMin },
                    Image = {Color = color}
                },
                panel);
            }

            private void CreateMenuButton(ref CuiElementContainer container, string panel, bool active, ButtonContent content, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter) {
                container.Add(new CuiPanel {
                    Image = {Color = active ? "0.05 0.85 0.1 0.7" : "0.1 0.1 0.1 0.7"},
                    RectTransform = {AnchorMin = aMin, AnchorMax = aMax}
                }, panel);

                if (content.Text != null) {
                    container.Add(new CuiElement {
                        Components = {
                            new CuiTextComponent { Color = "1 1 1 1", FontSize = 12, Align = align, Text = content.Text },
                            new CuiOutlineComponent { Color = "0 0 0 1" , Distance = "0.5 -0.5"},
                            new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },
                        },
                        Parent = panel
                    });
                } else {
                    CreateSprite(ref container, panel, content, active ? "1 1 1 1" : "1 1 1 0.8", aMin, aMax, "1 1", "-1 -1");
                }

                container.Add(new CuiElement {
                    Components = {
                        new CuiButtonComponent { Color = "0 0 0 0", Command = command},
                        new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },
                    }, 
                    Parent = panel
                });
            }

            private void CreateColorButton(ref CuiElementContainer container, string panel, bool active, string color, string aMin, string aMax, string command) {
                var circle = new ButtonContent();
                circle.Image = "Circle";
                if (active) CreateSprite(ref container, panel, circle, "0.05 0.85 0.1 0.7", aMin, aMax, "-1.5 -1.5", "1.5 1.5");
                CreateSprite(ref container, panel, circle, "0.2 0.2 0.2 1", aMin, aMax, "0 0", "0 0");
                CreateSprite(ref container, panel, circle, color, aMin, aMax, "1.5 1.5", "-1.5 -1.5");

                container.Add(new CuiElement {
                    Components = {
                        new CuiButtonComponent { Color = "0 0 0 0", Command = command},
                        new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },
                    },
                    Parent = panel
                });
            }

            static private List<float> ParseCoordinates(string coordinates) {
                string[] parts = coordinates.Split(' ');
                float.TryParse(parts[0], out float x);
                float.TryParse(parts[1], out float y);
                return new List<float> {x, y};
            }

            static private void CreateButton(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter) {
                container.Add(new CuiButton
                {
                    Button = { Color = color, Command = command},
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Text = { Text = text, FontSize = size, Align = align }
                },
                panel);
            }

            static public void CreateLabel(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter) {
                container.Add(new CuiElement {
                    Components = { 
                        new CuiTextComponent { Color = color, FontSize = size, Align = align, Text = text },
                        new CuiOutlineComponent { Color = "0 0 0 1" , Distance = "1 -1"},
                        new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },
                    },
                    Parent = panel
                });
            }

            
            private void CreateSprite(ref CuiElementContainer container, string panel, SpriteImage image, string color, string aMin, string aMax, string offsetMin, string offsetMax) {
                CuiRawImageComponent cuiRawImageComponent = null;
                CuiImageComponent cuiImageComponent = null;
                if (image.Image != null && _imageLibrary != null) {
                    cuiRawImageComponent = new CuiRawImageComponent {
                        Color = color,
                        Png = (string) _imageLibrary.Call("GetImage", image.Image)
                    };
                } else if (image.Url != null) {
                    cuiRawImageComponent = new CuiRawImageComponent {
                        Color = color,
                        Url = image.Url,
                    };
                } else if (image.Png != null) {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                        Png = image.Png,
                    };
                } else if (image.ItemId != null && image.SkinId != null) {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                        ItemId = (int)image.ItemId,
                        SkinId = (ulong)image.SkinId,
                    };
                } else if (image.ItemId != null) {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                        ItemId = (int)image.ItemId,
                    };
                } else if (image.Sprite != null) {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                        Sprite = image.Sprite,
                    };
                } else if (image.Material != null) {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                        Material = image.Material,
                    };
                } else {
                    cuiImageComponent = new CuiImageComponent {
                        Color = color,
                    };
                }

                container.Add(new CuiElement {
                    Parent = panel,
                    Components = {
                        cuiRawImageComponent != null ? cuiRawImageComponent : cuiImageComponent,
                        new CuiRectTransformComponent {
                            AnchorMin = aMin,
                            AnchorMax = aMax,
                            OffsetMin = offsetMin,
                            OffsetMax = offsetMax,
                        },
                    }
                });
            }

            private class GridCoordinates {
                public string aMin {get; set; }
                public string aMax {get; set; }

                public GridCoordinates(int column, int columnSpan, int columnCount, int row, int rowSpan, int rowCount, float columnGap, float rowGap) {
                    float cellWidth = (1 - columnGap * (columnCount - 1)) / columnCount;
                    float cellHeight = (1 - rowGap * (rowCount - 1)) / rowCount;
                    float x1 = (cellWidth + columnGap) * (column - 1);
                    float y1 = (cellHeight + rowGap) * (row - 1);
                    float x2 = x1 + columnSpan * (cellWidth + columnGap) - columnGap;
                    float y2 = y1 + rowSpan * (cellHeight + rowGap) - rowGap;

                    aMin = x1.ToString("n4") + " " + y1.ToString("n4");
                    aMax = x2.ToString("n4") + " " + y2.ToString("n4");
                }
            }

            private class Grid {
                private int _columnCount {get; set; }
                private int _rowCount {get; set; }
                private float _columnGap {get; set; }
                private float _rowGap {get; set; }
        

                public Grid(int columnCount, int rowCount, float columnGap, float rowGap) {
                    _columnCount = columnCount;
                    _rowCount = rowCount;
                    _columnGap = columnGap;
                    _rowGap = rowGap;
                }

                public GridCoordinates GetGridCoordinates(int column, int row) {
                    return new GridCoordinates(column, 1, _columnCount, row, 1, _rowCount, _columnGap, _rowGap);
                }

                public GridCoordinates GetGridCoordinates(int column, int row, int columnSpan, int rowSpan) {
                    return new GridCoordinates(column, columnSpan, _columnCount, row, rowSpan, _rowCount, _columnGap, _rowGap);
                }
            }
        }
    }
}