using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool m_isPlayerUnit;
    [SerializeField] BattleHud m_hud;

    public bool IsPlayerUnit
    {
        get { return m_isPlayerUnit; }
    }

    public BattleHud Hud
    {
        get { return m_hud; }
    }

    public Pokemon Pokemon { get; set; }

    public GameObject poke;

    Animator m_anim;

    private void Update()
    {
        m_anim = GetComponentInChildren<Animator>();
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (m_isPlayerUnit)
        {
            //選んでる自分のゲームオブジェクトを子オブジェクトとして表示する
            poke = GameObject.Instantiate(Pokemon.Base.PokemonObject, transform);
        }
        else
        {
            //選ばれた敵のゲームオブジェクトを子オブジェクトとして表示する
            poke = GameObject.Instantiate(Pokemon.Base.PokemonObject, transform);
        }
        m_hud.gameObject.SetActive(true);
        m_hud.SetData(pokemon);
    }

    public void Clear()
    {
        m_hud.gameObject.SetActive(false);
    }

    /// <summary>
    /// ポケモンの攻撃
    /// </summary>
    public void PokemonAttack()
    {
        m_anim.SetTrigger("Attack");
    }

    /// <summary>
    /// ポケモンがダメージを受けたとき
    /// </summary>
    public void PokemonHit()
    {
        m_anim.SetTrigger("Hit");
    }

    /// <summary>
    /// ポケモンが気絶したとき
    /// </summary>
    public void PokemonDie()
    {
        m_anim.SetTrigger("Die");
    }

    /// <summary>
    /// ポケモンが気絶したとき
    /// </summary>
    public void GameObjectDestroy()
    {
        Destroy(poke);
    }
}
