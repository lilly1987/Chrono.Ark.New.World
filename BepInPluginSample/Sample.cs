using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameDataEditor;
using HarmonyLib;
using System;
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
            // xpMulti = Config.Bind("game", "xpMulti", 2f);

            // =========================================================
            #endregion

            items[""] = new List<string>();
            rewards[""] = new List<string>();

            // InventoryManager.Reward(ItemBase.GetItem(
            items["Item_Consume_"] = new List<string>();
            items["Item_Active_"] = new List<string>();
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
                        Logger.LogMessage($"Name: {memberInfo.Name} ; Type: {f.FieldType.Name} ; IsPublic: {f.IsPublic} ; IsStatic: {f.IsStatic} ; GetValue: {f.GetValue(null)} ;");

                        if (memberInfo.Name.StartsWith("Item_Consume_")) items["Item_Consume_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Active_")) items["Item_Active_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Equip_")) items["Item_Equip_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Scroll_")) items["Item_Scroll_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Misc_")) items["Item_Misc_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Passive_")) items["Item_Passive_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Item_Potions_")) items["Item_Potions_"].Add((String)(f.GetValue(null)));
                        //else if (memberInfo.Name.StartsWith("ItemClass_")) items["ItemClass_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("RandomDrop_")) items["RandomDrop_"].Add((String)(f.GetValue(null)));
                        else if (memberInfo.Name.StartsWith("Reward_")) rewards["Reward_"].Add((String)(f.GetValue(null)));

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

                if (GUILayout.Button("save")) SaveManager.savemanager.ProgressOneSave();

                if (GUILayout.Button($"0 Damage {noDamage.Value}")) { noDamage.Value = !noDamage.Value; }
                if (GUILayout.Button($"0 Mna {noAP.Value}")) { noAP.Value = !noAP.Value; }
                if (GUILayout.Button($"MyTurn add Discard 10 {addDiscard.Value}")) { addDiscard.Value = !addDiscard.Value; }

                if (GUILayout.Button("TimeMoney +1000")) { SaveManager.NowData.TimeMoney += 1000; }

                if (GUILayout.Button("gold +1000")) { PlayData.Gold += 1000; }
                if (GUILayout.Button("gold *10")) { PlayData.Gold *= 10; }

                if (GUILayout.Button("Soul +1000")) { PlayData.Soul += 1000; }
                if (GUILayout.Button("Soul *10")) { PlayData.Soul *= 10; }

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

                if (BattleSystem.instance != null)
                {
                    if (BattleSystem.instance.AllyTeam != null)
                    {
                        GUILayout.Label("AllyTeam");

                        if (GUILayout.Button($"Mana {BattleSystem.instance.AllyTeam.AP} +10")) { BattleSystem.instance.AllyTeam.AP += 10; }

                        if (GUILayout.Button($"ActionCount =1 ; {BattleSystem.instance.AllyTeam.LucyAlly.ActionCount}")) { BattleSystem.instance.AllyTeam.LucyAlly.ActionCount = 1; }
                        if (GUILayout.Button($"Overload =1 ; {BattleSystem.instance.AllyTeam.LucyAlly.Overload}")) { BattleSystem.instance.AllyTeam.LucyAlly.Overload = 1; }
                        if (GUILayout.Button($"ActionNum =0 ; {BattleSystem.instance.AllyTeam.LucyAlly.ActionNum}")) { BattleSystem.instance.AllyTeam.LucyAlly.ActionNum = 0; }

                        if (GUILayout.Button($"UsedDeckToDeckNum =0 ; {BattleSystem.instance.AllyTeam.UsedDeckToDeckNum}")) { BattleSystem.instance.AllyTeam.UsedDeckToDeckNum = 0; }
                        if (GUILayout.Button($"GetDiscardCount ={BattleSystem.instance.AllyTeam.GetDiscardCount + 10} ; {BattleSystem.instance.AllyTeam.DiscardCount}")) { BattleSystem.instance.AllyTeam.DiscardCount = BattleSystem.instance.AllyTeam.GetDiscardCount + 10; }
                        if (GUILayout.Button($"WaitCount ={1 + PlayData.PartySpeed} ; {BattleSystem.instance.AllyTeam.WaitCount}")) { BattleSystem.instance.AllyTeam.WaitCount = 1 + PlayData.PartySpeed; }

                        GUILayout.Label($"BattleChar count {BattleSystem.instance.AllyTeam.AliveChars.Count}");
                        if (GUILayout.Button($"HP = /=2 ; "))
                            foreach (BattleChar c in BattleSystem.instance.AllyTeam.AliveChars)
                            {
                                c.HP = c.Info.get_stat.maxhp / 2;
                            }
                        foreach (BattleChar c in BattleSystem.instance.AllyTeam.AliveChars)
                        {
                            GUILayout.Label($"{c.Info.Name}");
                            if (GUILayout.Button($"HP +100; {c.HP}")) { c.HP += 100; }
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
                foreach (var i in items[itemkey])
                {
                    if (GUILayout.Button($"{i}")) {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            InventoryManager.Reward(ItemBase.GetItem(i,10));
                        }else
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
                    if (GUILayout.Button($"{i}")) {
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
                logger.LogMessage($"Reward1 : {item.GetName}");
            }
        }

        [HarmonyPatch(typeof(InventoryManager), "Reward", typeof(ItemBase))]
        [HarmonyPrefix]
        public static void Reward(ItemBase Item)//InventoryManager __instance,
        {
            logger.LogMessage($"Reward2 : {Item.GetName}");
        }
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
