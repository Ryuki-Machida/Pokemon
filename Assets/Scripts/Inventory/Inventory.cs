using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    /// <summary>アイテムの種類</summary>
    [SerializeField] List<ItemSlot> m_slots;

    public List<ItemSlot> Slots
    {
        get { return m_slots; }
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    /// <summary>アイテム</summary>
    [SerializeField] ItemBase m_item;
    /// <summary>個数</summary>
    [SerializeField] int m_count;

    public ItemBase Item
    {
        get { return m_item; }
    }

    public int Count
    {
        get { return m_count; }
    }
}
