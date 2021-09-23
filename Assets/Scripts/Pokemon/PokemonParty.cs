using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PokemonParty : MonoBehaviour
{
    /// <summary>パーティー内の数</summary>
    [SerializeField] List<Pokemon> m_pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return m_pokemons;
        }
        set
        {
            m_pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Start()
    {
        foreach (var pokemon in m_pokemons)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {
        return m_pokemons.Where(x => x.HP > 0).FirstOrDefault(); //条件を満たす要素が一つもなかった場合nullが返る
    }

    /// <summary>
    /// プレイヤーのパーティー情報
    /// </summary>
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
