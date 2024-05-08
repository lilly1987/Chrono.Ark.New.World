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
using UnityEngine.UI;

namespace BepInPluginSample
{
    internal class MyItemCollection
    {
        static internal ItemCollection itemCollection;
        static private List<string> ConsumableKeys = new List<string>();
        static private List<string> EquipKeys = new List<string>();
        static private List<string> MiscKeys = new List<string>();
        static private List<string> PassiveKeys = new List<string>();
        static private List<string> PotionsKeys = new List<string>();
        static private List<string> ScrollKeys = new List<string>();
        static private List<string> UsefulKeys = new List<string>();

        static public Dictionary<string, ItemBase> ItemDic = new Dictionary<string, ItemBase>();
        static public List<ItemBase> ItemList = new List<ItemBase>();
        static public List<ItemBase> EquipList = new List<ItemBase>();
        static public List<ItemBase> PassiveList = new List<ItemBase>();
        static public List<ItemBase> UsefulList = new List<ItemBase>();
        static public List<ItemBase> SwimDLCList = new List<ItemBase>();
        static public List<ItemBase> MiscList = new List<ItemBase>();

        internal static void Init()
        {
            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Consume, out MyItemCollection.ConsumableKeys);
		    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Equip, out MyItemCollection.EquipKeys);
		    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Misc, out MyItemCollection.MiscKeys);
		    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Passive, out MyItemCollection.PassiveKeys);
		    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Potions, out MyItemCollection.PotionsKeys);
		    GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Item_Scroll, out MyItemCollection.ScrollKeys);

            foreach (string key in MiscKeys)
            {
                MiscList.Add(ItemBase.GetItem(key));
            }
            //EquipKeys
            foreach (string key in EquipKeys)
            {
                EquipList.Add(ItemBase.GetItem(key));
            }
            //PassiveKeys
            foreach (string key2 in PassiveKeys)
            {
                PassiveList.Add(ItemBase.GetItem(key2));
            }
            //UsefulKeys
            foreach (string text2 in PotionsKeys)
            {
                UsefulList.Add(ItemBase.GetItem(text2));
                UsefulKeys.Add(text2);
            }
            foreach (string text3 in ScrollKeys)
            {
                UsefulList.Add(ItemBase.GetItem(text3));
                UsefulKeys.Add(text3);
            }
            foreach (string text4 in ConsumableKeys)
            {
                UsefulList.Add(ItemBase.GetItem(text4));
                UsefulKeys.Add(text4);
            }

            foreach (ItemBase item in MiscList)
            {
                ItemList.Add(item);
            }
            foreach (ItemBase item in EquipList)
            {
                ItemList.Add(item);
            }
            foreach (ItemBase item2 in PassiveList)
            {
                ItemList.Add(item2);
            }
            foreach (ItemBase item3 in UsefulList)
            {
                ItemList.Add(item3);
            }

            foreach (ItemBase item3 in ItemList)
            {
                ItemDic.Add(item3.itemkey, item3);
            }
        }
    }
}