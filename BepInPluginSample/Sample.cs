using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {
        #region GUI
        public static ManualLogSource logger;

        static Harmony harmony;
        static Harmony harmony2;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isGUIOnKey;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isOpenKey;

        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isOpen;
        private ConfigEntry<float> uiW;
        private ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string title = "";
        public string windowName = ""; // 변수용 
        public string FullName = "Plugin"; // 창 펼쳤을때
        public string ShortName = "P"; // 접었을때

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;
        #endregion

        #region 변수
        // =========================================================

        internal static ConfigEntry<bool> minHp1;
        internal static ConfigEntry<bool> noDead;
        internal static ConfigEntry<bool> noDamage;
        internal static ConfigEntry<bool> noRecovery;
        internal static ConfigEntry<bool> noAP;
        internal static ConfigEntry<bool> addDiscard;
        internal static ConfigEntry<bool> isFogout;
        internal static ConfigEntry<bool> StageArkPartOn;
        internal static ConfigEntry<bool> WaitCount;
        internal static ConfigEntry<bool> WaitCountAdd;
        internal static ConfigEntry<bool> isMaxHpUp;
        internal static ConfigEntry<bool> onPartyInventoryUI;
        internal static ConfigEntry<bool> skillAll;
        internal static ConfigEntry<bool> CheckFullHand;
        //private static ConfigEntry<bool> SkillAdd_Extended;
        // private static ConfigEntry<float> uiW;
        // private static ConfigEntry<float> xpMulti;

        // =========================================================
        #endregion

        static Dictionary<string, string> keyItemNames = new Dictionary<string, string>();
        static Dictionary<string, ItemBase> keyItems = new Dictionary<string, ItemBase>();
        static Dictionary<string, List<string>> items = new Dictionary<string, List<string>>();
        static List<string> itemkeys = new List<string>();
        static string itemkey = "";

        static Dictionary<string, List<string>> rewards = new Dictionary<string, List<string>>();
        static List<string> rewardkeys = new List<string>();
        static string rewardkey = "";



        public void Awake()
        {

            #region GUI
            logger = Logger;
            Logger.LogMessage("Awake");

            isGUIOnKey = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            isOpenKey = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;
            uiW = Config.Bind("GUI", "uiW", 300f);
            uiH = Config.Bind("GUI", "uiH", 600f);

            if (isOpen.Value)
                windowRect = new Rect(Screen.width - 65, 0, uiW.Value, 800);
            else
                windowRect = new Rect(Screen.width - uiW.Value, 0, uiW.Value, 800);

            IsOpen_SettingChanged(null, null);
            #endregion

            #region 변수 설정
            // =========================================================

            skillAll = Config.Bind("game", "skillAll", false);
            minHp1 = Config.Bind("game", "minHp1", false);
            noDead = Config.Bind("game", "noDead", false);
            noDamage = Config.Bind("game", "noDamage", false);
            noRecovery = Config.Bind("game", "noRecovery", false);
            noAP = Config.Bind("game", "noMana", false);
            addDiscard = Config.Bind("game", "addDiscard", false);
            isFogout = Config.Bind("game", "isFogout", false);
            StageArkPartOn = Config.Bind("game", "StageArkPartOn", false);
            WaitCount = Config.Bind("game", "WaitCount", false);
            WaitCountAdd = Config.Bind("game", "WaitCountAdd", false);
            onPartyInventoryUI = Config.Bind("game", "onPartyInventoryUI", false);
            CheckFullHand = Config.Bind("game", "CheckFullHand", false);
            //SkillAdd_Extended = Config.Bind("game", "SkillAdd_Extended", true);
            //isMaxHpUp = Config.Bind("game", "isMaxHpUp", true);
            // xpMulti = Config.Bind("game", "xpMulti", 2f);

            onPartyInventoryUI.SettingChanged += OnPartyInventoryUI_SettingChanged;
            StageArkPartOn.SettingChanged += StageArkPartOn_SettingChanged;
            // =========================================================
            #endregion

            items[""] = new List<string>();
            rewards[""] = new List<string>();

            // InventoryManager.Reward(ItemBase.GetItem(
            items["Item_Consume_"] = new List<string>();
            items["Item_Active_"] = new List<string>();
            items["Item_Equip_Legendary"] = new List<string>();
            items["Item_Equip_Unique"] = new List<string>();
            items["Item_Equip_"] = new List<string>();
            items["Item_Scroll_"] = new List<string>();
            items["Item_Misc_"] = new List<string>();
            items["Item_Passive_"] = new List<string>();
            items["Item_Potions_"] = new List<string>();
            items["Item_Friendship_"] = new List<string>();
            //items["ItemClass_"] = new List<string>();
            items["RandomDrop_"] = new List<string>();

            rewards["Reward_"] = new List<string>();

            itemkeys = items.Keys.ToList();
            rewardkeys = rewards.Keys.ToList();


        }

        private static void OnPartyInventoryUI_SettingChanged(object sender, EventArgs e)
        {
            if (onPartyInventoryUI.Value)
            {
                PartyInventory.Ins.Align.transform.localPosition = new Vector3(1150, 0.6f, 0.5f);
                PartyInventory.InvenM.Align.GetComponent<GridLayoutGroup>().constraintCount = 18;
            }
            else
            {
                PartyInventory.Ins.Align.transform.localPosition = new Vector3(579.7f, 0.6f, 0.5f);
                PartyInventory.InvenM.Align.GetComponent<GridLayoutGroup>().constraintCount = 9;

            }
        }

        private void StageArkPartOn_SettingChanged(object sender, EventArgs e)
        {
            if (PlayData.TSavedata != null)
            {
                PlayData.TSavedata.StageArkPartOn = StageArkPartOn.Value;
            }
        }

        #region GUI
        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = isGUIOnKey.Value.ToString() + "," + isOpenKey.Value.ToString();
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                title = "";
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                windowRect.x += (uiW.Value - 64);
            }
        }
        #endregion

        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Sample));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony2 = Harmony.CreateAndPatchAll(typeof(MySkill));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public void Start()
        {
            MyItemCollection.Init();


            System.Reflection.MemberInfo[] members = typeof(GDEItemKeys).GetMembers();
            foreach (var memberInfo in members)
            {
                //Logger.LogMessage($"Name: {memberInfo.Name}");
                System.Reflection.FieldInfo f = typeof(GDEItemKeys).GetField(memberInfo.Name);

                if (f?.FieldType == typeof(String))
                {
                    // Type: String ; IsPublic: True ; IsStatic: True ;
                    string key = (String)f.GetValue(null);
                    try
                    {
                        //Logger.LogMessage($"Name: {memberInfo.Name} ; Type: {f.FieldType.Name} ; IsPublic: {f.IsPublic} ; IsStatic: {f.IsStatic} ; GetValue: {f.GetValue(null)} ;");

                        if (memberInfo.Name.StartsWith("Item_Consume_")) items["Item_Consume_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Item_Active_")) items["Item_Active_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Item_Scroll_"))
                        {
                            items["Item_Scroll_"].Add(key);
                            keyItems[key] = ItemBase.GetItem(key);
                            ((Item_Scroll)keyItems[key]).Debug = true;
                            keyItemNames[key] = keyItems[key].GetName;
                            continue;
                        }
                        else if (memberInfo.Name.StartsWith("Item_Misc_")) items["Item_Misc_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Item_Passive_")) items["Item_Passive_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Item_Potions_")) items["Item_Potions_"].Add(key);
                        //else if (memberInfo.Name.StartsWith("ItemClass_")) items["ItemClass_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("RandomDrop_")) items["RandomDrop_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Item_Equip_"))
                        {
                            var k = new GDEItem_EquipData(key).Itemclass.Key;
                            if (k == GDEItemKeys.ItemClass_Legendary)
                                items["Item_Equip_Legendary"].Add(key);
                            else if (k == GDEItemKeys.ItemClass_Unique)
                                items["Item_Equip_Unique"].Add(key);
                            else
                                items["Item_Equip_"].Add(key);
                        }
                        else if (memberInfo.Name.StartsWith("Item_Friendship_")) items["Item_Friendship_"].Add(key);
                        else if (memberInfo.Name.StartsWith("Reward_")) rewards["Reward_"].Add(key);
                        else
                        {
                            Logger.LogMessage($"key skip : {key}");
                            continue;
                        }
                        keyItems[key] = ItemBase.GetItem(key);
                        keyItemNames[key] = keyItems[key].GetName;
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e);
                        Logger.LogMessage($"key err  : {key}");
                        keyItemNames[key] = key;
                    }
                }
            }
            foreach (var key in items["Item_Equip_"])
            {

            }
        }

        public void Update()
        {
            #region GUI
            if (isGUIOnKey.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (isOpenKey.Value.IsUp())// 단축키가 일치할때
            {
                if (isGUIOn.Value)
                {
                    isOpen.Value = !isOpen.Value;
                }
                else
                {
                    isGUIOn.Value = true;
                    isOpen.Value = true;
                }
            }
            #endregion
        }

        #region GUI
        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            // 창 나가는거 방지
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }
        #endregion

        public virtual void WindowFunction(int id)
        {
            #region GUI
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
                                        // 라벨 추가
                                        //GUILayout.Label(windowName, GUILayout.Height(20));
                                        // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            if (isOpen.Value) GUILayout.Label(title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { isOpen.Value = !isOpen.Value; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn.Value = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!isOpen.Value) // 닫혔을때
            {
            }
            else // 열렸을때
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                #endregion

                #region 여기에 GUI 항목 작성
                // =========================================================

                // if (GUILayout.Button($"{hpNotChg.Value}")) { hpNotChg.Value = !hpNotChg.Value; }

                // GUILayout.BeginHorizontal();
                // GUILayout.Label($"ammoMulti {ammoMulti.Value}");
                // if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { ammoMulti.Value += 1; }
                // if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { ammoMulti.Value -= 1; }
                // GUILayout.EndHorizontal();

                // =========================================================
                #endregion
                /*
                if (GUILayout.Button($"my cheat"))
                {
                SaveManager.NowData.TimeMoney = 1000;
                PlayData.Gold = 10000;
                PlayData.Soul = 1000;
                PlayData.TSavedata.StageArkPartOn = true;

                List<ItemBase> list12 = new List<ItemBase>();                    
                * for (int i = 0; i < 4; i++)
                {
                list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_ArtifactPlusInven));
                }
                InventoryManager.Reward(list12);

                ItemBaseCheat.ArtifactPlusInvenCheat();
                }
                */

                #region item1
                GUILayout.Label("---  ---");

                if (GUILayout.Button("save")) SaveManager.savemanager.ProgressOneSave();
                if (GUILayout.Button("Fogout")) Fogout();
                if (GUILayout.Button("StageArkPartOn")) PlayData.TSavedata.StageArkPartOn = true; ;

                if (GUILayout.Button("TimeMoney +1000")) { SaveManager.NowData.TimeMoney += 1000; }

                if (GUILayout.Button("gold +100 (LeftShift +10000)")) { if (Input.GetKey(KeyCode.LeftShift)) PlayData.Gold += 10000; else PlayData.Gold += 100; }
                if (GUILayout.Button("Soul +10 (LeftShift +1000)")) { if (Input.GetKey(KeyCode.LeftShift)) PlayData.Soul += 1000; else PlayData.Soul += 10; }
                //if (GUILayout.Button("PlusDiscard +10 (LeftShift +1000)")) { if (Input.GetKey(KeyCode.LeftShift)) BattleSystem.instance.AllyTeam.LucyChar.info.PlusDiscard += 1000;else PlayData.Soul += 10; }

                GUILayout.Label("---  ---");
                if (GUILayout.Button($"ArkPassivePlus {PlayData.TSavedata?.ArkPassivePlus}")) { ItemBaseCheat.ArtifactPlusInvenCheat(); }

                if (GUILayout.Button($"ArtifactPlusInven 8"))
                {
                    ItemBaseCheat.ArtifactPlusInvenReward(8);
                }
                if (GUILayout.Button($"Ark Parts Inven Add my Item \n(need open Inven)"))
                {
                    if (Inven)
                    {
                        ItemBaseCheat.ArtifactPlusInvenCheat();
                        Inven.AddNewItem(ItemBase.GetItem("AlphaBullet"));
                        Inven.AddNewItem(ItemBase.GetItem("AncientShield"));
                        Inven.AddNewItem(ItemBase.GetItem("Bookofmoon"));
                        Inven.AddNewItem(ItemBase.GetItem("Bookofsun"));
                        Inven.AddNewItem(ItemBase.GetItem("BrightShield"));
                        Inven.AddNewItem(ItemBase.GetItem("BronzeMotor"));
                        Inven.AddNewItem(ItemBase.GetItem("Crossoflight"));
                        Inven.AddNewItem(ItemBase.GetItem("FlameBullet"));
                        Inven.AddNewItem(ItemBase.GetItem("FlameOilBarrel"));
                        Inven.AddNewItem(ItemBase.GetItem("Goldleaves"));
                        Inven.AddNewItem(ItemBase.GetItem("HipSack"));
                        Inven.AddNewItem(ItemBase.GetItem("JokerCard"));
                        Inven.AddNewItem(ItemBase.GetItem("MagicBerry"));
                        Inven.AddNewItem(ItemBase.GetItem("MagicLamp"));
                        Inven.AddNewItem(ItemBase.GetItem("Maskofthesun"));
                        Inven.AddNewItem(ItemBase.GetItem("MindsEye"));
                        Inven.AddNewItem(ItemBase.GetItem("MistTotem"));
                        Inven.AddNewItem(ItemBase.GetItem("Obsidian"));
                        Inven.AddNewItem(ItemBase.GetItem("Palette"));
                        Inven.AddNewItem(ItemBase.GetItem("QuickCasting"));
                        Inven.AddNewItem(ItemBase.GetItem("RedBlossoms"));
                        Inven.AddNewItem(ItemBase.GetItem("Shiranui_Relic"));
                        Inven.AddNewItem(ItemBase.GetItem("Spinyblowfish"));
                        Inven.AddNewItem(ItemBase.GetItem("Sunset"));
                        Inven.AddNewItem(ItemBase.GetItem("Superconductor"));
                        Inven.AddNewItem(ItemBase.GetItem("ThornStem"));
                        Inven.AddNewItem(ItemBase.GetItem("ToothBottle"));
                        Inven.AddNewItem(ItemBase.GetItem("Tumble"));
                        Inven.AddNewItem(ItemBase.GetItem("WhiteMoon"));
                        Inven.AddNewItem(ItemBase.GetItem("WitchRelic"));
                        Inven.AddNewItem(ItemBase.GetItem("branche"));
                    }
                }

                if (GUILayout.Button($"Ark Parts Inven Add my file \n(need open Inven)"))
                {
                    List<string> l = new List<string>();
                    FileInfo fi = new FileInfo("Ark.txt");
                    if (fi.Exists)
                    {
                        using (StreamReader sr = new StreamReader("Ark.txt"))
                        {
                            char[] delimiterChars = { ' ' };
                            String line = sr.ReadLine();
                            while (line != null)
                            {
                                if (line.Length == 0) continue;

                                Sample.logger.LogMessage($"read : {line}");
                                string[] n = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                                if (n.Length == 0) continue;

                                var t = n.Last();
                                if (t.Length == 0) continue;
                                Sample.logger.LogMessage($"add  : {t}");
                                l.Add(t);

                                line = sr.ReadLine();
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"need Ark.txt in {System.Environment.CurrentDirectory}");
                    }
                    if (Inven && l.Count > 0)
                    {
                        ItemBaseCheat.ArtifactPlusInvenCheat(l.Count);
                        foreach (var item in l)
                        {
                            try
                            {
                                Inven.AddNewItem(ItemBase.GetItem(item));
                            }
                            catch (Exception e)
                            {
                                Logger.LogError($"err  : {item}");
                                //Logger.LogWarning(item);
                                //Logger.LogError(e);
                            }
                            finally
                            {

                            }
                        }
                    }
                }

                GUILayout.Label("---  ---");
                if (GUILayout.Button($"PassiveReward"))
                {
                    ItemBaseCheat.PassiveReward();
                }

                if (GUILayout.Button($"Item_Passive_ random"))
                    Reward("Item_Passive_");

                GUILayout.Label("---  ---");

                if (GUILayout.Button($"EquipsReward"))
                {
                    ItemBaseCheat.EquipsReward();
                }

                if (GUILayout.Button($"Item_Equip_Legendary random"))
                {
                    Reward("Item_Equip_Legendary");
                }

                if (GUILayout.Button($"Item_Equip_Unique random"))
                {
                    Reward("Item_Equip_Unique");
                }

                if (GUILayout.Button($"ItemReward"))
                {
                    ItemBaseCheat.ItemReward();
                }

                if (GUILayout.Button($"ScrollReward"))
                {
                    ItemBaseCheat.ScrollReward();
                }
                if (GUILayout.Button("get key (LeftShift click = 10)"))
                {
                    ItemBaseCheat.RewardGetItem(GDEItemKeys.Item_Misc_Item_Key);
                }
                if (GUILayout.Button($"ChangeMaxInventoryNum +9"))
                {
                    PartyInventory.InvenM.ChangeMaxInventoryNum(+9);
                    PartyInventory.Init();
                    PartyInventory.Ins.UpdateInvenUI();
                    PartyInventory.Ins.PadLayoutSetting();
                    //MyPartyInventory.PadLayoutSetting();

                }

                GUILayout.Label("---  ---");
                #endregion 1


                GUILayout.Label("=== Skill ===");
                if (GUILayout.Button($"Lucy "))
                {
                    MySkill.Use(false);
                }
                if (GUILayout.Button($"Lucy Draw"))
                {
                    MySkill.Use(true);
                }
                GUILayout.Label("---  ---");
                foreach (BattleAlly battleAlly in PlayData.Battleallys)
                {
                    if (GUILayout.Button($"{battleAlly.Info.Name}"))
                    {
                        MySkill.Use(battleAlly);
                    }
                }
                GUILayout.Label("--- Rare ---");
                foreach (BattleAlly battleAlly in PlayData.Battleallys)
                {
                    if (GUILayout.Button($"{battleAlly.Info.Name}"))
                    {
                        MySkill.Use(battleAlly, true);
                    }
                }
                GUILayout.Label("=== Skill ===");
                GUILayout.Label("=== Skill Extended ===");
                /*
                if (GUILayout.Button($"Select"))
                {
                CelestialCheat.Skill_ExtendedSelect2();
                }*/
                foreach (BattleAlly battleAlly in PlayData.Battleallys)
                {
                    if (GUILayout.Button($"{battleAlly.Info.Name} Give")) CelestialCheat.Skill_ExtendedSelect_Give(battleAlly);
                    GUILayout.Label("--- del ---");
                    foreach (Skill enforceSkill in battleAlly.Skills)
                    {
                        if (enforceSkill.CharinfoSkilldata.SKillExtended != null)
                            if (GUILayout.Button(
                                $" {enforceSkill.CharinfoSkilldata.Skill.Name}\n{enforceSkill.CharinfoSkilldata.SKillExtended.ExtendedName()}"
                                , GUILayout.Width(250)
                                ))
                            {
                                enforceSkill.CharinfoSkilldata.SKillExtended = null;
                            }
                    }
                    GUILayout.Label("---  ---");
                }

                if (GUILayout.Button($"auto all set"))
                {
                    CelestialCheat.Skill_ExtendedSelect_random();
                }

                if (GUILayout.Button($"all del"))
                {
                    CelestialCheat.Skill_ExtendedDel();
                }
                GUILayout.Label("=== Skill Extended ===");


                #region item2

                GUILayout.Label("---  ---");

                if (GUILayout.Button($"select my Equip Legendary"))
                {
                    List<ItemBase> list12 = new List<ItemBase>();
                    foreach (var item in PlayData.TSavedata.EquipList_Legendary)
                    {
                        list12.Add(ItemBase.GetItem(item));
                    }
                    if (list12.Count > 0)
                    {
                        ItemBaseCheat.SelectItemUI(list12);
                    }
                }

                if (GUILayout.Button($"select my Equip Unique"))
                {
                    List<ItemBase> list12 = new List<ItemBase>();
                    foreach (var item in PlayData.TSavedata.EquipList_Unique)
                    {
                        list12.Add(ItemBase.GetItem(item));
                    }
                    if (list12.Count > 0)
                    {
                        ItemBaseCheat.SelectItemUI(list12);
                    }
                }

                #endregion

                #region onoff

                GUILayout.Label("=== on/off ===");

                if (GUILayout.Button($"min Hp 1 {minHp1.Value}")) { minHp1.Value = !minHp1.Value; }
                if (GUILayout.Button($"no Dead {noDead.Value}")) { noDead.Value = !noDead.Value; }
                if (GUILayout.Button($"1 Damage {noDamage.Value}")) { noDamage.Value = !noDamage.Value; }
                if (GUILayout.Button($"Recovery max {noRecovery.Value}")) { noRecovery.Value = !noRecovery.Value; }
                if (GUILayout.Button($"Mana max {noAP.Value}")) { noAP.Value = !noAP.Value; }
                if (GUILayout.Button($"MyTurn add Discard {addDiscard.Value}")) { addDiscard.Value = !addDiscard.Value; }
                if (GUILayout.Button($"not isFogout {isFogout.Value}")) { isFogout.Value = !isFogout.Value; }
                if (GUILayout.Button($"StageArkPartOn {StageArkPartOn.Value}")) { StageArkPartOn.Value = !StageArkPartOn.Value; }
                if (GUILayout.Button($"WaitCount {WaitCount.Value}")) { WaitCount.Value = !WaitCount.Value; }
                if (GUILayout.Button($"WaitCountAdd {WaitCountAdd.Value}")) { WaitCountAdd.Value = !WaitCountAdd.Value; }
                if (GUILayout.Button($"onPartyInventoryUI {onPartyInventoryUI.Value}")) { onPartyInventoryUI.Value = !onPartyInventoryUI.Value; }
                if (GUILayout.Button($"all skill show {skillAll.Value}")) { skillAll.Value = !skillAll.Value; }
                if (GUILayout.Button($"CheckFullHand {CheckFullHand.Value}")) { CheckFullHand.Value = !CheckFullHand.Value; }
                //if (GUILayout.Button($"SkillAdd_Extended {SkillAdd_Extended.Value}")) { SkillAdd_Extended.Value = !SkillAdd_Extended.Value; }
                //if (GUILayout.Button($"isMaxHpUp {isMaxHpUp.Value}")) { isMaxHpUp.Value = !isMaxHpUp.Value; }
                GUILayout.Label("=== on/off ===");

                #endregion
                /*
                if (GUILayout.Button($"select my Equip "))
                {
                UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(ItemBaseCheat.Equips());
                }
                */

                //if (GUILayout.Button($"get my ArtifactPlusInven {items["Item_Passive_"].Count - 4}"))

                #region debug
                GUILayout.Label("---  ---");

                if (GUILayout.Button("GetPassiveRandom"))
                {
                    ItemBaseCheat.GetPassiveRandom();
                }
                /*
                if (GUILayout.Button("get skill"))
                {
                PlayData.TSavedata.LucySkills.Add(GDEItemKeys.Skill_S_Lucy_25);
                ChildClear.Clear(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().LucySkillAlign);
                foreach (string key in PlayData.TSavedata.LucySkills)
                {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().SkillView);
                gameObject.transform.SetParent(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().LucySkillAlign);
                Misc.UIInit(gameObject);
                gameObject.GetComponent<SkillButtonMain>().Skillbutton.InputData(Skill.TempSkill(key, PlayData.BattleDummy, PlayData.TempBattleTeam), null, false);
                }
                }
                */
                if (GUILayout.Button("potions"))
                {
                    List<string> list9 = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Potions, out list9);
                    InventoryManager.Reward(
                        new List<ItemBase>
                        {
                        ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_heal),
                        ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_weak),
                        ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_holywater)
                        }
                    );
                }


                if (GUILayout.Button("skill book 10"))
                {
                    InventoryManager.Reward(
                        new List<ItemBase>
                        {
                        ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 10)
                        }
                    );
                }

                if (GUILayout.Button("Jar*2 SmallReward Sou*10"))
                {
                    List<ItemBase> list12 = new List<ItemBase>();
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_SmallReward, false));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Soul, 10));
                    InventoryManager.Reward(list12);
                }


                if (GUILayout.Button("Reward_Ancient_Chest4"))
                {
                    Item_Equip item = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    Item_Equip item_Equip = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    Item_Equip item2 = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    List<ItemBase> list10 = new List<ItemBase>();
                    item_Equip.Curse = EquipCurse.NewCurse(item_Equip, GDEItemKeys.CurseList_Curse_powerfulCurse);
                    list10.Add(item);
                    list10.Add(item_Equip);
                    list10.Add(item2);
                    InventoryManager.Reward(list10);
                }


                if (GUILayout.Button("GoldenApple, GetPotion"))
                {
                    List<ItemBase> list11 = new List<ItemBase>();
                    for (int num2 = 0; num2 < 10; num2++)
                    {
                        list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                    }
                    InventoryManager.Reward(list11);
                    InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Consume_GoldenApple));
                }

                if (GUILayout.Button("skill book 3"))
                {
                    InventoryManager.Reward(new List<ItemBase>
{
ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Lucy_17)),
ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_7)),
ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_36))
});
                }

                if (GUILayout.Button("reward5 JokerCard"))
                {
                    InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Passive_JokerCard));
                }

                if (GUILayout.Button("Record_7"))
                {
                    InventoryManager.Reward(new List<ItemBase>
{
ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_7)
});
                }
                if (GUILayout.Button("Record_8"))
                {
                    InventoryManager.Reward(new List<ItemBase>
{
ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_8)
});
                }
                if (GUILayout.Button("Record_9"))
                {
                    InventoryManager.Reward(new List<ItemBase>
{
ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_9)
});
                }

                if (GUILayout.Button("allevent"))
                {
                    List<string> eventList = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.RandomEvent, out eventList);
                    FieldEventSelect.FieldEventSelectOpen(eventList, null, null, true);
                }


                if (GUILayout.Button("roulette"))
                {
                    List<ItemBase> list = new List<ItemBase>();
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SmallBarrierMachine));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_GasMask));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Active_PotLid));
                    UIManager.InstantiateActiveAddressable(UIManager.inst.AR_RouletteUI, AddressableLoadManager.ManageType.Battle).GetComponent<UI_MiniGame_Roulette>().Init();
                }

                if (GUILayout.Button("stageevent"))
                {
                    List<string> list2 = new List<string>();
                    foreach (GDERandomEventData gderandomEventData in StageSystem.instance.Map.StageData.RandomEventList)
                    {
                        list2.Add(gderandomEventData.Key);
                    }
                    FieldEventSelect.FieldEventSelectOpen(list2, null, null, true);
                }

                if (GUILayout.Button("equipget"))
                {
                    InventoryManager.Reward(new List<ItemBase>
{
ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato),
ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato_0),
ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato_1),
ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb),
ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb_0),
ItemBase.GetItem(GDEItemKeys.Item_Passive_EndlessSoul)
});
                }

                if (GUILayout.Button("fastrun"))
                {
                    StageSystem.instance.Player.GetComponent<PlayerController>().FastRun();
                }

                if (GUILayout.Button("fly"))
                {
                    StageSystem.instance.Player.GetComponent<PlayerController>().Fly();
                }
                #endregion

                GUILayout.Label("--- PlayData ---");

                if (PlayData.TSavedata != null)
                {
                    if (GUILayout.Button($"all maxhp +500 ;"))
                        foreach (var c in PlayData.TSavedata.Party)
                            c.OriginStat.maxhp += 500;

                    foreach (var c in PlayData.TSavedata.Party)
                    {
                        if (GUILayout.Button($"{c.Name} maxhp +500 ; {c.OriginStat.maxhp}")) { c.OriginStat.maxhp += 500; }
                    }
                }
                /*
                GUILayout.Label("--- FieldSystem ---");

                if (FieldSystem.instance != null)
                {

                }
                */
                GUILayout.Label("--- BattleSystem ---");

                if (BattleSystem.instance != null)
                {
                    var allyTeam = BattleSystem.instance.AllyTeam;
                    if (allyTeam != null)
                    {
                        GUILayout.Label("AllyTeam");

                        if (GUILayout.Button($"Mana {allyTeam.AP} +10")) { allyTeam.AP += 10; }

                        if (GUILayout.Button($"ActionCount =1 ; {allyTeam.LucyAlly.ActionCount}")) { allyTeam.LucyAlly.ActionCount = 1; }
                        if (GUILayout.Button($"Overload =1 ; {allyTeam.LucyAlly.Overload}")) { allyTeam.LucyAlly.Overload = 1; }
                        if (GUILayout.Button($"ActionNum =0 ; {allyTeam.LucyAlly.ActionNum}")) { allyTeam.LucyAlly.ActionNum = 0; }

                        if (GUILayout.Button($"UsedDeckToDeckNum =0 ; {allyTeam.UsedDeckToDeckNum}")) { allyTeam.UsedDeckToDeckNum = 0; }
                        if (GUILayout.Button($"Discard Count  ; ")) { allyTeam.DiscardCount = allyTeam.GetDiscardCount + 10; }
                        if (GUILayout.Button($"PartySpeed ={1 + PlayData.TSavedata._PartySpeed} ; {PlayData.TSavedata._PartySpeed}")) { PlayData.TSavedata._PartySpeed = 1 + PlayData.TSavedata._PartySpeed; }
                        if (GUILayout.Button($"WaitCount ={1 + allyTeam.WaitCount} ; {allyTeam.WaitCount}")) { allyTeam.WaitCount = 1 + allyTeam.WaitCount; }

                        GUILayout.Label($"BattleChar count {allyTeam.AliveChars.Count}");

                        //if (GUILayout.Button($"maxhp +1000 ;")) foreach (BattleChar c in allyTeam.AliveChars) c.Info.OriginStat.maxhp += 1000;
                        if (GUILayout.Button($"HP = /=2 ; ")) foreach (BattleChar c in allyTeam.AliveChars) c.HP = c.Info.get_stat.maxhp / 2;

                        foreach (BattleChar c in allyTeam.AliveChars)
                        {
                            GUILayout.Label($"{c.Info.Name}");
                            //if (GUILayout.Button($"maxhp +1000 ; {c.Info.OriginStat.maxhp}")) { c.Info.OriginStat.maxhp += 1000; }
                            if (GUILayout.Button($"HP +100; {c.HP}")) { c.HP += 100; }
                            if (GUILayout.Button($"HP =-1; {c.HP}")) { c.HP = -1; c.Recovery += 100; }
                            if (GUILayout.Button($"Recovery +100; {c.Recovery} ")) { c.Recovery += 100; }
                            if (GUILayout.Button($"HP Recovery +100")) { c.HP += 100; c.Recovery += 100; }
                            if (GUILayout.Button($"ActionCount =1; {c.ActionCount}")) { c.ActionCount = 1; }
                            if (GUILayout.Button($"Overload =1; {c.Overload}")) { c.Overload = 1; }
                            if (GUILayout.Button($"ActionNum =0; {c.ActionNum}")) { c.ActionNum = 0; }
                            if (GUILayout.Button($"SkillUseDraw =false; {c.SkillUseDraw}")) { c.SkillUseDraw = false; }
                        }
                    }
                    else
                    {
                        GUILayout.Label("no AllyTeam");
                    }

                    //if (BattleSystem.instance.AllyList[0] != null)
                    //else
                    //{
                    //    GUILayout.Label("no BattleSystem");
                    //}
                }

                GUILayout.Label("====== item ======");
                foreach (var i in itemkeys)
                {
                    if (GUILayout.Button($"{i}")) { itemkey = i; }
                }
                GUILayout.Label($"--- {itemkey} ---");
                if (GUILayout.Button($"get random 16"))
                {
                    string s;
                    var _sitems = new List<string>(items[itemkey]);
                    List<ItemBase> _items = new List<ItemBase>();
                    int c = items[itemkey].Count > 16 ? 16 : items[itemkey].Count;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        for (int i = 0; i < c; i++)
                        {
                            s = _sitems.Random();
                            _items.Add(ItemBase.GetItem(s, 9));
                            _sitems.Remove(s);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < c; i++)
                        {
                            s = _sitems.Random();
                            _items.Add(ItemBase.GetItem(s));
                            _sitems.Remove(s);
                        }
                    }
                    InventoryManager.Reward(_items);
                }

                if (GUILayout.Button($"get all {items[itemkey].Count} : bug!"))
                {
                    List<ItemBase> list = new List<ItemBase>();
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        foreach (var i in items[itemkey])
                        {
                            list.Add(ItemBase.GetItem(i, 10));
                            if (list.Count >= 16)
                            {
                                InventoryManager.Reward(list);
                                list.Clear();
                            }
                        }
                    }
                    else
                    {
                        foreach (var i in items[itemkey])
                        {
                            list.Add(ItemBase.GetItem(i));
                            if (list.Count >= 16)
                            {
                                InventoryManager.Reward(list);
                                list.Clear();
                            }
                        }
                    }
                    if (list.Count > 0)
                        InventoryManager.Reward(list);
                }

                GUILayout.Label($"--- LeftShift click = 10 ---");
                foreach (var i in items[itemkey])
                {
                    if (GUILayout.Button($"{keyItemNames[i]}"))
                    {
                        ItemBaseCheat.RewardGetItem(i);
                    }
                }
                GUILayout.Label("====== item ======");


                GUILayout.Label("====== reward ======");

                foreach (var i in rewardkeys)
                {
                    if (GUILayout.Button($"{i}")) { rewardkey = i; }
                }
                GUILayout.Label($"--- {rewardkey} ---");
                GUILayout.Label($"--- LeftShift click = 8 ---");
                foreach (var i in rewards[rewardkey])
                {
                    if (GUILayout.Button($"{i}"))
                    {
                        ItemBaseCheat.Reward(i);
                    }
                }
                GUILayout.Label("====== reward ======");

                if (GUILayout.Button($"Friendship info"))
                {
                    List<string> list = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Character, out list);
                    for (int i = 0; i < list.Count; i++)
                    {
                        GDECharacterData g = new GDECharacterData(list[i]);
                        if (g.CanFriendShip && !(list[i] == GDEItemKeys.Character_Phoenix) && !(list[i] == GDEItemKeys.Character_TW_Blue) && Misc.IsOriginPlaybleChar(list[i]))
                        {
                            var c = SaveManager.NowData.statistics.GetCharData(list[i]);
                            //SaveManager.NowData.statistics.GetCharData(list[i]).FriendshipLV
                            logger.LogWarning($"{c.FriendshipLV} ; {c.FriendshipAmount} ; {g.name}");
                        }
                    }
                }
                if (GUILayout.Button($"Friendship unlook"))
                {
                    List<string> list = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Character, out list);
                    for (int i = 0; i < list.Count; i++)
                    {
                        GDECharacterData g = new GDECharacterData(list[i]);
                        if (g.CanFriendShip && !(list[i] == GDEItemKeys.Character_Phoenix) && !(list[i] == GDEItemKeys.Character_TW_Blue) && Misc.IsOriginPlaybleChar(list[i]))
                        {
                            var c = SaveManager.NowData.statistics.GetCharData(list[i]);
                            //SaveManager.NowData.statistics.GetCharData(list[i]).FriendshipLV
                            c.FriendshipLV = 4;
                            c.FriendshipAmount = 0;
                            logger.LogWarning($"{c.FriendshipLV} ; {c.FriendshipAmount} ; {g.name}");
                        }
                    }
                }
                #region Debug
                if (GUILayout.Button($"Debug Mode {SaveManager.savemanager._DebugMode}"))
                    SaveManager.savemanager._DebugMode = !SaveManager.savemanager._DebugMode;
                #endregion

                if (GUILayout.Button($"get my info"))
                {
                    CelestialCheat.info();
                }
                if (GUILayout.Button($"ark inven info"))
                {
                    foreach (var item in Inven?.InventoryItems)
                    {
                        logger.LogWarning($"{item?.itemkey}");
                    }
                }
                #region GUI
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
            #endregion
        }



        private static void Reward(string s = "", int c = 24)
        {
            logger.LogWarning("Reward");
            if (items[s].Count > 0)
            {
                List<ItemBase> list = new List<ItemBase>();
                for (int i = 0; i < c; i++)
                {
                    list.Add(ItemBase.GetItem(items[s].Random()));
                }
                InventoryManager.Reward(list);
                //list.Clear();
            }
            logger.LogWarning("Reward");
        }

        private static void Fogout()
        {
            if (StageSystem.instance.gameObject.activeInHierarchy && StageSystem.instance != null && StageSystem.instance.Map != null)
            {
                //StageSystem.instance.Fogout(false);
                StageSystem.instance.Fogout(true);
            }
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
            harmony2?.UnpatchSelf();
        }

        #region Harmony
        // ====================== 하모니 패치 샘플 ===================================

        [HarmonyPatch(
            typeof(BattleTeam), "CheckFullHand"        
            )]
        [HarmonyPrefix]
        public static bool CheckFullHandPre(BattleTeam __instance)
        {
            //logger.LogMessage($"CheckFullHand"); ;
            if (CheckFullHand.Value)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(BattleAlly), "Damage",
        typeof(BattleChar), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void DamagePr(BattleAlly __instance, ref int Dmg)
        {
            logger.LogMessage($"DamagePr {Dmg} ; {__instance.HP}; {__instance.Info.Name}"); ;
            if (noDamage.Value)
            {
                Dmg = 0;
            }
        }

        [HarmonyPatch(typeof(BattleAlly), "Damage",
        typeof(BattleChar), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static void DamagePo(BattleAlly __instance, ref int Dmg)
        {
            logger.LogMessage($"DamagePo {Dmg} ; {__instance.HP}; {__instance.Info.Name}");
            if (noRecovery.Value)
            {
                __instance.Recovery = __instance.Info.get_stat.maxhp;
            }
            if (minHp1.Value && __instance.HP < 1)
            {
                __instance.HP = 1;
            }
        }
        // public virtual void Dead(bool notdeadeffect = false, bool NoTimeSlow = false)
        [HarmonyPatch(typeof(BattleAlly), "Dead", typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static bool Dead(BattleAlly __instance, bool notdeadeffect, bool NoTimeSlow)
        {
            logger.LogMessage($"Dead {notdeadeffect}; {NoTimeSlow} ; {__instance.HP}; {__instance.Info.Name}");
            if (minHp1.Value && __instance.HP < 1)
            {
                __instance.HP = 1;
            }
            if (noDead.Value)
            {
                return false;
            }
            return true;
        }
        //public void OnlyDamage(int Dmg, bool Weak, bool IgnoreHealPro = false, BattleChar User = null, bool Cri = false)
        [HarmonyPatch(typeof(BattleChar), "OnlyDamage", typeof(int), typeof(bool), typeof(bool), typeof(BattleChar), typeof(bool))]
        [HarmonyPostfix]
        public static void OnlyDamage(BattleChar __instance, int Dmg)
        {
            logger.LogMessage($"OnlyDamage  {Dmg} ; {__instance.HP}; {__instance.Info.Name}");
            if (minHp1.Value && __instance.Info.Ally && __instance.HP < 1)
            {
                __instance.HP = 1;
            }
        }
        /*
        */
        [HarmonyPatch(typeof(BattleTeam), "AP", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetAP(BattleTeam __instance, ref int __0)
        {
            logger.LogMessage($"SetAP");
            if (!noAP.Value)
            {
                return;
            }
            __0 = __instance.MAXAP;
        }


        [HarmonyPatch(typeof(BattleTeam), "GetDiscardCount", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool GetDiscardCount(ref int __result)//BattleTeam __instance,
        {
            //logger.LogMessage($"GetDiscardCount");
            if (!addDiscard.Value)
            {
                return true;
            }
            __result = 99;
            return false;
        }

        /*
        [HarmonyPatch(typeof(InventoryManager), "Reward", typeof(List<ItemBase>))]
        [HarmonyPrefix]
        public static void Reward(List<ItemBase> Items)//InventoryManager __instance,
        {
            foreach (var item in Items)
            {
                logger.LogMessage($"Reward1 : {item.itemkey}");
            }
        }
        [HarmonyPatch(typeof(InventoryManager), "Reward", typeof(ItemBase))]
        [HarmonyPrefix]
        public static void Reward(ItemBase Item)//InventoryManager __instance,
        {
            logger.LogMessage($"Reward2 : {Item.itemkey}");
        }
        */

        [HarmonyPatch(typeof(FieldSystem), "StageStart", typeof(string))]
        [HarmonyPostfix]
        public static void StageStart()//InventoryManager __instance, string StageKey
        {
            logger.LogMessage($"StageStart");
            if (isFogout.Value)
            {
                //return;
                Fogout();
            }
            if (StageArkPartOn.Value)
            {
                //return;
                PlayData.TSavedata.StageArkPartOn = true;
            }
        }

        /*
        [HarmonyPatch(typeof(FieldSystem), "NextStage")]
        [HarmonyPatch(typeof(FieldSystem), "ClearMap")]
        [HarmonyPostfix]
        public static void NextStage()//InventoryManager __instance, string StageKey
        {
            //logger.LogMessage($"Reward2 : {Item.GetName}");
        }
        */

        [HarmonyPatch(typeof(WaitButton), "WaitAct")]
        [HarmonyPostfix]
        public static void WaitAct()//InventoryManager __instance, string StageKey
        {
            logger.LogMessage($"WaitAct");
            if (!WaitCount.Value)
            {
                return;
            }
            BattleSystem.instance.AllyTeam.WaitCount++;
        }

        [HarmonyPatch(typeof(BattleTeam), "MyTurn")]
        [HarmonyPostfix]
        public static void MyTurn()//InventoryManager __instance, string StageKey
        {
            logger.LogMessage($"MyTurn");
            if (!WaitCountAdd.Value)
            {
                return;
            }
            BattleSystem.instance.AllyTeam.WaitCount += 9;
        }

        // public void SkillAdd(GDESkillData Data, Skill_Extended SkillEn = null)
        [HarmonyPatch(typeof(UI_DLCNotice), "Update")]
        [HarmonyPostfix]
        public static void Update(UI_DLCNotice __instance)//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"DLC !!");
            __instance.SelfDestroy();
        }

        static ArkPartsInven Inven = null;
        [HarmonyPatch(typeof(ArkPartsUI), "Start")]
        [HarmonyPostfix]
        public static void ArkPartsUI(ArkPartsUI __instance)//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"ArkPartsUI");
            Inven = __instance.Inven;
        }

        [HarmonyPatch(typeof(ArkPartsInven), "Start")]
        [HarmonyPostfix]
        public static void ArkPartsInven(ArkPartsInven __instance)//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"ArkPartsInven");
            Inven = __instance;
        }

        [HarmonyPatch(typeof(ItemCollection), "Start")]
        [HarmonyPostfix]
        public static void ItemCollection(ItemCollection __instance)//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"ItemCollection");
            MyItemCollection.itemCollection = __instance;
        }

        [HarmonyPatch(typeof(ArkPartsInven), "AddNewItem")]
        [HarmonyPostfix]
        public static void AddNewItem(ArkPartsInven __instance, int ItemNum, ItemBase Item)//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"AddNewItem : {Item.itemkey}");
        }

        //[HarmonyPatch(typeof(PartyInventory), "PadLayoutSetting")]
        //[HarmonyPrefix]
        public static bool PadLayoutSetting()//InventoryManager __instance, string StageKey
        {
            logger.LogWarning($"PadLayoutSetting");
            if (onPartyInventoryUI.Value)
            {
                {
                    {
                        if (PartyInventory.Ins.InvenLayout != null && PartyInventory.Ins.InvenLayout.Targets[0] != null)
                        {
                            return false;
                        }
                        PartyInventory.Ins.InvenLayout = new PadLayout();
                        PartyInventory.Ins.InvenLayout.ColumnNum = 18;
                        PartyInventory.Ins.InvenLayout.Index = 0;
                        PartyInventory.Ins.InvenLayout.Size = new Vector2(80f, 80f);
                        for (int i = 0; i < PartyInventory.InvenM.Align.transform.childCount; i++)
                        {
                            PartyInventory.Ins.InvenLayout.Targets.Add(PartyInventory.InvenM.Align.transform.GetChild(i).GetComponent<RectTransform>());
                        }
                        PartyInventory.Ins.InvenLayout.TargetType = typeof(ItemSlot);
                        PartyInventory.Ins.InvenLayout.LayoutType = typeof(PartyInventory);
                        GamepadManager.Add(PartyInventory.Ins.InvenLayout, false);
                        for (int j = 0; j < PlayData.TSavedata.Passive_Itembase.Count; j++)
                        {
                            if (PlayData.TSavedata.Passive_Itembase[j] != null)
                            {
                                PartyInventory.Ins.PassiveLayoutUpdate = true;
                                break;
                            }
                        }
                        if (PartyInventory.Ins.HopeLevel1.activeInHierarchy || PartyInventory.Ins.HopeLevel2.activeInHierarchy || PartyInventory.Ins.HopeLevel3.activeInHierarchy)
                        {
                            PartyInventory.Ins.PassiveLayoutUpdate = true;
                        }
                        if (PartyInventory.Ins.SpruleIcon.activeInHierarchy)
                        {
                            PartyInventory.Ins.PassiveLayoutUpdate = true;
                        }
                        if (PartyInventory.Ins.CrimsonIcon.activeInHierarchy)
                        {
                            PartyInventory.Ins.PassiveLayoutUpdate = true;
                        }
                        if (PartyInventory.Ins.BMistIcon.activeInHierarchy)
                        {
                            PartyInventory.Ins.PassiveLayoutUpdate = true;
                        }
                    }
                }
                OnPartyInventoryUI_SettingChanged(null, null);
                return false;
            }
            return true;
        }

        #endregion
    }
}
