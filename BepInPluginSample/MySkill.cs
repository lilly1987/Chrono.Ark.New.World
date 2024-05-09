using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DarkTonic.MasterAudio;
using GameDataEditor;
using HarmonyLib;
using HarmonyLib.Tools;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UseItem;

namespace BepInPluginSample
{
    internal class MySkill
    {
        public static bool Use(BattleAlly battleAlly, bool Rare = false)
        {
            List<Skill> list = new List<Skill>();
            foreach (var gdeskillData in PlayData.GetMySkills(battleAlly.Info.KeyData, Rare))
            {
                list.Add(Skill.TempSkill(gdeskillData.KeyID, battleAlly, PlayData.TempBattleTeam));
            }
            FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list, new SkillButton.SkillClickDel(MySkill.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, true));
            return true;
        }

        // Token: 0x06003374 RID: 13172 RVA: 0x00162AC8 File Offset: 0x00160CC8
        public static void SkillAdd(SkillButton Mybutton)
        {
            Mybutton.Myskill.Master.Info.UseSoulStone(Mybutton.Myskill);
            UIManager.inst.CharstatUI.GetComponent<CharStatV4>().SkillUPdate();
        }
        public static bool Use(bool isDrawSkill)
        {
            FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(PlayData.GetLucySkill(isDrawSkill), new SkillButton.SkillClickDel(MySkill.SkillAdd2), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, false));
            return true;
        }

        // Token: 0x06003393 RID: 13203 RVA: 0x001637A8 File Offset: 0x001619A8
        public static void SkillAdd2(SkillButton Mybutton)
        {
            PlayData.TSavedata.LucySkills.Add(Mybutton.Myskill.MySkill.KeyID);
            UIManager.inst.CharstatUI.GetComponent<CharStatV4>().SkillUPdate();
        }

        [HarmonyPatch(typeof(CharStatV4), "ReturnLucyDrawCard")]
        [HarmonyPrefix]
        public static bool ReturnLucyDrawCard(CharStatV4 __instance, ref List<Skill> __result)
        {
            Sample.logger.LogWarning($"CharStatV4.ReturnLucyDrawCard");
            if (Sample.skillAll.Value)
            {
                //CharStatV4.<> c__DisplayClass157_0 CS$<> 8__locals1 = new CharStatV4.<> c__DisplayClass157_0();
                List<Skill> list = new List<Skill>();
                var Myskills = new List<GDESkillData>();
                foreach (GDESkillData i in PlayData.ALLSKILLLIST)
                {
                    if (!i.NoDrop && (!CharacterSkinData.SwimDLCSkillKeyList.Contains(i.KeyID) || CharacterSkinData.SteamDLCCheck()))
                    {
                        if (i.User == "LucyDraw3" && PlayData.TSavedata.SoulUpgrade.SkillDraw >= 1 && SaveManager.IsUnlock(i))
                        {
                            Myskills.Add(i);
                        }
                        else if (i.User == "LucyDraw" && SaveManager.IsUnlock(i) && (i.LucyPartyDraw == "" || PlayData.TSavedata.Party.Find((Character a) => a.KeyData == i.LucyPartyDraw) != null))
                        {
                            Myskills.Add(i);
                        }
                    }
                }
                int k;
                for (int i = 0; i < Myskills.Count; i = k + 1)
                {
                    if (Myskills[i].LucyPartyDraw != "" && PlayData.TSavedata.LucySkills.Find((string a) => a == Myskills[i].KeyID) != null)
                    {
                        Myskills.RemoveAt(i);
                        k = i;
                        i = k - 1;
                    }
                    k = i;
                }
                foreach (GDESkillData gdeskillData in Myskills)
                {
                    if (!SaveManager.IsUnlock(gdeskillData.Key, SaveManager.NowData.unlockList.SkillPreView))
                    {
                        SaveManager.NowData.unlockList.SkillPreView.Add(gdeskillData.Key);
                    }
                    list.Add(Skill.TempSkill(gdeskillData.Key, PlayData.TempBattleTeam.DummyChar, PlayData.TempBattleTeam).CloneSkill(false, null, null, false));
                }
                __result = list;

                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(CharFace), "GetRandomSkill")]
        [HarmonyPrefix]
        public static bool GetRandomSkill(CharFace __instance, ref List<Skill> __result)
        {
            Sample.logger.LogWarning($"CharFace.GetRandomSkill");
            if (Sample.skillAll.Value)
            {
                List<Skill> list = new List<Skill>();
                List<GDESkillData> characterSkillNoOverLap = PlayData.GetCharacterSkillNoOverLap(__instance.AllyCharacter.Info, false, null);
                foreach (GDESkillData gdeskillData in characterSkillNoOverLap)
                {
                    list.Add(Skill.TempSkill(gdeskillData.Key, __instance.AllyCharacter, PlayData.TempBattleTeam).CloneSkill(false, null, null, false));
                    if (!SaveManager.IsUnlock(gdeskillData.Key, SaveManager.NowData.unlockList.SkillPreView))
                    {
                        SaveManager.NowData.unlockList.SkillPreView.Add(gdeskillData.Key);
                    }
                }
                if (PlayData.Passive.Find((Item_Passive a) => a.itemkey == GDEItemKeys.Item_Passive_505Error) != null)
                {
                    List<GDESkillData> list2 = new List<GDESkillData>();
                    foreach (GDESkillData gdeskillData2 in PlayData.ALLSKILLLIST)
                    {
                        if (gdeskillData2.User != "" && gdeskillData2.Category.Key != GDEItemKeys.SkillCategory_LucySkill && gdeskillData2.Category.Key != GDEItemKeys.SkillCategory_DefultSkill && gdeskillData2.User != GDEItemKeys.Character_LucyC && !gdeskillData2.NoDrop && !gdeskillData2.Lock && gdeskillData2.User != __instance.AllyCharacter.Info.KeyData)
                        {
                            GDECharacterData gdecharacterData = new GDECharacterData(gdeskillData2.User);
                            if (!(gdeskillData2.KeyID == GDEItemKeys.Skill_S_Phoenix_6) && !(gdeskillData2.Key == GDEItemKeys.Skill_S_Phoenix_6) && gdecharacterData != null && Misc.IsUseableCharacter(gdecharacterData.Key))
                            {
                                list2.Add(gdeskillData2);
                            }
                        }
                    }
                    List<GDESkillData> list3 = new List<GDESkillData>();
                    List<Skill> list4 = new List<Skill>();
                    list3.AddRange(list2);
                    foreach (GDESkillData gdeskillData3 in list3)
                    {
                        list4.Add(Skill.TempSkill(gdeskillData3.Key, __instance.AllyCharacter, __instance.AllyCharacter.MyTeam));
                    }
                    list.AddRange(list4);
                }
                __result = list;

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CharacterWindow), "GetRandomSkill")]
        [HarmonyPrefix]
        public static bool GetRandomSkill(CharacterWindow __instance, ref List<Skill> __result)
        {
            Sample.logger.LogWarning($"CharacterWindow.GetRandomSkill");
            if (Sample.skillAll.Value)
            {
                List<Skill> list = new List<Skill>();
                List<GDESkillData> characterSkillNoOverLap = PlayData.GetCharacterSkillNoOverLap(__instance.AllyCharacter.Info, false, null);
                //int num = 3;
                //int num2 = 0;            
                foreach (GDESkillData gdeskillData in characterSkillNoOverLap)
                {
                    characterSkillNoOverLap.Remove(gdeskillData);
                    list.Add(Skill.TempSkill(gdeskillData.Key, __instance.AllyCharacter, PlayData.TempBattleTeam).CloneSkill(false, null, null, false));
                    if (!SaveManager.IsUnlock(gdeskillData.Key, SaveManager.NowData.unlockList.SkillPreView))
                    {
                        SaveManager.NowData.unlockList.SkillPreView.Add(gdeskillData.Key);
                    }
                }
                if (PlayData.Passive.Find((Item_Passive a) => a.itemkey == GDEItemKeys.Item_Passive_505Error) != null)
                {
                    List<GDESkillData> list2 = new List<GDESkillData>();
                    foreach (GDESkillData gdeskillData2 in PlayData.ALLSKILLLIST)
                    {
                        if (gdeskillData2.User != "" && gdeskillData2.Category.Key != GDEItemKeys.SkillCategory_LucySkill && gdeskillData2.Category.Key != GDEItemKeys.SkillCategory_DefultSkill && gdeskillData2.User != GDEItemKeys.Character_LucyC && !gdeskillData2.NoDrop && !gdeskillData2.Lock && gdeskillData2.User != __instance.AllyCharacter.Info.KeyData)
                        {
                            GDECharacterData gdecharacterData = new GDECharacterData(gdeskillData2.User);
                            if (!(gdeskillData2.KeyID == GDEItemKeys.Skill_S_Phoenix_6) && !(gdeskillData2.Key == GDEItemKeys.Skill_S_Phoenix_6) && gdecharacterData != null && Misc.IsUseableCharacter(gdecharacterData.Key))
                            {
                                list2.Add(gdeskillData2);
                            }
                        }
                    }
                    List<GDESkillData> list3 = new List<GDESkillData>();
                    List<Skill> list4 = new List<Skill>();
                    //list3.AddRange(list2.Random(RandomClassKey.AllSkill, 3));
                    list3.AddRange(list2);
                    foreach (GDESkillData gdeskillData3 in list3)
                    {
                        list4.Add(Skill.TempSkill(gdeskillData3.Key, __instance.AllyCharacter, __instance.AllyCharacter.MyTeam));
                    }
                    list.AddRange(list4);
                }
                __result = list;

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SkillBookLucy), "Use")]
        [HarmonyPrefix]
        public static bool Use(SkillBookLucy __instance, ref bool __result)
        {
            Sample.logger.LogWarning($"SkillBookLucy");
            if (Sample.skillAll.Value)
            {
                List<string> lists = new List<string>();
                List<GDESkillData> list = new List<GDESkillData>();
                foreach (GDESkillData gdeskillData in PlayData.ALLSKILLLIST)
                {
                    if ((!CharacterSkinData.SwimDLCSkillKeyList.Contains(gdeskillData.KeyID) || CharacterSkinData.SteamDLCCheck()) && gdeskillData.Category.Key == GDEItemKeys.SkillCategory_LucySkill && gdeskillData.User == "Lucy" && gdeskillData.Key != GDEItemKeys.Skill_S_Lucy_25)
                    {
                        list.Add(gdeskillData);
                        if (!((gdeskillData.Lock && !SaveManager.IsUnlock(gdeskillData.Key)) || gdeskillData.NoDrop || PlayData.TSavedata.LucySkillList_Legendary.Find((string a) => a == gdeskillData.Key) != null))
                        {
                            PlayData.TSavedata.LucySkillList_Legendary.Add(gdeskillData.Key);
                            lists.Add(gdeskillData.Key);
                        }
                    }
                }

                List<Skill> list2 = new List<Skill>();
                foreach (string key in lists)
                {
                    list2.Add(Skill.TempSkill(key, PlayData.BattleLucy, PlayData.TempBattleTeam));
                }
                PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_SkillBookLucy);
                FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list2, new SkillButton.SkillClickDel(__instance.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, false));
                MasterAudio.PlaySound("BookFlip", 1f, null, 0f, null, null, false, false);
                __result = true;

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SkillBookCharacter_Rare), "Use")]
        [HarmonyPrefix]
        public static bool Use(SkillBookCharacter_Rare __instance, ref bool __result)//InventoryManager __instance, string StageKey        
        {
            Sample.logger.LogWarning($"SkillBookCharacter_Rare");
            if (Sample.skillAll.Value)
            {
                List<Skill> list = new List<Skill>();
                List<BattleAlly> battleallys = PlayData.Battleallys;
                BattleTeam tempBattleTeam = PlayData.TempBattleTeam;
                for (int i = 0; i < PlayData.TSavedata.Party.Count; i++)
                {
                    bool flag = false;
                    if (PlayData.TSavedata.SpRule == null || !PlayData.TSavedata.SpRule.RuleChange.CharacterRareSkillInfinityGet)
                    {
                        using (List<CharInfoSkillData>.Enumerator enumerator = PlayData.TSavedata.Party[i].SkillDatas.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.Skill.Rare)
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (PlayData.TSavedata.Party[i].BasicSkill.Rare)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        //GDESkillData gdeskillData = PlayData.GetMySkills(PlayData.TSavedata.Party[i].KeyData, true).Random(RandomClassKey.RareSkill(i));
                        //if (gdeskillData != null)
                        //{
                        //    list.Add(Skill.TempSkill(gdeskillData.KeyID, battleallys[i], tempBattleTeam));
                        //}
                        foreach (var gdeskillData in PlayData.GetMySkills(PlayData.TSavedata.Party[i].KeyData, true))
                        {
                            list.Add(Skill.TempSkill(gdeskillData.KeyID, battleallys[i], tempBattleTeam));
                        }
                    }
                }
                if (list.Count == 0)
                {
                    EffectView.SimpleTextout(FieldSystem.instance.TopWindow.transform, ScriptLocalization.System.CantRareSkill, 1f, false, 1f);
                    __result = false;
                }
                foreach (Skill skill in list)
                {
                    if (!SaveManager.IsUnlock(skill.MySkill.KeyID, SaveManager.NowData.unlockList.SkillPreView))
                    {
                        SaveManager.NowData.unlockList.SkillPreView.Add(skill.MySkill.KeyID);
                    }
                }
                PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_SkillBookCharacter_Rare);
                MasterAudio.PlaySound("BookFlip", 1f, null, 0f, null, null, false, false);
                FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list, new SkillButton.SkillClickDel(__instance.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, true));
                __result = true;

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SkillBookCharacter), "Use")]
        [HarmonyPrefix]
        public static bool Use(SkillBookCharacter __instance, ref bool __result)//InventoryManager __instance, string StageKey
        {
            Sample.logger.LogWarning($"SkillBookCharacter");
            if (Sample.skillAll.Value)
            {
                // ---

                List<Skill> list = new List<Skill>();
                List<BattleAlly> battleallys = PlayData.Battleallys;
                BattleTeam tempBattleTeam = PlayData.TempBattleTeam;
                foreach (var item in battleallys)
                {
                    foreach (var gdeskillData in PlayData.GetCharacterSkillNoOverLap(item.Info, false, null))
                    {
                        list.Add(Skill.TempSkill(gdeskillData.KeyID, item, tempBattleTeam));
                    }
                }
                foreach (Skill skill in list)
                {
                    if (!SaveManager.IsUnlock(skill.MySkill.KeyID, SaveManager.NowData.unlockList.SkillPreView))
                    {
                        SaveManager.NowData.unlockList.SkillPreView.Add(skill.MySkill.KeyID);
                    }
                }
                Sample.logger.LogWarning($"SkillBookCharacter {list.Count}");
                PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_SkillBookCharacter);
                FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list, new SkillButton.SkillClickDel(__instance.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, true));
                MasterAudio.PlaySound("BookFlip", 1f, null, 0f, null, null, false, false);
                __result = true;

                // ---
                return false;
            }
            return true;
        }
    }
}