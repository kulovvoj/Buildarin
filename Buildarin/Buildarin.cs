using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Rust;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Plugins;
using Oxide.Core;
using UnityEngine.UI;
using CompanionServer;
using ConVar;

namespace Oxide.Plugins {
    [Info("Buildarin - the builder's plugin", "Daladirn", "0.1.0")]
    [Description("A plugin to supply you everything you might need in day to day building")]
    class Buildarin : RustPlugin {
        public static Buildarin Instance;
		public TOD_Time TimeComponent;
        private List<int> blueprints = new List<int>();
        private List<ulong> wallpaperWallIDs = new List<ulong>();
        private List<ulong> wallpaperFloorIDs = new List<ulong>();
        private List<ulong> wallpaperCeilingIDs = new List<ulong>();
        private Config config = new Config();
		private int TimeComponentSearchAttempts;
		public List<SpawnablePrefab> spawnablePrefabs = new List<SpawnablePrefab>();

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
                new("Jungle", "https://i.ibb.co/ch19Hr2g/Stone-Jungle.png", 10326),
                new("Crypt", "https://i.ibb.co/xNmxGyM/Stone-Crypt.png", 10472)
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

        public class SpawnablePrefab {
            public string Name;
            public string Value;

            public SpawnablePrefab(string prefab) {
                string value = prefab.Substring(prefab.LastIndexOf('/') + 1);
                if (value.EndsWith(".prefab"))
                    value = value.Substring(0, value.Length - ".prefab".Length);
                string name = value
                    .Replace("-", " ")
                    .Replace("_", " ")
                    .Replace(".", " ")
                    .ToUpperInvariant();
                Name = name;
                Value = value;
            }
        }

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
            public bool IsGradePanel {get; set;}
            public int BuildingGrade {get; set;}
            public int SpawnablePage {get; set;}
            public string SpawnableFilter  {get; set;}
            public SpawnablePrefab SelectedSpawnable {get; set;}
            public Dictionary<int, ulong> BuildingSkins;
            public Dictionary<ulong, uint> BuildingSkinColors;

            public CustomPlayer(Buildarin buildarin, BasePlayer basePlayer) {
                BasePlayer = basePlayer;
                Ui = new Ui(buildarin, this);
                Ui.RenderGradeUi();
                IsAdmin = basePlayer.IsAdmin;
                IsGod = true;
                IsDurability = false;
                IsAmmoInfinite = true;
                IsStability = true;
                IsCrosshair = false;
                IsGradePanel = true;
                BuildingGrade = 0;
                BuildingSkins = new Dictionary<int, ulong> {
                    {0, 0},
                    {1, 0},
                    {2, 0},
                    {3, 0},
                    {4, 0},
                };
                SpawnablePage = 1;
                SpawnableFilter = "";
                SelectedSpawnable = null;

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
                if (IsGradePanel) {
                    Ui.RenderGradeUi();
                }
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
                customPlayer = new CustomPlayer(this, player);
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

            InitTimeComponent();
            InitImageLibrary();
            SetupBlueprints();

            SetupSpawnablePrefabs();
            SetupWallpaperIDs(wallpaperWallIDs, "wallpaper.wall");
            SetupWallpaperIDs(wallpaperFloorIDs, "wallpaper.flooring");
            SetupWallpaperIDs(wallpaperCeilingIDs, "wallpaper.ceiling");
            SetupQuickDespawn();
            SetupBedLimit();
        }

