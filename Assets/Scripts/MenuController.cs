using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject m_menu;
    List<Text> m_menuItems;

    int m_selectedItem = 0;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    private void Awake()
    {
        m_menuItems = m_menu.GetComponentsInChildren<Text>().ToList();
    }

    /// <summary>
    /// メニューを開く
    /// </summary>
    public void OpenMenu()
    {
        m_menu.SetActive(true);
        UpdateItemSelection();
    }

    /// <summary>
    /// メニューを閉じる
    /// </summary>
    public void CloseMenu()
    {
        m_menu.SetActive(false);
    }

    /// <summary>
    /// メニューの操作方法
    /// </summary>
    public void HandleUpdate()
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

        m_selectedItem = Mathf.Clamp(m_selectedItem, 0, m_menuItems.Count - 1);

        if (prevSelection != m_selectedItem)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            onMenuSelected?.Invoke(m_selectedItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    /// <summary>
    /// 選択中の色を変える
    /// </summary>
    void UpdateItemSelection()
    {
        for (int i = 0; i < m_menuItems.Count; i++)
        {
            if (i == m_selectedItem)
            {
                m_menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                m_menuItems[i].color = Color.black;
            }
        }
    }
}
