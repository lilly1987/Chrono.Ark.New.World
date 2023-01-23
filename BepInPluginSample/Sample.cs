using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {
        #region GUI
        public static ManualLogSource logger;

        static Harmony harmony;

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

        private static ConfigEntry<bool> noDamage;
        private static ConfigEntry<bool> noAP;
        private static ConfigEntry<bool> addDiscard;
        private static ConfigEntry<bool> isFogout;
        private static ConfigEntry<bool> StageArkPartOn;
        private static ConfigEntry<bool> WaitCount;
        private static ConfigEntry<bool> WaitCountAdd;
        //private static ConfigEntry<bool> isMaxHpUp;
        // private static ConfigEntry<float> uiW;
        // private static ConfigEntry<float> xpMulti;

        // =========================================================
        #endregion

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

            noDamage = Config.Bind("game", "noDamage", true);
            noAP = Config.Bind("game", "noMana", true);
            addDiscard = Config.Bind("game", "addDiscard", true);
            isFogout = Config.Bind("game", "isFogout", true);
            StageArkPartOn = Config.Bind("game", "StageArkPartOn", true);
            WaitCount = Config.Bind("game", "WaitCount", true);
            WaitCountAdd = Config.Bind("game", "WaitCountAdd", true);
            //isMaxHpUp = Config.Bind("game", "isMaxHpUp", true);
            // xpMulti = Config.Bind("game", "xpMulti", 2f);

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
            //items["ItemClass_"] = new List<string>();
            items["RandomDrop_"] = new List<string>();

            rewards["Reward_"] = new List<string>();

            itemkeys = items.Keys.ToList();
            rewardkeys = rewards.Keys.ToList();

            System.Reflection.MemberInfo[] members = typeof(GDEItemKeys).GetMembers();
            foreach (var memberInfo in members)
            {
                //Logger.LogMessage($"Name: {memberInfo.Name}");
                try
                {
                    System.Reflection.FieldInfo f = typeof(GDEItemKeys).GetField(memberInfo.Name);

                    // Type: String ; IsPublic: True ; IsStatic: True ;
                    if (f?.FieldType == typeof(String))
                    {
                        //Logger.LogMessage($"Name: {memberInfo.Name} ; Type: {f.FieldType.Name} ; IsPublic: {f.IsPublic} ; IsStatic: {f.IsStatic} ; GetValue: {f.GetValue(null)} ;");

                        if (memberInfo.Name.StartsWith("Item_Consume_")) items["Item_Consume_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Active_")) items["Item_Active_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Scroll_")) items["Item_Scroll_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Misc_")) items["Item_Misc_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Passive_")) items["Item_Passive_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Potions_")) items["Item_Potions_"].Add((String)(f.GetValue(null)));
                        //else if (memberInfo.Name.StartsWith("ItemClass_")) items["ItemClass_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("RandomDrop_")) items["RandomDrop_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Reward_")) rewards["Reward_"].Add((String)(f.GetValue(null)));

                        else if (memberInfo.Name.StartsWith("Item_Equip_"))
                        {
                            string key = (String)f.GetValue(null);/*
                            // itemBase.InputInfo(key);
                            var k = new GDEItem_EquipData(key).Itemclass.Key;
                            if (k == GDEItemKeys.ItemClass_Legendary)
                                items["Item_Equip_Legendary"].Add(key);
                            else if (k == GDEItemKeys.ItemClass_Unique)
                                items["Item_Equip_Unique"].Add(key);
                            else*/
                                items["Item_Equip_"].Add(key);

                        }


                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e);
                }
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
        }

        public void Start()
        {
            foreach (var key in items["Item_Equip_"])
            {
                var k = new GDEItem_EquipData(key).Itemclass.Key;
                if (k == GDEItemKeys.ItemClass_Legendary)
                    items["Item_Equip_Legendary"].Add(key);
                else if (k == GDEItemKeys.ItemClass_Unique)
                    items["Item_Equip_Unique"].Add(key);
                //else
                //    items["Item_Equip_"].Add(key);
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

                if (GUILayout.Button($"my cheat"))
                {
                    SaveManager.NowData.TimeMoney = 1000;
                    PlayData.Gold = 10000;
                    PlayData.Soul = 1000;
                    PlayData.TSavedata.StageArkPartOn = true;                    
                    /*
                    List<ItemBase> list12 = new List<ItemBase>();                    
                     * for (int i = 0; i < 4; i++)
                    {
                        list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_ArtifactPlusInven));
                    }
                    InventoryManager.Reward(list12);
                    */
                    ArtifactPlusInven();
                }

                if (GUILayout.Button($"my cheat Passive"))
                    Myitem("Item_Passive_");

                if (GUILayout.Button($"my cheat Legendary"))
                { 
                    Myitem("Item_Equip_Legendary");
                }
                if (GUILayout.Button($"my cheat Unique"))
                { 
                    Myitem("Item_Equip_Unique");
                }

                if (GUILayout.Button($"my cheat Item"))
                {

                    List<ItemBase> list12 = new List<ItemBase>();

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_Celestial, 30));

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter_Rare, 5));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 5 * 4));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookLucy, 5));

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Enchant, 9));
                    //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Transfer, 9));

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key, 9));

                    InventoryManager.Reward(list12);
                }


                if (GUILayout.Button($"get my Passive"))
                {
                    List<ItemBase> list12 = new List<ItemBase>();

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Bookofmoon));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Bookofsun));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_BronzeMotor));
                    //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_CuteComputer));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_GolemRelic));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_JokerCard));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_MagicLamp));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_MagicBerry));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Memoryfragment));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_ShadowOrb));

                    InventoryManager.Reward(list12);
                }


                GUILayout.Label("---  ---");

                if (GUILayout.Button($"0 Damage {noDamage.Value}")) { noDamage.Value = !noDamage.Value; }
                if (GUILayout.Button($"0 Mna {noAP.Value}")) { noAP.Value = !noAP.Value; }
                if (GUILayout.Button($"MyTurn add Discard 10 {addDiscard.Value}")) { addDiscard.Value = !addDiscard.Value; }
                if (GUILayout.Button($"isFogout {isFogout.Value}")) { isFogout.Value = !isFogout.Value; }
                if (GUILayout.Button($"StageArkPartOn {StageArkPartOn.Value}")) { StageArkPartOn.Value = !StageArkPartOn.Value; }
                if (GUILayout.Button($"WaitCount {WaitCount.Value}")) { WaitCount.Value = !WaitCount.Value; }
                if (GUILayout.Button($"WaitCountAdd {WaitCountAdd.Value}")) { WaitCountAdd.Value = !WaitCountAdd.Value; }
                //if (GUILayout.Button($"isMaxHpUp {isMaxHpUp.Value}")) { isMaxHpUp.Value = !isMaxHpUp.Value; }

                GUILayout.Label("---  ---");

                if (GUILayout.Button("save")) SaveManager.savemanager.ProgressOneSave();
                if (GUILayout.Button("Mapping")) Fogout();
                if (GUILayout.Button("StageArkPartOn")) PlayData.TSavedata.StageArkPartOn = true; ;

                if (GUILayout.Button("TimeMoney +1000")) { SaveManager.NowData.TimeMoney += 1000; }

                if (GUILayout.Button("gold +1000")) { PlayData.Gold += 1000; }
                if (GUILayout.Button("gold *10")) { PlayData.Gold *= 10; }

                if (GUILayout.Button("Soul +1000")) { PlayData.Soul += 1000; }
                if (GUILayout.Button("Soul *10")) { PlayData.Soul *= 10; }

                if (GUILayout.Button($"ArkPassivePlus =8 {PlayData.TSavedata?.ArkPassivePlus}")) { ArtifactPlusInven(); }

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
                        UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(list12);
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
                        UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(list12);
                    }
                }

                if (GUILayout.Button($"select my Equip "))
                {
                    UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(Equips());
                }

                if (GUILayout.Button($"get my Equip"))
                {
                    InventoryManager.Reward(Equips());
                }

                if (GUILayout.Button($"get my Item"))
                {

                    List<ItemBase> list12 = new List<ItemBase>();

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_Celestial, 30));

                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter_Rare, 5));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 5 * 4));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookLucy, 5));

                    InventoryManager.Reward(list12);
                }

                //if (GUILayout.Button($"get my ArtifactPlusInven {items["Item_Passive_"].Count - 4}"))
                if (GUILayout.Button($"get my ArtifactPlusInven 4"))
                {

                    List<ItemBase> list12 = new List<ItemBase>();

                    //for (int i = 0; i < items["Item_Passive_"].Count - 4; i++)
                    for (int i = 0; i < 4; i++)
                    {
                        list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_ArtifactPlusInven));
                    }

                    InventoryManager.Reward(list12);
                }

                if (GUILayout.Button($"get my Scroll"))
                {

                    List<ItemBase> list12 = new List<ItemBase>();
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Enchant, 9));
                    //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Mapping, 9));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Uncurse, 9));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Transfer, 9));

                    InventoryManager.Reward(list12);
                }

                GUILayout.Label("---  ---");

                if (GUILayout.Button("GetPassiveRandom"))
                {
                    UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(new List<ItemBase>
                        {
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom(),
                            PlayData.GetPassiveRandom()
                        });
                }

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

                if (GUILayout.Button("get key"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key, 10)
                    });
                }



                if (GUILayout.Button("get potions"))
                {
                    List<string> list9 = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Potions, out list9);
                    InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_heal),
                            ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_weak),
                            ItemBase.GetItem(GDEItemKeys.Item_Potions_Potion_holywater)
                        });
                }


                if (GUILayout.Button("get rewards"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 10)
                    });
                }

                if (GUILayout.Button("get Jar*2 SmallReward Sou*10"))
                {
                    List<ItemBase> list12 = new List<ItemBase>();
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    list12.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_SmallReward, false));
                    list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Soul, 10));
                    InventoryManager.Reward(list12);
                }


                if (GUILayout.Button("get Reward_Ancient_Chest4"))
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


                if (GUILayout.Button("get reward3"))
                {
                    List<ItemBase> list11 = new List<ItemBase>();
                    for (int num2 = 0; num2 < 10; num2++)
                    {
                        list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                    }
                    InventoryManager.Reward(list11);
                    InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Consume_GoldenApple));
                }

                if (GUILayout.Button("get reward4"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Lucy_17)),
                        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_7)),
                        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_36))
                    });
                }

                if (GUILayout.Button("get reward5 JokerCard"))
                {
                    InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Passive_JokerCard));
                }

                if (GUILayout.Button("get reward7"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_7)
                    });
                }
                if (GUILayout.Button("get reward8"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_8)
                    });
                }
                if (GUILayout.Button("get reward9"))
                {
                    InventoryManager.Reward(new List<ItemBase>
                    {
                        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_9)
                    });
                }

                if (GUILayout.Button("get allevent"))
                {
                    List<string> eventList = new List<string>();
                    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.RandomEvent, out eventList);
                    FieldEventSelect.FieldEventSelectOpen(eventList, null, StageSystem.instance.RandomEventMainObject_S1, true);
                }


                if (GUILayout.Button("get roulette"))
                {
                    List<ItemBase> list = new List<ItemBase>();
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SmallBarrierMachine));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_GasMask));
                    list.Add(ItemBase.GetItem(GDEItemKeys.Item_Active_PotLid));
                    UIManager.InstantiateActive(UIManager.inst.RouletteUI).GetComponent<UI_MiniGame_Roulette>().Init();
                }


                if (GUILayout.Button("get stageevent"))
                {
                    List<string> list2 = new List<string>();
                    foreach (GDERandomEventData gderandomEventData in StageSystem.instance.Map.StageData.RandomEventList)
                    {
                        list2.Add(gderandomEventData.Key);
                    }
                    FieldEventSelect.FieldEventSelectOpen(list2, null, StageSystem.instance.RandomEventMainObject_S1, true);
                }



                if (GUILayout.Button("get equipget"))
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



                if (GUILayout.Button("get fastrun"))
                {
                    StageSystem.instance.Player.GetComponent<PlayerController>().FastRun();
                }




                if (GUILayout.Button("get fly"))
                {
                    StageSystem.instance.Player.GetComponent<PlayerController>().Fly();
                }



                /*
                if (GUILayout.Button("Soul *10")) {
                    using (List<Character>.Enumerator enumerator5 = PlayData.TSavedata.Party.GetEnumerator())
                    {
                        while (enumerator5.MoveNext())
                        {
                            Character character = enumerator5.Current;
                            character.Hp /= 2;
                        }
                        return;
                    }
                }
                */

                GUILayout.Label("--- PlayData ---");

                if (PlayData.TSavedata != null)
                {
                    if (GUILayout.Button($"maxhp +500 ;"))
                        foreach (var c in PlayData.TSavedata.Party)
                            c.OriginStat.maxhp += 500;

                    foreach (var c in PlayData.TSavedata.Party)
                    {
                        GUILayout.Label($"{c.Name}");
                        if (GUILayout.Button($"maxhp +500 ; {c.OriginStat.maxhp}")) { c.OriginStat.maxhp += 500; }
                    }
                }
                GUILayout.Label("--- FieldSystem ---");

                if (FieldSystem.instance != null)
                {

                }

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
                        if (GUILayout.Button($"GetDiscardCount ={allyTeam.GetDiscardCount + 10} ; {allyTeam.DiscardCount}")) { allyTeam.DiscardCount = allyTeam.GetDiscardCount + 10; }
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
                            if (GUILayout.Button($"ActionNum =0; {c.ActionNum }")) { c.ActionNum = 0; }
                            if (GUILayout.Button($"SkillUseDraw =false; {c.SkillUseDraw }")) { c.SkillUseDraw = false; }
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

                GUILayout.Label("--- item ---");
                foreach (var i in itemkeys)
                {
                    if (GUILayout.Button($"{i}")) { itemkey = i; }
                }
                GUILayout.Label($"--- {itemkey} ---");
                if (GUILayout.Button($"grt 16"))
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
                if (GUILayout.Button($"grt all {items[itemkey].Count} : bug!"))
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
                GUILayout.Label($"--- {itemkey} ---");
                foreach (var i in items[itemkey])
                {
                    if (GUILayout.Button($"{i}"))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            InventoryManager.Reward(ItemBase.GetItem(i, 10));
                        }
                        else
                            InventoryManager.Reward(ItemBase.GetItem(i));
                    }
                }
                GUILayout.Label("--- reward ---");

                foreach (var i in rewardkeys)
                {
                    if (GUILayout.Button($"{i}")) { rewardkey = i; }
                }
                GUILayout.Label($"--- {rewardkey} ---");
                foreach (var i in rewards[rewardkey])
                {
                    if (GUILayout.Button($"{i}"))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            List<ItemBase> list12 = new List<ItemBase>();
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            list12.AddRange(InventoryManager.RewardKey(i, false));
                            InventoryManager.Reward(list12);
                        }
                        else
                            InventoryManager.Reward(InventoryManager.RewardKey(i, false));
                    }
                }
                GUILayout.Label($"--- {Input.GetKey(KeyCode.LeftShift)} ---");



                #region GUI
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
            #endregion
        }

        private static void ArtifactPlusInven()
        {
            PlayData.TSavedata.ArkPassivePlus = 9;
            for (int m = PlayData.TSavedata.Passive_Itembase.Count; m < PlayData.TSavedata.ArkPassivePlus; m++)
            {
                PlayData.TSavedata.Passive_Itembase.Add(null);
            }
        }

        private static void Myitem(string s = "", int c = 16)
        {
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
        }

        private static List<ItemBase> Equips()
        {
            List<ItemBase> list12 = new List<ItemBase>();

            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_FlameDarkSword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_SwordOfStar));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_HalfMask));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Revenger));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb_0));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_DarkPrestClothes));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Arrowofangel));
            // etc
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Replica));
            // atk
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_BlackMoonSword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Sword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Ilya_Sword_0));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Ilya_Sword_1));
            // sup
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Cape));
            // def
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Armor));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_BlackSpikedArmor));
            return list12;
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
        }

        #region Harmony
        // ====================== 하모니 패치 샘플 ===================================

        //public override int Damage(BattleChar User, int Dmg, bool Cri, bool Pain = false, bool NOEFFECT = false, int PlusPenetration = 0, bool IgnorehealPro = false, bool HealingPro = false, bool OnlyUnscaleTime = false)
        [HarmonyPatch(typeof(BattleAlly), "Damage",
            typeof(BattleChar), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void Damage(BattleAlly __instance, ref int Dmg)
        {
            if (!noDamage.Value)
            {
                return;
            }
            Dmg = 0;
        }        

        [HarmonyPatch(typeof(BattleAlly), "Damage",
            typeof(BattleChar), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void Damage(BattleAlly __instance)
        {
            if (!noDamage.Value)
            {
                return;
            }            
            __instance.Recovery = __instance.Info.get_stat.maxhp;
        }

        [HarmonyPatch(typeof(BattleTeam), "AP", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetAP(BattleTeam __instance, ref int __0)
        {
            if (!noAP.Value)
            {
                return;
            }
            __0 = __instance.MAXAP;
        }


        [HarmonyPatch(typeof(BattleTeam), "GetDiscardCount", MethodType.Getter)]
        [HarmonyPostfix]
        public static void GetDiscardCount(ref int __result)//BattleTeam __instance,
        {
            if (!addDiscard.Value)
            {
                return;
            }
            __result += 10;
        }

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

        [HarmonyPatch(typeof(FieldSystem), "StageStart", typeof(string))]
        [HarmonyPostfix]
        public static void StageStart()//InventoryManager __instance, string StageKey
        {
            //logger.LogMessage($"Reward2 : {Item.GetName}");
            if (!isFogout.Value)
            {
                return;
            }
            Fogout();
        }

        [HarmonyPatch(typeof(FieldSystem), "NextStage")]
        [HarmonyPostfix]
        public static void NextStage()//InventoryManager __instance, string StageKey
        {
            //logger.LogMessage($"Reward2 : {Item.GetName}");
            if (!StageArkPartOn.Value)
            {
                return;
            }
            PlayData.TSavedata.StageArkPartOn = true;
        }

        
        [HarmonyPatch(typeof(WaitButton), "WaitAct")]
        [HarmonyPostfix]
        public static void WaitAct()//InventoryManager __instance, string StageKey
        {
            //logger.LogMessage($"Reward2 : {Item.GetName}");
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
            //logger.LogMessage($"Reward2 : {Item.GetName}");
            if (!WaitCountAdd.Value)
            {
                return;
            }
            BattleSystem.instance.AllyTeam.WaitCount+=3;
        }


        //public Stat AllyLevelPlusStat(int PlusLv = 0)
        /// <summary>
        /// 다른곳하고 공유되서 비효율적
        /// </summary>
        /// <param name="__result"></param>
        /*
        [HarmonyPatch(typeof(Character), "AllyLevelPlusStat", typeof(int))]
        [HarmonyPostfix]
        public static void AllyLevelPlusStat(ref Stat __result)//InventoryManager __instance, string StageKey
        {
            if (!isMaxHpUp.Value)
            {
                return;
            }
            logger.LogMessage($"AllyLevelPlusStat maxhp : {__result.maxhp}");
            __result.maxhp += 100;
        }
        */

        /*
         // public static void Reward(string rewardkey)
        [HarmonyPatch(typeof(InventoryManager), "Reward", typeof(string))]//, MethodType.StaticConstructor
        [HarmonyPrefix]
        public static void Reward(string rewardkey)//InventoryManager __instance,
        {
            logger.LogMessage($"Reward3 : {rewardkey}");
        }
        */
        /*
        [HarmonyPatch(typeof(BattleTeam), "MyTurn")]
        [HarmonyPostfix]
        public static void SetAP(BattleTeam __instance)
        {
            if (!addDiscard.Value)
            {
                return;
            }
            __instance.DiscardCount = __instance.GetDiscardCount+10;
        }
        */


        /*

        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(XPPicker __instance, ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            ___pickupRadius = pickupRadius.Value;
        }

        [HarmonyPatch(typeof(BattleAlly), "Damage", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eDamageMult.Value;
        }
        */
        // =========================================================
        #endregion
    }
}
