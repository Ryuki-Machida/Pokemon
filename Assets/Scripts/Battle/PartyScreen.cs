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

    public void Init()
    {
        m_memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    /// <summary>
    /// ボックスにいるポケモンの数に応じて表示する
    /// </summary>
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.m_pokemons = pokemons;

        for (int i = 0; i < m_memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                m_memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                m_memberSlots[i].gameObject.SetActive(false);
            }
        }
        m_messageText.text = "入れ替えるポケモン";
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
