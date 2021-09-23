using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Inventory : MonoBehaviour
{
    /// <summary>アイテムの種類</summary>
    [SerializeField] List<ItemSlot> m_slots;

    public event Action OnUpdated;

    public List<ItemSlot> Slots
    {
        get { return m_slots; }
    }

    /// <summary>
    /// アイテム使用
    /// </summary>
    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon)
    {
        var item = Slots[itemIndex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }

        return null;
    }

    /// <summary>
    /// アイテムの個数管理
    /// </summary>
    public void RemoveItem(ItemBase item)
    {
        var itemSlot = m_slots.First(m_slots => m_slots.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            m_slots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
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
        get
        {
            return m_count;
        }
        set
        {
            m_count = value;
        }
    }
}
