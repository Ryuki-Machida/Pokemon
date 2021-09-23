using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag, Cutscene }

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
    [SerializeField] InventoryUI m_inventoryUI;
    [SerializeField] PartyScreen m_partyScreen;

    GameState state;
    TrainerController m_trainerController;
    MenuController m_menuController;

    public static GamaManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        m_menuController = GetComponent<MenuController>();

        ConditionDB.Init();
    }

    private void Start()
    {
        m_player.OnEncountered += Fade;
        m_battleManager.OnBattleOver += EndBattle;

        m_partyScreen.Init();

        m_player.OnTrainersView += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if (trainer != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TrainerBattle(m_player));
            }
        };

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

        m_menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        m_menuController.onMenuSelected += OnMenuSelected;
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
        state = GameState.Battle;
        m_battleManager.gameObject.SetActive(true);
        m_worldCamera.gameObject.SetActive(false);

        var playerParty = m_player.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        m_battleManager.StartBattle(playerParty, wildPokemon);
    }

    /// <summary>
    /// トレーナーバトルが始まった時の処理
    /// </summary>
    public void StartTrainerBattle(TrainerController  trainer)
    {
        state = GameState.Battle;
        m_battleManager.gameObject.SetActive(true);
        m_worldCamera.gameObject.SetActive(false);

        this.m_trainerController = trainer;
        var playerParty = m_player.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        m_battleManager.StartTrainerBattle(playerParty, trainerParty);
    }

    /// <summary>
    /// バトルではない時の処理
    /// </summary>
    void EndBattle(bool won)
    {
        if (m_trainerController != null && won == true)
        {
            m_trainerController.BattleLost();
            m_trainerController = null;
        }

        state = GameState.FreeRoam;
        m_battleManager.gameObject.SetActive(false);
        m_worldCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            m_player.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.M))
            {
                m_menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            m_battleManager.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            m_menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {

            };

            Action onBack = () =>
            {
                m_partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            m_partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                m_inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            m_inventoryUI.HandleUpdate(onBack);
        }
    }

    /// <summary>
    /// メニュー
    /// </summary>
    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //モンスター
            m_partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //バック
            m_inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
    }
}