        private void InitTimeComponent() {
			if (TOD_Sky.Instance == null) {
				TimeComponentSearchAttempts++;
                if (TimeComponentSearchAttempts < 10)
                    timer.Once(1, OnServerInitialized);
                else
                    PrintWarning("Could not find required component after 10 attempts. Plugin disabled");
                return;
            }
            TimeComponent = TOD_Sky.Instance.Components.Time;
            TimeComponent.ProgressTime = false;
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

            if (arg.HasArgs(1)) {
                switch (arg.GetString(0)) {
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

            if (arg.HasArgs(1)) {
                switch (arg.GetString(0)) {
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

            if (arg.HasArgs(1)) {
                if (ulong.TryParse(arg.GetString(0), out ulong skinId)) {
                    customPlayer.SetBuildSkinAndUpdateUi(customPlayer.BuildingGrade, skinId);
                }
            }
        }

        [ConsoleCommand("buildarin.color")]
        private void CommmandBuildColor(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.HasArgs(1)) {
                if (uint.TryParse(arg.GetString(0), out uint colorId)) {
                    customPlayer.SetBuildColorAndUpdateUi(customPlayer.BuildingSkins[customPlayer.BuildingGrade], colorId);
                }
            }
        }

        [ConsoleCommand("buildarin.spawnable")]
        private void CommandSpawnable(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;
            SpawnableToggle(customPlayer);
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

        [ConsoleCommand("buildarin.gradepanel")]
        private void CommandGradePanel(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.IsGradePanel = !customPlayer.IsGradePanel;
            if (customPlayer.IsGradePanel) {
                customPlayer.Ui.RenderGradeUi();
            } else {
                customPlayer.Ui.RemoveGradeUi();
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

        [ConsoleCommand("buildarin.freezetime")]
        private void CommandFreezeTime(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            TimeComponent.ProgressTime = !TimeComponent.ProgressTime;
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

        [ConsoleCommand("buildarin.selectspawnable")]
        private void CommmandSelectSpawnable(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.HasArgs(1)) {
                SpawnablePrefab prefab = spawnablePrefabs.FirstOrDefault(
                    prefab => prefab.Value.Equals(arg.GetString(0), StringComparison.OrdinalIgnoreCase)
                );
                customPlayer.SelectedSpawnable = prefab;
            }
            customPlayer.Ui.RemoveSpawnableUi();
        }

        [ConsoleCommand("buildarin.spawnablepage")]
        private void CommmandSpawnablePage(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.HasArgs(1) && int.TryParse(arg.GetString(0), out int page)) {
                customPlayer.SpawnablePage = page;
                customPlayer.Ui.RenderSpawnableSelection();
            }
        }

        [ConsoleCommand("buildarin.spawnablefilter")]
        private void CommmandSpawnableFilter(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            if (arg.HasArgs(1)) {
                customPlayer.SpawnableFilter = arg.GetString(0);
                customPlayer.SpawnablePage = 1;
                customPlayer.Ui.RenderSpawnableSelection();
                customPlayer.Ui.RenderSpawnableFilterClear();
            }
        }

        [ConsoleCommand("buildarin.spawnablefilterclear")]
        private void CommmandSpawnableFilterClear(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.SpawnableFilter = "";
            customPlayer.SpawnablePage = 1;
            customPlayer.Ui.RenderSpawnableSelection();
            customPlayer.Ui.RenderSpawnableFilter();
            CuiHelper.DestroyUi(customPlayer.BasePlayer, Ui.PanelNames.SpawnableFilterClear);
        }

        [ConsoleCommand("buildarin.selectedspawnableclear")]
        private void CommmandSelectedSpawnableClear(ConsoleSystem.Arg arg) {
            if (arg.Player() == null) return;
            CustomPlayer customPlayer;
            if (!CustomPlayer.TryGetPlayer(arg.Player(), out customPlayer)) return;

            customPlayer.SelectedSpawnable = null;
            CuiHelper.DestroyUi(customPlayer.BasePlayer, Ui.PanelNames.SelectedSpawnableClear);
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

        #region Spawnable Prefabs Methods

        private void SetupSpawnablePrefabs() {
            var prefabs = new HashSet<string>();

            foreach (var prefab in GameManifest.Current.entities) {
                if (!string.IsNullOrEmpty(prefab) && prefab.StartsWith("Assets/bundled/Prefabs/autospawn/resource")) {
                    spawnablePrefabs.Add(new SpawnablePrefab(prefab));
                }
            }
        }

        void SpawnSpawnable(CustomPlayer customPlayer) {
            if (customPlayer.SelectedSpawnable == null) return;

            Ray ray = customPlayer.BasePlayer.eyes.HeadRay();
            RaycastHit hit;
            if (!UnityEngine.Physics.Raycast(ray, out hit, 100f, ~0)) return;

            Vector3 hitLocation = hit.point;
            Vector3 direction = ray.direction;
            direction.y = 0;
            direction.Normalize();
            Entity.svspawn(customPlayer.SelectedSpawnable.Value, hitLocation, direction);
        }

        #endregion

        #region Wallpaper Methods

        void ChangeWallpaperId(CustomPlayer customPlayer, int offset) {
            RaycastHit hit;
            if (!UnityEngine.Physics.Raycast(customPlayer.BasePlayer.eyes.HeadRay(), out hit, 100f, LayerMask.GetMask("Construction"))) return;

            var entity = hit.GetEntity();

            if (entity is BuildingBlock buildingBlock) {
                if (buildingBlock == null) return;
                if (buildingBlock.PrefabName.Contains("floor")) {
                    if (buildingBlock.HasWallpaper(0) && wallpaperCeilingIDs.Count > 0) {
                        buildingBlock.SetWallpaper(GetNewWallpaper(wallpaperCeilingIDs, buildingBlock.wallpaperID, offset), 0, 0f);
                    }
                    if (buildingBlock.HasWallpaper(1) && wallpaperFloorIDs.Count > 0) {
                        buildingBlock.SetWallpaper(GetNewWallpaper(wallpaperFloorIDs, buildingBlock.wallpaperID, offset), 1, 0f);
                    }
                } else if (buildingBlock.PrefabName.Contains("foundation") && buildingBlock.HasWallpaper(0)) {
                    if (wallpaperFloorIDs.Count > 0)
                        buildingBlock.SetWallpaper(GetNewWallpaper(wallpaperFloorIDs, buildingBlock.wallpaperID, offset), 0, 0f);
                } else if (buildingBlock.HasWallpaper(0) && wallpaperWallIDs.Count > 0) {
                    buildingBlock.SetWallpaper(GetNewWallpaper(wallpaperWallIDs, buildingBlock.wallpaperID, offset), 0, 0f);
                }
            }
        }

        ulong GetNewWallpaper(List<ulong> wallpaperList, ulong currentId, int offset) {
            var currentIndex = wallpaperList.IndexOf(currentId);
            if (currentIndex == -1) {
                return wallpaperList[0];
            }
            var newIndex = ((currentIndex + offset) % wallpaperList.Count + wallpaperList.Count) % wallpaperList.Count;
            return wallpaperList[newIndex];
        }

        private void SetupWallpaperIDs(List<ulong> list, string itemName) {
            if (list == null) return;
            list.Clear();

            var itemDef = ItemManager.FindItemDefinition(itemName);

            if (itemDef == null) return;
            var skins = ItemSkinDirectory.ForItem(itemDef);

            if (skins == null || skins.Count() == 0) return;

            foreach (var skin in skins) {
                list.Add((ulong)skin.id);
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
            player.ClientRPC(RpcTarget.Player("UnlockedBlueprint", player), 0);
        }

        #endregion

        #region No Workbench Required/Instant Craft Methods

        private void DisableWorkbenchRequirements(BasePlayer player) {
            player.ClientRPC(RpcTarget.Player("craftMode", player), 1);
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

        private Dictionary<string, List<string>> heldEntityLayers = new Dictionary<string, List<string>> {
            {"hammer.entity", new List<string> { "Construction", "Default", "Deployed", "Resource", "Terrain", "Water", "World", "Tree", "Vehicle Detailed" } },
            {"wiretool.entity", new List<string> { "Deployed" } },
            {"pipetool.entity", new List<string> { "Deployed" } },
            {"hosetool.entity", new List<string> { "Deployed" } },
        };

        private BaseEntity GetRaycastEntity(BasePlayer player, string heldEntityName) {
            List<String> layers;
            if (!heldEntityLayers.TryGetValue(heldEntityName, out layers)) return null;
            RaycastHit hit;
            UnityEngine.Physics.Raycast(player.eyes.HeadRay(), out hit, 100f, LayerMask.GetMask("Construction", "Default", "Deployed", "Resource", "Terrain", "Water", "World", "Tree"));

            var ent = hit.GetEntity();
            if (ent is not BaseEntity) return null;
            if (!layers.Contains(LayerMask.LayerToName(ent.gameObject.layer))) return null;
            return ent;
        }

        private void CheckLazer(CustomPlayer customPlayer, InputState input) {
            BaseEntity entity;

            if (customPlayer.BasePlayer.GetHeldEntity() != null && input.IsDown(BUTTON.SPRINT) && (entity = GetRaycastEntity(customPlayer.BasePlayer, customPlayer.BasePlayer.GetHeldEntity().ShortPrefabName)) != null) {
                customPlayer.Ui.RenderEntityNameUi(entity);

                if (input.WasJustPressed(BUTTON.RELOAD)) {
                    entity.Kill();
                }
            } else {
                customPlayer.Ui.RemoveEntityNameUi();
            }

            if (input.IsDown(BUTTON.SPRINT) && input.WasJustPressed(BUTTON.USE)) {
                SpawnSpawnable(customPlayer);
            }

            if (customPlayer.BasePlayer.GetHeldEntity() != null) {
                if (customPlayer.BasePlayer.GetHeldEntity().ShortPrefabName == "hammer.entity" && input.WasJustPressed(BUTTON.FIRE_PRIMARY)) {
                    if (input.IsDown(BUTTON.SPRINT)) {
                        UpgradeBuildingGrade(customPlayer);
                    } else if (input.IsDown(BUTTON.DUCK)) {
                        DowngradeBuildingGrade(customPlayer);
                    }
                }
                if (customPlayer.BasePlayer.GetHeldEntity().ShortPrefabName == "wallpaper.tool.entity" && input.WasJustPressed(BUTTON.FIRE_PRIMARY)) {
                    if (input.IsDown(BUTTON.SPRINT)) {
                        ChangeWallpaperId(customPlayer, 1);
                    } else if (input.IsDown(BUTTON.DUCK)) {
                        ChangeWallpaperId(customPlayer, -1);
                    }
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
                if (!customPlayer.Ui.OpenPanels.Contains(Ui.PanelNames.MainMenu) && !customPlayer.Ui.OpenPanels.Contains(Ui.PanelNames.SpawnableMenu)) {
                    playerUi.InstantiateMenuUi();
                } else if (customPlayer.Ui.OpenPanels.Contains(Ui.PanelNames.MainMenu)) {
                    playerUi.RemoveMenuUi();
                } else if (customPlayer.Ui.OpenPanels.Contains(Ui.PanelNames.SpawnableMenu)) {
                    playerUi.RemoveSpawnableUi();
                }
            }
        }

        private void SpawnableToggle(CustomPlayer customPlayer) {
            Ui playerUi = customPlayer.Ui;
            playerUi.RemoveMenuUi();
            playerUi.InstantiateSpawnableUi();
        }

        #endregion

        public class Ui {
            public static string LeftPanelText = "<size=14>This plugin has a couple extra features:</size>\n" +
                "<color=#CCCCCC>" +
                "• Prevents buildings from decaying\n" +
                "• Unlocks all BPs\n" +
                "• Tier 3 crafting anywhere\n" +
                "• Instant craft\n" +
                "• Items will not be consumed on placement\n\n" +
                "</color>";

            public static string RightPanelText = "<size=14>Middle click</size>\n" +
                "<color=#CCCCCC>" +
                "• Opens and closes menu\n" +
                "</color>" +
                "<size=14>Light toggle</size>\n" +
                "<color=#CCCCCC>" +
                "• Cycles between building grades\n" +
                "</color>" +
                "<size=14>Holding a tool + Sprint + Reload</size>\n" +
                "<color=#CCCCCC>" +
                "• Destroys an entity you're looking at\n" +
                "• Hammer - Entity types of 'Construction', 'Default', 'Deployed', 'Resource', 'Terrain', 'Water', 'World', 'Tree'\n" +
                "• Wire/Hose/Pipe tool - Entity type of 'Deployed'\n" +
                "</color>" +
                "<size=14>Holding a hammer + Sprint + Attack</size>\n" +
                "<color=#CCCCCC>" +
                "• Upgrades building block you're looking at to the next tier\n" +
                "</color>" +
                "<size=14>Holding a hammer + Duck + Attack</size>\n" +
                "<color=#CCCCCC>" +
                "• Downgrades building block you're looking at to the previous tier\n" +
                "</color>" +
                "<size=14>Sprint + Use</size>\n" +
                "<color=#CCCCCC>" +
                "• Spawns an entity from the spawnable list" +
                "</color>";

            public static class PanelNames {
                public const string BuildGrade = "BuildGrade";
                public const string Crosshair = "Crosshair";
                public const string EntityName = "EntityName";
                public const string CursorLayer = "CursorLayer";
                public const string MainMenu = "MainMenu";
                public const string SpawnableMenu = "SpawnableMenu";
                public const string MenuNavigation = "MenuNavigation";
                public const string HeaderContainer = "HeaderContainer";
                public const string HeaderBackground = "HeaderBackground";
                public const string FooterContainer = "FooterContainer";
                public const string FooterBackground = "FooterBackground";
                public const string PanelContainer = "PanelContainer";
                public const string LeftPanel = "LeftPanel";
                public const string BuildSkinPanel = "BuildSkinPanel";
                public const string RightPanel = "RightPanel";
                public const string SpawnablePanel = "SpawnablePanel";
                public const string SpawnableFilter = "SpawnableFilter";
                public const string SpawnableFilterClear = "SpawnableFilterClear";
                public const string SelectedSpawnableClear = "SelectedSpawnableClear";
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
            private Buildarin _buildarin;
            private int[] buildGrades = {(int)BuildGradeIDs.Twig, (int)BuildGradeIDs.Wood, (int)BuildGradeIDs.Stone, (int)BuildGradeIDs.Metal, (int)BuildGradeIDs.HQM};
            private int SPAWNABLE_ROWS = 18;
            private int SPAWNABLE_COLUMNS = 7;

            public Ui(Buildarin buildarin, CustomPlayer customPlayer) {
                _customPlayer = customPlayer;
                _buildarin = buildarin;
            }

            public void Init() {
            }

            public void RenderGradeUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.BuildGrade);
                CuiElementContainer pageContainer = CreateElementContainer("Under", PanelNames.BuildGrade, "0 0 0 0", "0.7 0.067", "0.8 0.067", false);
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
                CuiElementContainer pageContainer = CreateElementContainer("Under", PanelNames.Crosshair, "0 0 0 0",  "0 0", "1 1", false);

                pageContainer.Add(new CuiPanel {
                    Image = {Color = "1 0 0 1"},
                    RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-3 -0.5", OffsetMax = "3 0.5"}
                }, PanelNames.Crosshair);

                pageContainer.Add(new CuiPanel {
                    Image = {Color = "1 0 0 1"},
                    RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-0.5 -3", OffsetMax = "0.5 3"}
                }, PanelNames.Crosshair);

                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                OpenPanels.Add(PanelNames.Crosshair);
            }

            public void RemoveCrosshairUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.Crosshair);
                OpenPanels.Remove(PanelNames.Crosshair);
            }

            public void RenderEntityNameUi(BaseEntity entity) {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.EntityName);
                CuiElementContainer pageContainer = CreateElementContainer("Hud", PanelNames.EntityName, "0 0 0 0",  "0 0.4", "0.48 0.6", false);
                CreateLabel(ref pageContainer, PanelNames.EntityName, "1 1 1 1", entity.ShortPrefabName, 12, "0 0", "1 1", TextAnchor.MiddleRight);

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

            public void InstantiateSpawnableUi() {
                _customPlayer.SpawnablePage = 1;
                _customPlayer.SpawnableFilter = "";

                InstantiateCursorLayer();
                OpenPanels.Add(PanelNames.CursorLayer);
                RenderSpawnableUi();
            }

            public void RemoveSpawnableUi() {
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SpawnableMenu);
                RemoveCursorLayer();
                OpenPanels.Remove(PanelNames.CursorLayer);
                OpenPanels.Remove(PanelNames.SpawnableMenu);
            }

            public void RenderSpawnableUi() {
                OpenPanels.Add(PanelNames.SpawnableMenu);

                GridCoordinates gridCoordinates;
                CuiElementContainer pageContainer = CreateElementContainer("Overlay", PanelNames.SpawnableMenu, "0 0 0 0", "0 0", "1 1", false);
                CuiElementContainer headerContainer = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.HeaderContainer, "0 0 0 0", "0.5 1", "0.5 1", false, "-640 -54", "640 0");
                CuiElementContainer panelContainer = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.PanelContainer, "0 0 0 0", "0.5 0.5", "0.5 0.5", false, "-640 -360", "640 360");
                CuiElementContainer footerBackground = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.FooterBackground, "0 0 0 0", "0.5 0", "0.5 0", false, "-640 0", "640 54");
                CreatePanel(ref headerContainer, PanelNames.HeaderContainer, "0.1 0.1 0.1 0.7", "0 0", "1 1");
                CreateLabel(ref headerContainer, PanelNames.HeaderContainer, "1 1 1 1", "Select Spawnable", 16, "0 0", "1 1", TextAnchor.MiddleCenter);
                CreatePanel(ref footerBackground, PanelNames.FooterBackground, "0.1 0.1 0.1 0.7", "0 0", "1 1");

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SpawnableMenu);
                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, panelContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, headerContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, footerBackground);
                if (_customPlayer.SelectedSpawnable != null) {
                    RenderSelectedSpawnableClear();
                }
                RenderSpawnableSelection();
                RenderSpawnableFilter();
            }

            public void RenderSpawnableSelection() {
                List<SpawnablePrefab> filteredPrefabs;
                if (string.IsNullOrEmpty(_customPlayer.SpawnableFilter)) {
                    filteredPrefabs = _buildarin.spawnablePrefabs;
                } else {
                     filteredPrefabs = _buildarin.spawnablePrefabs
                        .Where(prefab => prefab.Name.Contains(_customPlayer.SpawnableFilter.ToUpperInvariant()))
                        .ToList();
                }

                GridCoordinates gridCoordinates;
                CuiElementContainer footerContainer = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.FooterContainer, "0 0 0 0", "0.5 0", "0.5 0", false, "-640 0", "640 54");

                int maxPages = (int)Math.Ceiling((double)filteredPrefabs.Count / (SPAWNABLE_ROWS * SPAWNABLE_COLUMNS));
                if (maxPages > 0) {
                    CreateLabel(ref footerContainer, PanelNames.FooterContainer, "1 1 1 1", $"Page {_customPlayer.SpawnablePage} / {maxPages}", 16, "0 0", "1 1", TextAnchor.MiddleCenter);
                }

                if (_customPlayer.SpawnablePage > 1) {
                    CreateButton(ref footerContainer, PanelNames.FooterContainer, "0.1 0.1 0.1 0.7", "◄", 32, "0.41 0.1", "0.45 0.9", $"buildarin.spawnablepage {Math.Max(0, _customPlayer.SpawnablePage - 1)}", highlightedColor: "0.1 0.1 0.1 0.85", pressedColor: "0.1 0.1 0.1 0.825");
                }
                if (_customPlayer.SpawnablePage < maxPages) {
                    CreateButton(ref footerContainer, PanelNames.FooterContainer, "0.1 0.1 0.1 0.7", "►", 32, "0.55 0.1", "0.59 0.9", $"buildarin.spawnablepage {Math.Min(maxPages, _customPlayer.SpawnablePage + 1)}", highlightedColor: "0.1 0.1 0.1 0.85", pressedColor: "0.1 0.1 0.1 0.825");
                }

                Grid grid = new Grid(SPAWNABLE_COLUMNS, SPAWNABLE_ROWS, 0.005f, 0.01f);
                CuiElementContainer spawnablePanelContainer = CreateElementContainer(PanelNames.PanelContainer, PanelNames.SpawnablePanel, "0 0 0 0", "0.05 0.1", "0.95 0.7", true);
                int pageOffset = SPAWNABLE_ROWS * SPAWNABLE_COLUMNS * (_customPlayer.SpawnablePage - 1);
                for (int i = 0; i < SPAWNABLE_COLUMNS; i++) {
                    for (int j = 0; j < SPAWNABLE_ROWS && pageOffset + j + i * SPAWNABLE_ROWS < filteredPrefabs.Count; j++) {
                        gridCoordinates = grid.GetGridCoordinates(i + 1, SPAWNABLE_ROWS - j);
                        int prefabIndex = pageOffset + j + i * SPAWNABLE_ROWS;
                        string spawnableName = filteredPrefabs[prefabIndex].Name;
                        string spawnableValue = filteredPrefabs[prefabIndex].Value;
                        CreateMenuButton(ref spawnablePanelContainer, PanelNames.SpawnablePanel, false, new ButtonContent(spawnableName), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.selectspawnable " + spawnableValue);
                    }
                }


                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SpawnablePanel);
                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.FooterContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, spawnablePanelContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, footerContainer);
            }

