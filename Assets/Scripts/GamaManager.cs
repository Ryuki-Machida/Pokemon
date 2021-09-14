using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog }

public class GamaManager : MonoBehaviour
{
    /// <summary>Script</summary>
    [SerializeField] PlayerController m_player;
    /// <summary>Script</summary>
    [SerializeField] BattleManager m_battleManager;
    /// <summary>Map用のカメラ</summary>
    [SerializeField] Camera m_worldCamera;
    /// <summary>Script</summary>
    [SerializeField] FadeManager m_fade;

    GameState state;

    private void Awake()
    {
        ConditionDB.Init();
    }

    private void Start()
    {
        m_player.OnEncountered += Fade;
        m_battleManager.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    /// <summary>
    /// フェード
    /// </summary>
    void Fade()
    {
        state = GameState.Battle;
        m_fade.OnBattle();
        Invoke("StartBattle", 1);
    }

    /// <summary>
    /// バトルが始まった時の処理
    /// </summary>
    void StartBattle()
    {
        m_battleManager.gameObject.SetActive(true);
        m_worldCamera.gameObject.SetActive(false);

        var playerParty = m_player.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        m_battleManager.StartBattle(playerParty, wildPokemon);
    }

    /// <summary>
    /// バトルではない時の処理
    /// </summary>
    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        m_battleManager.gameObject.SetActive(false);
        m_worldCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            m_player.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            m_battleManager.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }
}
