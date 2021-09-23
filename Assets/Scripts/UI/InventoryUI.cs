using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バックの管理
/// </summary>

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject m_itemList;
    [SerializeField] ItemSlotUI m_itemSlotUI;

    [SerializeField] Image m_itemIcon;
    [SerializeField] Text m_itemDescription;

    [SerializeField] PartyScreen m_partyScreen;
    [SerializeField] SoundManager m_soundManager;

    int m_selectedItem = 0;
    InventoryUIState state;

    Action onItemUsed;

    List<ItemSlotUI> m_slotUIList;
    Inventory m_inventory;

    private void Awake()
    {
        m_inventory = Inventory.GetInventory();
    }

    private void Start()
    {
        UpdateItemList();

        m_inventory.OnUpdated += UpdateItemList;
    }

    /// <summary>
    /// アイテムの更新
    /// </summary>
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
    public void HandleUpdate(Action onBack, Action onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            //バック
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OpenPartyScreen();
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            //パーティー画面
            Action onselected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            m_partyScreen.HandleUpdate(onselected, onBackPartyScreen);
        }
    }

    /// <summary>
    /// アイテムを使うときのtext
    /// </summary>
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        var usedItem = m_inventory.UseItem(m_selectedItem, m_partyScreen.SelectedMember);
        if (usedItem != null)
        {
            m_soundManager.ItemUse();
            yield return DialogManager.Instance.ShowDialogText($"{usedItem.Name}を使った");
            onItemUsed?.Invoke();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"使っても効果ありません");
        }

        ClosePartyScreen();
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

    /// <summary>
    /// アイテムを選択したらパーティー画面
    /// </summary>
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        m_partyScreen.gameObject.SetActive(true);
    }

    /// <summary>
    /// アイテム画面に戻る
    /// </summary>
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        m_partyScreen.gameObject.SetActive(false);
    }
}