            public void RenderSpawnableFilter() {
                CuiElementContainer spawnableFilter = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.SpawnableFilter, "0 0 0 0", "0.4 0.725", "0.6 0.775", false);

                CreateInputField(ref spawnableFilter, PanelNames.SpawnableFilter, "0.1 0.1 0.1 0.7", "Enter text to search", 14, "0 0", "1 1", $"buildarin.spawnablefilter");

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SpawnableFilter);
                CuiHelper.AddUi(_customPlayer.BasePlayer, spawnableFilter);
            }

            public void RenderSpawnableFilterClear() {
                CuiElementContainer spawnableFilterClear = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.SpawnableFilterClear, "0 0 0 0", "0.57 0.735", "0.6 0.765", false);

                CreateButton(ref spawnableFilterClear, PanelNames.SpawnableFilterClear, "0 0 0 0", "✖", 20, "0 0", "1 1", $"buildarin.spawnablefilterclear", TextAnchor.MiddleCenter, "1 0 0 1");

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SpawnableFilterClear);
                CuiHelper.AddUi(_customPlayer.BasePlayer, spawnableFilterClear);
            }

            public void RenderSelectedSpawnableClear() {
                GridCoordinates gridCoordinates;
                CuiElementContainer selectedSpawnableClear = CreateElementContainer(PanelNames.SpawnableMenu, PanelNames.SelectedSpawnableClear, "0 0 0 0", "0.4 0.8", "0.6 0.85", false);

                CreatePanel(ref selectedSpawnableClear, PanelNames.SelectedSpawnableClear, "0.05 0.85 0.1 0.7", "0 0", "1 1");
                CreateLabel(ref selectedSpawnableClear, PanelNames.SelectedSpawnableClear, "1 1 1 1", _customPlayer.SelectedSpawnable.Name, 12, "0 0", "1 1", TextAnchor.MiddleCenter);
                CreateButton(ref selectedSpawnableClear, PanelNames.SelectedSpawnableClear, "0 0 0 0", "✖", 20, "0.85 0", "1 1", "buildarin.selectedspawnableclear", TextAnchor.MiddleCenter, "1 0 0 1");

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.SelectedSpawnableClear);
                CuiHelper.AddUi(_customPlayer.BasePlayer, selectedSpawnableClear);
            }

            public void RenderMainMenuUi() {
                OpenPanels.Add(PanelNames.MainMenu);

                Grid grid = new Grid(15, 6, 0.01175f, 0.01375f);
                GridCoordinates gridCoordinates;
                CuiElementContainer pageContainer = CreateElementContainer("Overlay", PanelNames.MainMenu, "0 0 0 0", "0 0", "1 1", false);
                CuiElementContainer headerContainer = CreateElementContainer(PanelNames.MainMenu, PanelNames.HeaderContainer, "0 0 0 0", "0.5 1", "0.5 1", false, "-640 -54", "640 0");
                CuiElementContainer panelContainer = CreateElementContainer(PanelNames.MainMenu, PanelNames.PanelContainer, "0 0 0 0", "0.5 0.5", "0.5 0.5", false, "-640 -360", "640 360");
                CreatePanel(ref headerContainer, PanelNames.HeaderContainer, "0.1 0.1 0.1 0.7", "0 0", "1 1");
                CreateLabel(ref headerContainer, PanelNames.HeaderContainer, "1 1 1 1", "Main Menu", 16, "0 0", "1 1", TextAnchor.MiddleCenter);

                CuiElementContainer leftPanelContainer = CreateElementContainer(PanelNames.PanelContainer, PanelNames.LeftPanel, "0 0 0 0", "0.05 0.3", "0.475 0.7", true);
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
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.IsGradePanel, new ButtonContent("Grade Panel"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.gradepanel");
                gridCoordinates = grid.GetGridCoordinates(1, 5, 5, 1);
                CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, _customPlayer.IsStability, new ButtonContent("Building Stability"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.stability");
                gridCoordinates = grid.GetGridCoordinates(1, 6, 5, 1);
                if (_buildarin.TimeComponent != null) {
                    CreateMenuButton(ref leftPanelContainer, PanelNames.LeftPanel, !_buildarin.TimeComponent.ProgressTime, new ButtonContent("Freeze time"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.freezetime");
                }
                gridCoordinates = grid.GetGridCoordinates(6, 2, 10, 5);
                CreateTextPanel(ref leftPanelContainer, PanelNames.LeftPanel, "0.1 0.1 0.1 0.7", LeftPanelText, 12, gridCoordinates.aMin, gridCoordinates.aMax);

                CuiElementContainer rightPanelContainer = CreateElementContainer(PanelNames.PanelContainer, PanelNames.RightPanel, "0 0 0 0", "0.525 0.3", "0.95 0.7", true);
                gridCoordinates = grid.GetGridCoordinates(1, 1, 5, 1);
                CreateMenuButton(ref rightPanelContainer, PanelNames.RightPanel, false, new ButtonContent("Select Spawnable"), gridCoordinates.aMin, gridCoordinates.aMax, "buildarin.spawnable");
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
                CreateTextPanel(ref rightPanelContainer, PanelNames.RightPanel, "0.1 0.1 0.1 0.7", RightPanelText, 12, gridCoordinates.aMin, gridCoordinates.aMax);

                CuiHelper.DestroyUi(_customPlayer.BasePlayer, PanelNames.MainMenu);
                CuiHelper.AddUi(_customPlayer.BasePlayer, pageContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, headerContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, panelContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, leftPanelContainer);
                CuiHelper.AddUi(_customPlayer.BasePlayer, rightPanelContainer);
                RenderBuildSkinPanel();
            }

            public void RenderBuildSkinPanel () {
                GridCoordinates gridCoordinates;
                CuiElementContainer buildSkinPanelContainer = CreateElementContainer(PanelNames.PanelContainer, PanelNames.BuildSkinPanel, "0 0 0 0", "0.05 0.17", "0.475 0.29", true);
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
            
            static public CuiElementContainer CreateElementContainer(string parent, string panelName, string color, string aMin, string aMax, bool useCursor, string offsetMin = "0 0", string offsetMax = "0 0") {
                var NewElement = new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = {Color = color},
                            RectTransform = {AnchorMin = aMin, AnchorMax = aMax, OffsetMin = offsetMin, OffsetMax = offsetMax},
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

            static private void CreateTextPanel(ref CuiElementContainer container, string panel, string color, string text, int fontSize, string aMin, string aMax, TextAnchor align = TextAnchor.UpperLeft) {
                container.Add(new CuiPanel
                {
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Image = {Color = color}
                },
                panel);
                container.Add(new CuiElement {
                    Components = {
                        new CuiTextComponent { Color = "1 1 1 1", FontSize = fontSize, Align = align, Text = text },
                        new CuiOutlineComponent { Color = "0 0 0 1" , Distance = "0.5 -0.5"},
                        new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax, OffsetMin = "5 5", OffsetMax = "-5 -5" },
                    },
                    Parent = panel
                });
            }

            private void CreateMenuButton(ref CuiElementContainer container, string panel, bool active, ButtonContent content, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter) {
                string buttonGuid = CuiHelper.GetGuid();
                string color = active ? "0.05 0.85 0.1 0.7" : "0.1 0.1 0.1 0.7";
                string highlightedColor = active ? "0.05 0.85 0.1 0.85" : "0.1 0.1 0.1 0.85";
                string pressedColor = active ? "0.05 0.85 0.1 0.825" : "0.1 0.1 0.1 0.825";

                container.Add(new CuiElement {
                    Components = {
                        new CuiButtonComponent { PressedColor = pressedColor, SelectedColor = color, DisabledColor = color, NormalColor = color, HighlightedColor = highlightedColor, FadeDuration = 0.1f, Command = command},
                        new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },
                    },
                    Name = buttonGuid,
                    Parent = panel
                });

                if (content.Text != null) {
                    container.Add(new CuiElement {
                        Components = {
                            new CuiTextComponent { Color = "1 1 1 1", FontSize = 12, Align = align, Text = content.Text },
                            new CuiOutlineComponent { Color = "0 0 0 1" , Distance = "0.5 -0.5"},
                            new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" },
                        },
                        Parent = buttonGuid
                    });
                } else {
                    CreateSprite(ref container, buttonGuid, content, active ? "1 1 1 1" : "1 1 1 0.8", "0 0", "1 1", "1 1", "-1 -1");
                }
            }

            private void CreateColorButton(ref CuiElementContainer container, string panel, bool active, string color, string aMin, string aMax, string command) {
                var circle = new ButtonContent();
                circle.Image = "Circle";
//                if (active) CreateSprite(ref container, panel, circle, "0.05 0.85 0.1 0.7", aMin, aMax, "-1.5 -1.5", "1.5 1.5");
//                CreateSprite(ref container, panel, circle, "0.2 0.2 0.2 1", aMin, aMax, "0 0", "0 0");
//                CreateSprite(ref container, panel, circle, color, aMin, aMax, "1.5 1.5", "-1.5 -1.5");

                List<float> aMinCoordinates = ParseCoordinates(aMin);
                List<float> aMaxCoordinates = ParseCoordinates(aMax);
                float avgX = (aMinCoordinates[0] + aMaxCoordinates[0]) / 2;
                float avgY = (aMinCoordinates[1] + aMaxCoordinates[1]) / 2;
                string aAvg = avgX + " " + avgY;
                if (active) CreateSprite(ref container, panel, circle, "0.05 0.85 0.1 0.7", aAvg, aAvg, "-15 -15", "15 15");
                CreateSprite(ref container, panel, circle, "0.2 0.2 0.2 1", aAvg, aAvg, "-13.5 -13.5", "13.5 13.5");
                CreateSprite(ref container, panel, circle, color, aAvg, aAvg, "-12 -12", "12 12");

                container.Add(new CuiElement {
                    Components = {
                        new CuiButtonComponent { Color = "0 0 0 0", Command = command },
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

            static private void CreateButton(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter, string textColor = "1 1 1 1", string highlightedColor = null, string pressedColor = null) {
                if (highlightedColor != null) {
                    container.Add(new CuiButton {
                        Button = { PressedColor = pressedColor != null ? pressedColor : color, SelectedColor = color, DisabledColor = color, NormalColor = color, HighlightedColor = highlightedColor, FadeDuration = 0.1f, Command = command },
                        RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                        Text = { Text = text, FontSize = size, Align = align, Color = textColor }
                    },
                    panel);
                } else {
                    container.Add(new CuiButton {
                        Button = { Color = color, Command = command },
                        RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                        Text = { Text = text, FontSize = size, Align = align, Color = textColor }
                    },
                    panel);
                }
            }

            static private void CreateInputField(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter) {
                container.Add(new CuiElement {
                    Parent = panel,
                    Components = {
                        new CuiImageComponent {
                            Color = color
                        },
                        new CuiRectTransformComponent {
                            AnchorMin = aMin,
                            AnchorMax = aMax
                        }
                    }
                });

                container.Add(new CuiElement {
                    Parent = panel,
                    Components = {
                        new CuiInputFieldComponent {
                            Text = text,
                            FontSize = size,
                            Align = align,
                            Command = command,
                            NeedsKeyboard = true
                        },
                        new CuiRectTransformComponent {
                            AnchorMin = aMin,
                            AnchorMax = aMax
                        }
                    }
                });
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
                if (image.Image != null && _buildarin.ImageLibrary != null) {
                    cuiRawImageComponent = new CuiRawImageComponent {
                        Color = color,
                        Png = (string) _buildarin.ImageLibrary.Call("GetImage", image.Image)
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