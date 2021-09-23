using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    /// <summary>色を変えるimage</summary>
    [SerializeField] Image m_image;
    /// <summary>名前のtext</summary>
    [SerializeField] Text m_nameText;
    /// <summary>レベルのtext</summary>
    [SerializeField] Text m_levelText;
    /// <summary>HPバー</summary>
    [SerializeField] HPBar m_hpBar;
    /// <summary>HPのtext</summary>
    [SerializeField] Text m_hpText;

    /// <summary>Pokemon クラス</summary>
    Pokemon m_pokemon;

    public void SetData(Pokemon pokemon)
    {
        m_pokemon = pokemon;

        m_nameText.text = pokemon.Base.Name;
        m_levelText.text = "Lv " + pokemon.Level;
        m_hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        m_hpText.text = pokemon.HP + "/" + pokemon.MaxHp.ToString();
    }

    /// <summary>
    /// 選択中の色の設定
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            m_image.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            m_image.color = Color.white;
        }
    }
}
