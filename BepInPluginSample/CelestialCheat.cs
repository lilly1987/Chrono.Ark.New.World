using GameDataEditor;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BepInPluginSample
{
    internal class CelestialCheat
    {

        public static void info()
        {
            //PlayData.GetEnforce(false).Random(4);
            foreach (var item in PlayData.GetEnforce(false))
            {
                Sample.logger.LogMessage($"a1 {item.Name}");
                if (item.Data!=null)
                {
                    Sample.logger.LogMessage($"a1 {item.Data.Name}");
                }
                
            }
           
        }


        public static List<Skill_Extended> GetEnforce(Skill SelectSkill = null)
        {

            List<Skill_Extended> list = new List<Skill_Extended>();
            List<string> list2 = new List<string>();
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list2);
            /*
            if (Weak)
            {
                using (List<string>.Enumerator enumerator = list2.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string key = enumerator.Current;
                        GDESkillExtendedData gdeskillExtendedData = new GDESkillExtendedData(key);
                        if (gdeskillExtendedData.Drop && gdeskillExtendedData.Debuff)
                        {
                            list.Add(Skill_Extended.DataToExtended(gdeskillExtendedData));
                        }
                    }                    
                }
            }
            else
            */
            {
            foreach (string key2 in list2)
            {
                GDESkillExtendedData Temp = new GDESkillExtendedData(key2);
                if (Temp.Drop && !Temp.Debuff && (Temp.NeedCharacter == "" || PlayData.TSavedata.Party.Find((Character a) => a.KeyData == Temp.NeedCharacter) != null))
                {
                    list.Add(Skill_Extended.DataToExtended(Temp));
                }
            }
            }

            int i = 0;
            while (i < list.Count)
            {
                bool flag = false;
                if (SelectSkill == null)
                {
                    using (List<BattleAlly>.Enumerator enumerator2 = PlayData.Battleallys.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            BattleAlly battleAlly = enumerator2.Current;
                            foreach (Skill skill in battleAlly.Skills)
                            {
                                if (!skill.Enforce && !skill.Enforce_CantUse && !skill.Enforce_Weak && skill.MySkill.Category.Key != GDEItemKeys.SkillCategory_DefultSkill && list[i].CanSkillEnforce(skill) && list[i].CanSkillEnforceChar(skill))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                        goto IL_245;
                    }                   
                }
                goto IL_1EE;
            IL_245:
                if (!flag)
                {
                    list.RemoveAt(i);
                    i--;
                }
                i++;
                continue;
            IL_1EE:
                if (!SelectSkill.Enforce && !SelectSkill.Enforce_CantUse && !SelectSkill.Enforce_Weak && SelectSkill.MySkill.Category.Key != GDEItemKeys.SkillCategory_DefultSkill && list[i].CanSkillEnforce(SelectSkill) && list[i].CanSkillEnforceChar(SelectSkill))
                {
                    flag = true;                   
                }
                goto IL_245;
            }
            return list;
        }

        #region Skill_Extended
        internal static void Skill_ExtendedInfo()
        {
            foreach (BattleChar battleChar in PlayData.Battleallys)
            {
                foreach (CharInfoSkillData item in battleChar.Info.SkillDatas)
                {
                    Sample.logger.LogMessage($"CharInfoSkillData ; {item.Skill.Name} ; {item.SKillExtended?.Name} ;");
                }
                foreach (Skill enforceSkill in battleChar.Skills)
                {
                    Sample.logger.LogMessage($"Skill ; {enforceSkill.CharinfoSkilldata.Skill.Name} ; {enforceSkill.CharinfoSkilldata.SKillExtended?.Name} ;");
                }
            }
        }

        internal static void Skill_ExtendedDel()
        {
            foreach (BattleChar battleChar in PlayData.Battleallys)
            {
                foreach (Skill enforceSkill in battleChar.Skills)
                {
                    Sample.logger.LogMessage($"del ; {enforceSkill.CharinfoSkilldata.Skill.Name} ; {enforceSkill.CharinfoSkilldata.SKillExtended?.Name} ; {enforceSkill.CharinfoSkilldata.SKillExtended?.ExtendedName()} ;");
                    enforceSkill.CharinfoSkilldata.SKillExtended = null;
                }
            }
        }

        public static void AllSkill_ExtendedSet(List<Skill_Extended> list, BattleChar battleChar)
        {            

            Sample.logger.LogMessage($"0 {battleChar.Info.Name} {battleChar.Skills.Count} {battleChar.Info.SkillDatas.Count}");
            foreach (CharInfoSkillData item in battleChar.Info.SkillDatas)
            {
                Sample.logger.LogMessage($"b ; {item.Skill.Name} ; {item.SKillExtended?.Name} ;");
            }

            foreach (Skill enforceSkill in battleChar.Skills)
            {
                Sample.logger.LogMessage($"1 {enforceSkill.CharinfoSkilldata.Skill.Name}");
                Sample.logger.LogMessage($"2 {enforceSkill.CharinfoSkilldata?.SKillExtended}");
                Sample.logger.LogMessage($"3 {enforceSkill.AllExtendeds.Count}");
                foreach (var item in enforceSkill.AllExtendeds)
                {
                    if (item.Data != null)
                    {
                        Sample.logger.LogMessage($"4 {item.Name} ; {item.ExtendedName()}");
                    }
                    else
                        Sample.logger.LogMessage($"4 {item.Name}");
                }

                List<Skill_Extended> list4 = new List<Skill_Extended>();
                foreach (Skill_Extended skill_Extended in list)
                {
                    if (skill_Extended.CanEnforce(enforceSkill))
                    {
                        /*
[Message: Lilly] 요한; SkillEn_Silver_0
[Message: Lilly] 요한; SkillEn_Silver_1
                        */
                        Sample.logger.LogMessage($"5 {battleChar.Info.Name} ; {skill_Extended.Name}");
                        list4.Add(skill_Extended);
                    }
                }
                Sample.logger.LogMessage($"6 {list4.Count}");
                if (list4.Count > 0)
                {
                    var _skill_Extended = list4.Random<Skill_Extended>();
                    Sample.logger.LogMessage($"7 {_skill_Extended.Name} ; {enforceSkill.CharinfoSkilldata.SKillExtended} ;");                    
                    enforceSkill.CharinfoSkilldata.SKillExtended = _skill_Extended;
                }
            }
        }
                
		public static bool Skill_ExtendedSelect_Give(BattleChar battleAlly)
        {
            List<Skill_Extended> list = new List<Skill_Extended>();
            List<Skill_Extended> list2 = new List<Skill_Extended>();
            List<string> list3 = new List<string>();
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list3);
            foreach (string key in list3)
            {
                GDESkillExtendedData Temp = new GDESkillExtendedData(key);
                if (Temp.Drop && !Temp.Debuff && PlayData.TSavedata.Party.Find((Character a) => a.KeyData == Temp.NeedCharacter) != null)
                {
                    list.Add(Skill_Extended.DataToExtended(Temp));
                }
            }

            //foreach (BattleAlly battleAlly in PlayData.Battleallys)
            {
                //List<Skill_Extended> list4 = new List<Skill_Extended>();
                foreach (Skill_Extended skill_Extended in list)
                {
                    if (skill_Extended.Data.NeedCharacter == battleAlly.Info.KeyData)
                    {
                        foreach (BattleChar battleChar in PlayData.Battleallys)
                        {
                            bool flag = false;
                            foreach (Skill enforceSkill in battleChar.Skills)
                            {
                                if (skill_Extended.CanEnforce(enforceSkill))
                                {
                                    //list4.Add(skill_Extended);
                                    list2.Add(skill_Extended);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
                //if (list4.Count != 0)
                //{
                //    list2.Add(list4.Random(RandomClassKey.Celestial));
                //}
            }
            if (list2.Count >= 1)
            {
                UIManager.InstantiateActive(UIManager.inst.EnforceUI).GetComponent<UI_Enforce>().Init(list2);
                //MasterAudio.PlaySound("Crystal", 1f, null, 0f, null, null, false, false);
                //PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_Celestial);
                return true;
            }
            //EffectView.SimpleTextout(FieldSystem.instance.TopWindow.transform, ScriptLocalization.System_Item.CelestialCant, 1f, false, 1f);
            return false;
        }
        
		public static bool Skill_ExtendedSelect_receive(BattleChar battleChar)
        {
            List<Skill_Extended> list = new List<Skill_Extended>();
            List<Skill_Extended> list2 = new List<Skill_Extended>();
            List<string> list3 = new List<string>();
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list3);
            foreach (string key in list3)
            {
                GDESkillExtendedData Temp = new GDESkillExtendedData(key);
                if (Temp.Drop && !Temp.Debuff && PlayData.TSavedata.Party.Find((Character a) => a.KeyData == Temp.NeedCharacter) != null)
                {
                    list.Add(Skill_Extended.DataToExtended(Temp));
                }
            }
            foreach (BattleAlly battleAlly in PlayData.Battleallys)
            {
                //List<Skill_Extended> list4 = new List<Skill_Extended>();
                foreach (Skill_Extended skill_Extended in list)
                {
                    if (skill_Extended.Data.NeedCharacter == battleAlly.Info.KeyData)
                    {
                        //foreach (BattleChar battleChar in PlayData.Battleallys)
                        {
                            bool flag = false;
                            foreach (Skill enforceSkill in battleChar.Skills)
                            {
                                if (skill_Extended.CanEnforce(enforceSkill))
                                {
                                    //list4.Add(skill_Extended);
                                    list2.Add(skill_Extended);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
                //if (list4.Count != 0)
                //{
                //    list2.Add(list4.Random(RandomClassKey.Celestial));
                //}
            }
            if (list2.Count >= 1)
            {
                UIManager.InstantiateActive(UIManager.inst.EnforceUI).GetComponent<UI_Enforce>().Init(list2);
                //MasterAudio.PlaySound("Crystal", 1f, null, 0f, null, null, false, false);
                //PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_Celestial);
                return true;
            }
            //EffectView.SimpleTextout(FieldSystem.instance.TopWindow.transform, ScriptLocalization.System_Item.CelestialCant, 1f, false, 1f);
            return false;
        }

        public static void Skill_ExtendedSelect_random()
        {
            List<Skill_Extended> list = new List<Skill_Extended>();            
            List<string> list3 = new List<string>();
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list3);
            foreach (string key in list3)
            {
                GDESkillExtendedData Temp = new GDESkillExtendedData(key);
                if (Temp.Drop && !Temp.Debuff && PlayData.TSavedata.Party.Find((Character a) => a.KeyData == Temp.NeedCharacter) != null)
                {
                    list.Add(Skill_Extended.DataToExtended(Temp));
                }
            }
            foreach (BattleChar battleChar in PlayData.Battleallys)
            {
                foreach (Skill enforceSkill in battleChar.Skills)
                {
                    List<Skill_Extended> list4 = new List<Skill_Extended>();
                    foreach (BattleAlly battleAlly in PlayData.Battleallys)
                    {
                        foreach (Skill_Extended skill_Extended in list)
                        {
                            if (skill_Extended.Data.NeedCharacter == battleAlly.Info.KeyData)
                            {
                                bool flag = false;
                                if (skill_Extended.CanEnforce(enforceSkill))
                                {
                                    list4.Add(skill_Extended);
                                    flag = true;
                                    break;
                                }
                                if (flag)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (list4.Count != 0)
                    {
                        enforceSkill.CharinfoSkilldata.SKillExtended = 
                        list4.Random(RandomClassKey.Celestial);
                    }
                }
            }
        }

        #endregion
    }
}
