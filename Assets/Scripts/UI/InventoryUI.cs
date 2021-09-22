using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バックの管理
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject m_itemList;
    [SerializeField] ItemSlotUI m_itemSlotUI;

    [SerializeField] Image m_itemIcon;
    [SerializeField] Text m_itemDescription;

    int m_selectedItem = 0;

    List<ItemSlotUI> m_slotUIList;
    Inventory m_inventory;

    private void Awake()
    {
        m_inventory = Inventory.GetInventory();
    }

    private void Start()
    {
        UpdateItemList();
    }

    void UpdateItemList()
    {
        //既存のアイテムをすべてきれいする
        foreach (Transform child in m_itemList.transform)
        {
            Destroy(child.gameObject);
        }

        m_slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in m_inventory.Slots)
        {
            var slotUIobj = Instantiate(m_itemSlotUI, m_itemList.transform);
            slotUIobj.SetData(itemSlot);

            m_slotUIList.Add(slotUIobj);
        }

        UpdateItemSelection();
    }

    /// <summary>
    /// バックの操作方法
    /// </summary>
    public void HandleUpdate(Action onBack)
    {
        int prevSelection = m_selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++m_selectedItem;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --m_selectedItem;
        }

        m_selectedItem = Mathf.Clamp(m_selectedItem, 0, m_inventory.Slots.Count - 1);

        if (prevSelection != m_selectedItem)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            onBack?.Invoke();
        }
    }

    /// <summary>
    /// 選択中の色を変える
    /// </summary>
    void UpdateItemSelection()
    {
        for (int i = 0; i < m_slotUIList.Count; i++)
        {
            if (i == m_selectedItem)
            {
                m_slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                m_slotUIList[i].NameText.color = Color.black;
            }
        }

        var item = m_inventory.Slots[m_selectedItem].Item;
        m_itemIcon.sprite = item.Icon;
        m_itemDescription.text = item.Description;
    }
}
