using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    /// <summary>ポケモンを選択中に表示するtext</summary>
    [SerializeField] Text m_messageText;
    /// <summary>Script</summary>
    PartyMemberUI[] m_memberSlots;
    /// <summary>ボックス内のポケモン</summary>
    List<Pokemon> m_pokemons;
    PokemonParty m_party;

    int m_selection = 0;

    public Pokemon SelectedMember
    {
        get { return m_pokemons[m_selection]; }
    }

    /// <summary>
    /// パーティ画面は、ActionSelection、Running、AboutToUseなどのさまざまな状態から呼び出す
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        m_memberSlots = GetComponentsInChildren<PartyMemberUI>();

        m_party = PokemonParty.GetPlayerParty();
        SetPartyData();

        m_party.OnUpdated += SetPartyData;
    }

    /// <summary>
    /// ボックスにいるポケモンの数に応じて表示する
    /// </summary>
    public void SetPartyData()
    {
        m_pokemons = m_party.Pokemons;

        for (int i = 0; i < m_memberSlots.Length; i++)
        {
            if (i < m_pokemons.Count)
            {
                m_memberSlots[i].Init(m_pokemons[i]);
            }
            else
            {
                m_memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelected(m_selection);

        m_messageText.text = "入れ替えるポケモン";
    }

    /// <summary>
    /// パーティー画面の操作方法
    /// </summary>
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = m_selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++m_selection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --m_selection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_selection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_selection -= 2;
        }

        m_selection = Mathf.Clamp(m_selection, 0, m_pokemons.Count - 1);

        if (m_selection != prevSelection)
        {
            UpdateMemberSelected(m_selection);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            onBack?.Invoke();
        }
    }

    /// <summary>
    /// 選択されてたら色を表示
    /// </summary>
    public void UpdateMemberSelected(int selectedMember)
    {
        for (int i = 0; i < m_pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                m_memberSlots[i].SetSelected(true);
            }
            else
            {
                m_memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        m_messageText.text = message;
    }
}
