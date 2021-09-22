using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    /// <summary>アイテムの名前</summary>
    [SerializeField] Text m_nameText;
    /// <summary>個数</summary>
    [SerializeField] Text m_countText;

    public Text NameText
    {
        get { return m_nameText; }
    }

    public Text Count
    {
        get { return m_countText; }
    }

    public void SetData(ItemSlot itemSlot)
    {
        m_nameText.text = itemSlot.Item.Name;
        m_countText.text = $"{itemSlot.Count}個";
    }
}
