using GameDataEditor;
using System;
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


		public static void Skill_ExtendedSet()
        {
            List<Skill_Extended> list = GetList();

            foreach (BattleChar battleChar in PlayData.Battleallys)
            {
                AllSet(list, battleChar);
            }
        }

        public static void AllSet(List<Skill_Extended> list, BattleChar battleChar)
        {            

            Sample.logger.LogMessage($"0 {battleChar.Info.Name} {battleChar.Skills.Count} {battleChar.Info.SkillDatas.Count}");
            foreach (CharInfoSkillData item in battleChar.Info.SkillDatas)
            {
                Sample.logger.LogMessage($"a1 {item.Skill.Name}");
                Sample.logger.LogMessage($"a2 {item.SKillExtended?.Name}");
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
                    Sample.logger.LogMessage($"7 {_skill_Extended.Name}");
                    enforceSkill.CharinfoSkilldata.SKillExtended = _skill_Extended;
                }
            }
        }

		public static bool Skill_ExtendedSelect()
        {
            List<Skill_Extended> list = GetList();

            List<Skill_Extended> list2 = new List<Skill_Extended>();

            foreach (BattleAlly battleAlly in PlayData.Battleallys)
            {
                Sample.logger.LogMessage($"1 {battleAlly.name}");
                List<Skill_Extended> list4 = new List<Skill_Extended>();
                foreach (Skill_Extended skill_Extended in list)
                {
                    if (skill_Extended.Data.NeedCharacter == battleAlly.Info.KeyData)
                    {
                        /*
                        [Message: Lilly] SilverStein
                        [Message: Lilly] SilverStein
                         */
                        Sample.logger.LogMessage(battleAlly.Info.KeyData);
                        foreach (BattleChar battleChar in PlayData.Battleallys)
                        {
                            bool flag = false;
                            foreach (Skill enforceSkill in battleChar.Skills)
                            {
                                if (skill_Extended.CanEnforce(enforceSkill))
                                {
                                    /*
[Message: Lilly] 요한; SkillEn_Silver_0
[Message: Lilly] 요한; SkillEn_Silver_1
									*/
                                    Sample.logger.LogMessage($"{battleChar.Info.Name} ; {skill_Extended.Name}");
                                    list4.Add(skill_Extended);
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
                if (list4.Count != 0)
                {
                    list2.Add(list4.Random<Skill_Extended>());
                }
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

        public static List<Skill_Extended> GetList()
        {
            List<Skill_Extended> list = new List<Skill_Extended>();
            List<string> list3 = new List<string>();
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list3);// 모든 확장스킬 목록
            foreach (string key in list3)
            {
                GDESkillExtendedData Temp = new GDESkillExtendedData(key);
                //if (Temp.Drop && !Temp.Debuff)
                if (Temp.Drop && !Temp.Debuff && PlayData.TSavedata.Party.Find((Character a) => a.KeyData == Temp.NeedCharacter) != null)
                {   // 현제 파티에서 추출 가능한 확장스킬
                    Sample.logger.LogMessage($"{Temp.Drop} ; {Temp.Debuff} ; {key} ; {Temp.Name}");
                    /*
[Message:     Lilly] SkillEn_Silver_0 ; 실버스틴의 표식 대상을 지정 했을 경우, 마나 1을 회복합니다.
[Message:     Lilly] SkillEn_Silver_1 ; 손에서 내면 실버스틴이 실버스틴의 표식이 있는 적을 2번 사격합니다.
[Message:     Lilly] SkillEn_Mement_0 ; 손에 있는동안 요한이 근접사격을 3번 시전할 때 마다 비용 1 감소, 이 스킬은 비용이 0이어도 요한의 고정능력을 다시 활성화 할 수 있습니다.
[Message:     Lilly] SKillEn_Mement_1 ; 시전시 이 스킬을 생성하여 손으로 가져오고 제외를 부여합니다. 그 스킬은 생성된 스킬로 취급하지 않습니다.
					 */
                    
                    list.Add(Skill_Extended.DataToExtended(Temp));
                }
            }
            list.AddRange(PlayData.GetEnforce(false));

            return list;
        }
    }
}
