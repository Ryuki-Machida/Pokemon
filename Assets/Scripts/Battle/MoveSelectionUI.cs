using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> m_moveText;
    [SerializeField] Color m_highlightedColor;

    int m_currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            m_moveText[i].text = currentMoves[i].Name;
        }

        m_moveText[currentMoves.Count].text = newMove.Name;
    }

    /// <summary>
    /// 操作
    /// </summary>
    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++m_currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --m_currentSelection;
        }

        m_currentSelection = Mathf.Clamp(m_currentSelection, 0, PokemonBase.MaxNumOfMoves);

        UpdateMoveSelection(m_currentSelection);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            onSelected?.Invoke(m_currentSelection);
        }
    }

    /// <summary>
    /// 選択中のtextの色を変える
    /// </summary>
    /// <param name="selection"></param>
    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves + 1; i++)
        {
            if (i == selection)
            {
                m_moveText[i].color = m_highlightedColor;
            }
            else
            {
                m_moveText[i].color = Color.black;
            }
        }
    }
}
