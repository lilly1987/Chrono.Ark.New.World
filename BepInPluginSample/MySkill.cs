using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BepInPluginSample
{
    internal class MySkill
    {
        public  static bool Use(BattleAlly battleAlly,bool Rare=false)
        {
            List<Skill> list = new List<Skill>();
            //foreach (var gdeskillData in PlayData.GetCharacterSkillNoOverLap(battleAlly.Info, false, null))
            foreach (var gdeskillData in PlayData.GetMySkills(battleAlly.Info.KeyData, Rare))
            {
                list.Add(Skill.TempSkill(gdeskillData.KeyID, battleAlly, PlayData.TempBattleTeam));
            }
            //PlayData.TSavedata.UseItemKeys.Add(GDEItemKeys.Item_Consume_SkillBookCharacter);
            FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list, new SkillButton.SkillClickDel(MySkill.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, true));
            return true;
        }

        // Token: 0x06003374 RID: 13172 RVA: 0x00162AC8 File Offset: 0x00160CC8
        public static void SkillAdd(SkillButton Mybutton)
        {
            Mybutton.Myskill.Master.Info.UseSoulStone(Mybutton.Myskill);
            UIManager.inst.CharstatUI.GetComponent<CharStatV4>().SkillUPdate();
        }
    }
}