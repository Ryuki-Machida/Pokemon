using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> m_wildPokemons;

    public Pokemon GetRandomWildPokemon()
    {
        var wildPokemon = m_wildPokemons[Random.Range(0, m_wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }
}
