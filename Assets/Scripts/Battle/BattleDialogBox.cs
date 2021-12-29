using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    /// <summary>文字を表示する速さ</summary>
    [SerializeField] int m_lettersPerSecond;

    [SerializeField] GameObject m_dialogBox;
    [SerializeField] Text m_dialogText;
    [SerializeField] GameObject m_actionSelector;
    [SerializeField] GameObject m_moveSelector;
    [SerializeField] GameObject m_moveDetails;
    [SerializeField] GameObject m_choicBox;

    [SerializeField] List<Image> m_actionImage;
    [SerializeField] List<Text> m_actionText;
    [SerializeField] List<Image> m_typeImage;
    [SerializeField] List<Text> m_moveText;
    [SerializeField] List<Image> m_moveImage;

    [SerializeField] List<Text> m_ppText;

    [SerializeField] Text m_yesText;
    [SerializeField] Text m_noText;
    [SerializeField] Image m_yesImage;
    [SerializeField] Image m_noImage;

    Color m_highlightedColor;

    private void Start()
    {
        m_highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        m_dialogText.text = dialog;
    }

    /// <summary>
    /// textを一文字ずつ表示する
    /// </summary>
    public IEnumerator TypeDialog(string dialog)
    {
        m_dialogText.text = " ";
        foreach (var letter in dialog.ToCharArray())
        {
            m_dialogText.text += letter;
            yield return new WaitForSeconds(1f / m_lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 文の表示、非表示
    /// </summary>
    public void EnableDialogText(bool enabled)
    {
        m_dialogBox.SetActive(enabled);
        m_dialogText.enabled = enabled;
    }

    /// <summary>
    /// 行動選択の表示、非表示
    /// </summary>
    public void EnableActionSelector(bool enabled)
    {
        m_actionSelector.SetActive(enabled);
    }

    /// <summary>
    /// わざの表示、非表示
    /// </summary>
    public void EnableMoveSelector(bool enabled)
    {
        m_moveSelector.SetActive(enabled);
        m_moveDetails.SetActive(enabled);
    }

    /// <summary>
    /// 交換するかの表示、非表示
    /// </summary>
    public void EndbleChoiceBox(bool enabled)
    {
        m_choicBox.SetActive(enabled);
    }

    /// <summary>
    /// 行動選択中の色を変える
    /// </summary>
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < m_actionImage.Count; i++)
        {
            if (i == selectedAction)
            {
                m_actionImage[i].color = Color.black;
            }
            else
            {
                m_actionImage[i].color = Color.white;
            }
        }

        for (int i = 0; i < m_actionText.Count; i++)
        {
            if (i == selectedAction)
            {
                m_actionText[i].color = Color.white;
            }
            else
            {
                m_actionText[i].color = Color.black;
            }
        }
    }

    /// <summary>
    /// わざ選択中の処理
    /// </summary>
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        //わざ選択中の色を変える
        for (int i = 0; i < m_moveImage.Count; i++)
        {
            if (i == selectedMove)
            {
                m_moveImage[i].color = Color.black;
            }
            else
            {
                m_moveImage[i].color = new Color(0, 0, 0, 0);
            }
        }
        //他のわざを選んでもPPが変わらないようにしてる
        for (int i = 0; i < m_ppText.Count; i++)
        {
            if (i == selectedMove)
            {
                m_ppText[i].text = $"PP{move.PP}/{move.Base.PP}";
            }
        }
    }

    /// <summary>
    /// text、色のセット
    /// </summary>
    public void SetMoveNames(List<Move> moves)
    {
        //それぞれの技の名前
        for (int i = 0; i < m_moveText.Count; i++)
        {
            if (i < moves.Count)
            {
                m_moveText[i].text = moves[i].Base.Name;
            }
            else
            {
                m_moveText[i].text = "-";
            }
        }

        //それぞれの技のPP
        for (int i = 0; i < m_ppText.Count; i++)
        {
            if (i < moves.Count)
            {
                m_ppText[i].text = $"PP{moves[i].PP}/{moves[i].Base.PP}";
            }
            else
            {
                m_ppText[i].text = "-";
            }
        }

        //それぞれの技のタイプカラー
        for (int i = 0; i < m_typeImage.Count; i++)
        {
            if (i < moves.Count)
            {
                m_typeImage[i].sprite = moves[i].Base.TypeSprite;
            }
        }
    }

    /// <summary>
    /// 交換を選ぶ時のtext,image
    /// </summary>
    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            m_yesImage.color = Color.black;
            m_yesText.color = Color.white;
            m_noImage.color = Color.white;
            m_noText.color = Color.black;
        }
        else
        {
            m_noImage.color = Color.black;
            m_noText.color = Color.white;
            m_yesImage.color = Color.white;
            m_yesText.color = Color.black;
        }
    }
}
