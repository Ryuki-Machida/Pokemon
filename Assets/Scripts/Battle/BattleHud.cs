using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ポケモンのUI
/// </summary>
public class BattleHud : MonoBehaviour
{
    [SerializeField] Text m_nameText;
    [SerializeField] Text m_levelText;
    [SerializeField] HPBar m_hpBar;
    [SerializeField] Text m_hpText;
    [SerializeField] Text m_statusText;
    [SerializeField] Image m_statusImage;
    [SerializeField] GameObject m_expBar;

    [SerializeField] Color m_psnColor;
    [SerializeField] Color m_brnColor;
    [SerializeField] Color m_slpColor;
    [SerializeField] Color m_parColor;
    [SerializeField] Color m_frzColor;

    Pokemon m_pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        if (m_pokemon != null)
        {
            m_pokemon.OnHPChanged -= UpdateHP;
            m_pokemon.OnStatusChamged -= SetStatusUI;
        }

        m_pokemon = pokemon;

        m_nameText.text = pokemon.Base.Name;
        SetLevel();
        m_hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        m_hpText.text = pokemon.HP + "/" + pokemon.MaxHp.ToString();
        SetExp();

        //状態異常のUIのカラーを設定
        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.どく, m_psnColor },
            {ConditionID.やけど, m_brnColor },
            {ConditionID.ねむり, m_slpColor },
            {ConditionID.まひ, m_parColor },
            {ConditionID.こおり, m_frzColor },
        };

        SetStatusUI();
        m_pokemon.OnStatusChamged += SetStatusUI;
        m_pokemon.OnHPChanged += UpdateHP;
    }

    /// <summary>
    /// 状態異常のときに出すUI
    /// </summary>
    void SetStatusUI()
    {
        if (m_pokemon.Status == null)
        {
            m_statusText.text = "";
            m_statusImage.color = Color.white;
        }
        else
        {
            m_statusText.text = m_pokemon.Status.ID.ToString().ToUpper();
            m_statusImage.color = statusColors[m_pokemon.Status.ID];
        }
    }

    public void SetLevel()
    {
        m_levelText.text = "Lv " + m_pokemon.Level;
        //m_hpText.text = m_pokemon.HP + "/" + m_pokemon.MaxHp.ToString();
    }

    public void SetExp()
    {
        if (m_expBar == null)
        {
            return;
        }

        float normalizedExp = GetNormalizedExp();
        m_expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (m_expBar == null)
        {
            yield break;
        }

        if (reset) //レベルが上がったら
        {
            m_expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return m_expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion(); //結果を渡してから命令を検索する
    }

    /// <summary>
    /// 計算方したExpを取得する
    /// </summary>
    float GetNormalizedExp()
    {
        int currLevelExp = m_pokemon.Base.GetExpForLevel(m_pokemon.Level);
        int nextLevelExp = m_pokemon.Base.GetExpForLevel(m_pokemon.Level + 1);

        float normalizedExp = (float)(m_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp); //Mathf.Clamp01(0から1の間に制限し、float型の値が返る)
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    /// <summary>
    /// HPの更新
    /// </summary>
    public IEnumerator UpdateHPAsync()
    {
        yield return m_hpBar.SetHPSmooth((float)m_pokemon.HP / m_pokemon.MaxHp);
        yield return m_hpText.text = m_pokemon.HP + "/" + m_pokemon.MaxHp.ToString();
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => m_hpBar.IsUpdating == false);
    }
}
