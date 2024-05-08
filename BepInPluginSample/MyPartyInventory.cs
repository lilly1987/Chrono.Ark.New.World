using System;
using System.Collections.Generic;
using UnityEngine;

namespace BepInPluginSample
{
    internal class MyPartyInventory
    {
        
        public static void ChangeMaxInventoryNum(int num, List<string> ExceptItems = null)
        {
            PlayData.TSavedata.MaxinventoryNumPlus += num;
            if (num < 0)
            {
                int num2 = num * -1;
                List<ItemBase> list = new List<ItemBase>();
                int num3 = PartyInventory.InvenM.Align.transform.childCount - 1;
                while (num3 >= 0 && num2 > 0)
                {
                    Transform child = PartyInventory.InvenM.Align.transform.GetChild(num3);
                    ItemSlot component = child.GetComponent<ItemSlot>();
                    if (component.Item != null && component.Item.GetComponent<ItemObject>().Item != null && !ExceptItems.Contains(component.Item.GetComponent<ItemObject>().Item.itemkey))
                    {
                        list.Add(component.Item.GetComponent<ItemObject>().Item);
                    }
                    if (num3 < PlayData.TSavedata.Inventory.Count)
                    {
                        PlayData.TSavedata.Inventory.RemoveAt(num3);
                    }
                    for (int i = 0; i < child.childCount; i++)
                    {
                        if (child.GetChild(i).GetComponent<ItemObject>() != null)
                        {
                            UnityEngine.Object.Destroy(child.GetChild(i).gameObject);
                        }
                    }
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                    num2--;
                    num3--;
                }
                list.Reverse();
                PartyInventory.InvenM.ItemUpdateFromInven();
                if (list.Count > 0)
                {
                    InventoryManager.Reward(list);
                }
            }
            else if (num>0)
            {
                int num2 = num * 1;
                List<ItemBase> list = new List<ItemBase>();
                int num3 = PartyInventory.InvenM.Align.transform.childCount - 1;
                while (num3 >= 0 && num2 > 0)
                {
                    Transform child = PartyInventory.InvenM.Align.transform.GetChild(num3);
                    ItemSlot component = child.GetComponent<ItemSlot>();
                    if (component.Item != null && component.Item.GetComponent<ItemObject>().Item != null && !ExceptItems.Contains(component.Item.GetComponent<ItemObject>().Item.itemkey))
                    {
                        list.Add(component.Item.GetComponent<ItemObject>().Item);
                    }
                    if (num3 < PlayData.TSavedata.Inventory.Count)
                    {
                        PlayData.TSavedata.Inventory.RemoveAt(num3);
                    }
                    for (int i = 0; i < child.childCount; i++)
                    {
                        if (child.GetChild(i).GetComponent<ItemObject>() != null)
                        {
                            UnityEngine.Object.Destroy(child.GetChild(i).gameObject);
                        }
                    }
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                    num2--;
                    num3--;
                }
                list.Reverse();
                PartyInventory.InvenM.ItemUpdateFromInven();
                if (list.Count > 0)
                {
                    InventoryManager.Reward(list);
                }
            }
        }

    }
}