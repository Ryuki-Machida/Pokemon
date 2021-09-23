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

    public void Init(Pokemon pokemon)
    {
        m_pokemon = pokemon;
        UpdateDate();

        m_pokemon.OnHPChanged += UpdateDate;
    }

    /// <summary>
    /// 更新
    /// </summary>
    void UpdateDate()
    {
        m_nameText.text = m_pokemon.Base.Name;
        m_levelText.text = "Lv " + m_pokemon.Level;
        m_hpBar.SetHP((float)m_pokemon.HP / m_pokemon.MaxHp);
        m_hpText.text = m_pokemon.HP + "/" + m_pokemon.MaxHp.ToString();
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
