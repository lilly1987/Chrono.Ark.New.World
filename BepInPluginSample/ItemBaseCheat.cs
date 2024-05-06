using GameDataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BepInPluginSample
{
    internal class ItemBaseCheat
    {

        internal static void GetPassiveRandom()
        {
            var l = new List<ItemBase>();
            for (int i = 0; i < 16; i++)
            {
                l.Add(PlayData.GetPassiveRandom());
            }
            SelectItemUI(l);
        }


        internal static void SelectItemUI(List<ItemBase> l)
        {
            UIManager.InstantiateActive(UIManager.inst.SelectItemUI).GetComponent<SelectItemUI>().Init(l);
        }


        internal static void RewardGetItem(string i)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                InventoryManager.Reward(ItemBase.GetItem(i, 10));
            }
            else
                InventoryManager.Reward(ItemBase.GetItem(i));
        }
        internal static void Reward(string i)
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

        internal static void ArtifactPlusInvenReward(int c=4)
        {
            List<ItemBase> list12 = new List<ItemBase>();

            //for (int i = 0; i < items["Item_Passive_"].Count - 4; i++)
            for (int i = 0; i < c; i++)
            {
                list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_ArtifactPlusInven));
            }

            InventoryManager.Reward(list12);
        }

        internal static void ArtifactPlusInvenCheat()
        {
            if( PlayData.TSavedata.ArkPassivePlus < 32)
            {
                PlayData.TSavedata.ArkPassivePlus = 32;
                for (int m = PlayData.TSavedata.Passive_Itembase.Count; m < PlayData.TSavedata.ArkPassivePlus; m++)
                {
                    PlayData.TSavedata.Passive_Itembase.Add(null);
                }
            }
        }

        internal static void ScrollReward()
        {
            List<ItemBase> list12 = new List<ItemBase>();
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Enchant, 9));
            //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Mapping, 9));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Uncurse, 9));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Transfer, 9));

            InventoryManager.Reward(list12);
        }


        internal static void ItemReward()
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

        internal static void PassiveReward()
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
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Crossoflight));

            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_MindsEye));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_WitchRelic));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_OldHourglass));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Revolvercylinder));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Sunset));

            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_RelicBox));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_BlackFlag));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Tumble));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Markofdeath));

            //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_WaterplayTube_SwimDLC));
            //list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Passive_Suitecase_SwimDLC));
            InventoryManager.Reward(list12);
        }

        internal static void EquipsReward()
        {
            List<ItemBase> list12 = new List<ItemBase>();

            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_FlameDarkSword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_SwordOfStar));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb_0));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_DarkPrestClothes));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Arrowofangel));
            // etc
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Replica));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_HalfMask));
            // atk
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_BlackMoonSword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Sword));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Ilya_Sword_0));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Ilya_Sword_1));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_MessiahbladesPrototype));
            // sup
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Cape));
            // def
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_Revenger));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_King_Armor));
            list12.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_BlackSpikedArmor));
            InventoryManager.Reward(list12);
        }

        internal static void ArkPartsInvenAddNewItem()
        {
            throw new NotImplementedException();
        }
    }
}
