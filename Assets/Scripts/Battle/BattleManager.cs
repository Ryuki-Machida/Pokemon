using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget , BattleOver }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleManager : MonoBehaviour
{
    /// <summary>Script<summary>
    [SerializeField] BattleUnit m_playerUnit;
    [SerializeField] BattleUnit m_enemyUnit;
    [SerializeField] BattleDialogBox m_dialogBox;
    [SerializeField] PartyScreen m_partyScreen;
    [SerializeField] MoveSelectionUI m_moveSelectionUI;
    [SerializeField] InventoryUI m_inventoryUI;
    [SerializeField] BattleCamera m_battleCamera;
    [SerializeField] SoundManager m_soundManager;

    [SerializeField] GameObject m_PlayerHud;
    [SerializeField] GameObject m_EnemyHud;
    [SerializeField] GameObject m_player;
    [SerializeField] GameObject m_trainer;

    public event Action<bool> m_OnBattleOver;

    BattleState m_state;
    
    int m_currentAction;
    int m_currentMove;
    bool m_aboutToUseChoice;

    PokemonParty m_playerParty;
    PokemonParty m_trainerParty;
    Pokemon m_wildPokemon;

    bool isTrainerBattle = false;
    PlayerController m_Player;
    TrainerController m_Trainer;

    int m_escapeAttempts;
    MoveBase m_moveToLearn;

    /// <summary>
    /// バトル
    /// </summary>
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.m_playerParty = playerParty;
        this.m_wildPokemon = wildPokemon;
        m_Player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// トレーナ戦
    /// </summary>
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.m_playerParty = playerParty;
        this.m_trainerParty = trainerParty;

        isTrainerBattle = true;
        m_Player = playerParty.GetComponent<PlayerController>();
        m_Trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// バトルが始まった時の処理
    /// </summary>
    public IEnumerator SetupBattle()
    {
        m_playerUnit.Clear();
        m_enemyUnit.Clear();

        if (!isTrainerBattle)  //野生のポケモンバトル
        {
            m_player.gameObject.SetActive(true);

            m_playerUnit.Setup(m_playerParty.GetHealthyPokemon());
            m_enemyUnit.Setup(m_wildPokemon);

            m_EnemyHud.SetActive(true);
            m_dialogBox.SetMoveNames(m_playerUnit.Pokemon.Moves);

            m_battleCamera.EnemyCamera();
            yield return m_dialogBox.TypeDialog($"あっ!\n 野生の{m_enemyUnit.Pokemon.Base.Name}が飛び出して来た！");
            yield return new WaitForSeconds(0.2f);
            m_battleCamera.PlayerCamera();
            yield return m_dialogBox.TypeDialog($"ゆけっ!\n{m_playerUnit.Pokemon.Base.Name}！");
            yield return new WaitForSeconds(1f);
        }
        else  //トレーナーバトル
        {
            m_playerUnit.gameObject.SetActive(false);
            m_enemyUnit.gameObject.SetActive(false);

            m_player.gameObject.SetActive(true);
            m_trainer.gameObject.SetActive(true);

            m_battleCamera.EnemyCamera();
            yield return m_dialogBox.TypeDialog($"{m_Trainer.Name}が\n 勝負を仕掛けてきた！");

            //トレーナーの最初のポケモン
            m_enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = m_trainerParty.GetHealthyPokemon();
            m_enemyUnit.Setup(enemyPokemon);
            yield return m_dialogBox.TypeDialog($"{m_Trainer.Name}は\n {enemyPokemon.Base.Name}をくりだした！");

            //プレイヤーの最初のポケモン
            m_battleCamera.PlayerCamera();
            m_playerUnit.gameObject.SetActive(true);
            var playerPokemon = m_playerParty.GetHealthyPokemon();
            m_playerUnit.Setup(playerPokemon);
            yield return m_dialogBox.TypeDialog($"行け！\n{playerPokemon.Base.Name}！");
            m_dialogBox.SetMoveNames(m_playerUnit.Pokemon.Moves);
            yield return new WaitForSeconds(1f);
        }

        m_escapeAttempts = 0;
        m_partyScreen.Init();
        ActionSelection();
    }

    private void BattleOver(bool won)
    {
        m_state = BattleState.BattleOver;
        m_playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        m_playerUnit.Hud.ClearDate();
        m_enemyUnit.Hud.ClearDate();
        m_OnBattleOver(won);
    }

    /// <summary>
    /// 行動を選択してるとき
    /// </summary>
    private void ActionSelection()
    {
        m_state = BattleState.ActionSelection;
        m_dialogBox.EnableActionSelector(true);
        m_dialogBox.EnableDialogText(false);
        m_battleCamera.MoveingCamera();
    }

    /// <summary>
    /// ポケモンを選択してるとき
    /// </summary>
    private void OpenPartySelection()
    {
        m_partyScreen.CalledFrom = m_state;
        m_state = BattleState.PartyScreen;
        m_partyScreen.gameObject.SetActive(true);
        m_dialogBox.EnableActionSelector(false);
    }

    /// <summary>
    /// バック選択してるとき
    /// </summary>
    private void OpenBag()
    {
        m_state = BattleState.Bag;
        m_inventoryUI.gameObject.SetActive(true);
    }

    /// <summary>
    /// わざを選択しているとき
    /// </summary>
    private void MoveSelection()
    {
        m_state = BattleState.MoveSelection;
        m_dialogBox.EnableActionSelector(false);
        m_dialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// トレーナが次のポケモンを出す前に
    /// </summary>
    private IEnumerator AboutToUse(Pokemon newPokemon)
    {
        m_state = BattleState.Busy;
        yield return m_dialogBox.TypeDialog($"{m_Trainer.Name}は {newPokemon.Base.Name}を出そうとしている\n 交換しますか？");

        m_state = BattleState.AboutToUse;
        m_dialogBox.EndbleChoiceBox(true);
    }

    /// <summary>
    /// わざを入れ替える
    /// </summary>
    private IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        m_state = BattleState.Busy;
        yield return m_dialogBox.TypeDialog($"忘れさせたいワザを選んでください");
        m_moveSelectionUI.gameObject.SetActive(true);
        m_moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        m_moveToLearn = newMove;

        m_state = BattleState.MoveToForget;
    }

    private IEnumerator RunTurns(BattleAction playerAction)
    {
        m_state = BattleState.RunningTurn;
        m_battleCamera.BattlePlayCamera();

        if (playerAction == BattleAction.Move)
        {
            m_playerUnit.Pokemon.CurrentMove = m_playerUnit.Pokemon.Moves[m_currentMove];
            m_enemyUnit.Pokemon.CurrentMove = m_enemyUnit.Pokemon.GetRandomMove();

            //先制攻撃
            int playerMovePriority = m_playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = m_enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //誰が最初か確認
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = m_playerUnit.Pokemon.Speed >= m_enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? m_playerUnit : m_enemyUnit;
            var secondUnit = (playerGoesFirst) ? m_enemyUnit : m_playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //最初のターン
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            if (m_state == BattleState.BattleOver)
            {
                yield break;
            }

            if (secondPokemon.HP > 0)
            {
                //2番目のターン
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit, firstUnit);
                yield return new WaitForSeconds(0.1f);
                yield return RunAfterTurn(firstUnit, secondUnit);
                if (m_state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon) //ポケモン選択
            {
                var selectedPokemon = m_partyScreen.SelectedMember;
                m_state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                //アイテム画面から処理するので、何もせずに敵の移動にスキップする
                m_dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run) //逃げる選択
            {
                yield return TryToEscape();
            }

            //敵のターン
            var enemyMove = m_enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(m_enemyUnit, m_playerUnit, enemyMove);
            yield return RunAfterTurn(m_enemyUnit, m_playerUnit);
            yield return new WaitForSeconds(0.1f);
            yield return RunAfterTurn(m_playerUnit, m_enemyUnit);
            if (m_state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if (m_state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    /// <summary>
    /// 戦闘の中身の処理
    /// </summary>
    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        m_dialogBox.EnableDialogText(true);
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return m_dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}の\n {move.Base.Name}!");

        m_dialogBox.EnableDialogText(false);
        m_EnemyHud.SetActive(true);

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)) //攻撃が当たったら
        {
            //攻撃アニメーション
            m_soundManager.Attack();
            sourceUnit.PokemonAttack();
            yield return new WaitForSeconds(0.8f);
            //受けるアニメーション
            m_soundManager.Hit();
            targetUnit.PokemonHit();
            yield return new WaitForSeconds(0.5f);

            if (move.Base.Category == MoveCategory.Status)
            {
                m_dialogBox.EnableDialogText(true);
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
                yield return new WaitForSeconds(0.5f);
                m_dialogBox.EnableDialogText(false);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();

                m_dialogBox.EnableDialogText(true);

                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            //受けた方のHPが0になったら
            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit, sourceUnit);
            }
        }
        else
        {
            m_dialogBox.EnableDialogText(true);
            yield return m_dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}には\n 攻撃が当たらなかった");
        }
    }

    private IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        //ステータス
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        //状態異常
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // 状態変化
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    private IEnumerator RunAfterTurn(BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (m_state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => m_state == BattleState.RunningTurn);

        //やけど、どく状態だった場合、ターン後にダメージを与える
        m_dialogBox.EnableDialogText(true);
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit, targetUnit);
            yield return new WaitUntil(() => m_state == BattleState.RunningTurn);
        }
    }

    /// <summary>
    /// 命中率
    /// </summary>
    private bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.命中率];
        int evasion = target.StatBoosts[Stat.回避率];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    /// <summary>
    /// ステータスの変更を表示
    /// </summary>
    private IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var massge = pokemon.StatusChanges.Dequeue();
            yield return m_dialogBox.TypeDialog(massge);
        }
    }

    /// <summary>
    /// ポケモンが気絶した後の処理
    /// </summary>
    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, BattleUnit sourceUnit)
    {
        yield return m_dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}は\n たおれた！");
        faintedUnit.PokemonDie();
        yield return new WaitForSeconds(1f);

        faintedUnit.GameObjectDestroy();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //経験値
            //参考計算方法https://bulbapedia.bulbagarden.net/wiki/Experience#Gain_formula
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f; // トレーナ戦だと1.5倍

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            m_playerUnit.Pokemon.Exp += expGain;
            yield return m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}は\n 経験値{expGain}貰った");
            yield return m_playerUnit.Hud.SetExpSmooth();

            //レベルアップ
            while (m_playerUnit.Pokemon.CheckForLevelUp())
            {
                m_soundManager.LevelUp();
                m_playerUnit.Hud.SetLevel();
                yield return m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}のレベルが\n {m_playerUnit.Pokemon.Level}になった！");

                //新しいワザを覚える
                var newMove = m_playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (m_playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        m_playerUnit.Pokemon.LearnMove(newMove);
                        yield return m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}は\n {newMove.Base.Name}を覚えた！");
                        m_dialogBox.SetMoveNames(m_playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}は\n {newMove.Base.Name}を覚えようとしている");
                        yield return m_dialogBox.TypeDialog($"しかし{m_playerUnit.Pokemon.Base.Name}は\n わざを{PokemonBase.MaxNumOfMoves}つ覚えている");
                        yield return ChooseMoveToForget(m_playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => m_state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return m_playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(0.5f);
        }

        CheckForBattleOver(faintedUnit, sourceUnit);
        yield return new WaitUntil(() => m_state == BattleState.RunningTurn);
    }

    /// <summary>
    /// ポケモンがパーティーに残っているか
    /// </summary>
    private void CheckForBattleOver(BattleUnit faintedUnit, BattleUnit destroyUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = m_playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartySelection();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
                destroyUnit.GameObjectDestroy(); //バトルが終わったら残っている方の子オブジェクトを消す
            }
            else  //トレーナー戦
            {
                var nextPokemon = m_trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    BattleOver(true);
                    destroyUnit.GameObjectDestroy(); //バトルが終わったら残っている方の子オブジェクトを消す
                    m_player.gameObject.SetActive(false);
                    m_trainer.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 与えたわざのタイプ相性によってtextの文字を変える
    /// </summary>
    private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return m_dialogBox.TypeDialog("急所に当たった！");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return m_dialogBox.TypeDialog("効果バツグン！");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return m_dialogBox.TypeDialog("効果いまひとつ");
        }
    }

    public void HandleUpdate()
    {
        if (m_state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (m_state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (m_state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (m_state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                m_inventoryUI.gameObject.SetActive(false);
                m_state = BattleState.ActionSelection;
            };

            Action onItemUsed = () =>
            {
                m_state = BattleState.Busy;
                m_inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };

            m_inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (m_state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (m_state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                m_moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //わざを覚えない
                    StartCoroutine(m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}は\n {m_moveToLearn.Name}を覚えなかった"));
                }
                else
                {
                    //選択したわざを忘れ、新しいわざを覚える
                    var selectedMove = m_playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(m_dialogBox.TypeDialog($"{m_playerUnit.Pokemon.Base.Name}は {selectedMove.Name}を忘れ\n {m_moveToLearn.Name}を覚えた！"));

                    m_playerUnit.Pokemon.Moves[moveIndex] = new Move(m_moveToLearn);
                }

                m_moveToLearn = null;
                m_state = BattleState.RunningTurn;
            };

            m_moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    /// <summary>
    /// 行動選択中の処理
    /// </summary>
    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (m_currentAction < 3)
            {
                ++m_currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (m_currentAction > 0)
            {
                --m_currentAction;
            }
        }

        m_dialogBox.UpdateActionSelection(m_currentAction);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_currentAction == 0)
            {
                //たたかう
                MoveSelection();
            }
            else if (m_currentAction == 1)
            {
                //ポケモン
                OpenPartySelection();
            }
            else if (m_currentAction == 2)
            {
                //バック
                OpenBag();
            }
            else if (m_currentAction == 3)
            {
                //にげる
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    /// <summary>
    /// たたかうを選択した場合の処理
    /// </summary>
    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++m_currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --m_currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_currentMove -= 2;
        }

        m_currentMove = Mathf.Clamp(m_currentMove, 0, m_playerUnit.Pokemon.Moves.Count - 1);

        m_dialogBox.UpdateMoveSelection(m_currentMove, m_playerUnit.Pokemon.Moves[m_currentMove]);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var move = m_playerUnit.Pokemon.Moves[m_currentMove];
            if (move.PP == 0)  //PPがなくなったら選択できない
            {
                return;
            }

            m_dialogBox.EnableMoveSelector(false);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            m_dialogBox.EnableMoveSelector(false);
            ActionSelection();
        }
    }

    /// <summary>
    /// Pokemonを選択した場合の処理
    /// </summary>
    private void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = m_partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                m_partyScreen.SetMessageText("ひんしのためバトルに出せません");
                return;
            }
            if (selectedMember == m_playerUnit.Pokemon)
            {
                m_partyScreen.SetMessageText("すでにバトルに出ています");
                return;
            }

            m_partyScreen.gameObject.SetActive(false);

            if (m_partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                m_state = BattleState.Busy;
                bool isTrainerAboutToUse = m_partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            m_partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (m_playerUnit.Pokemon.HP <= 0)
            {
                m_partyScreen.SetMessageText("手持ちにポケモンが残ってない");
                return;
            }

            m_partyScreen.gameObject.SetActive(false);

            if (m_partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }

            m_partyScreen.CalledFrom = null;
        };

        m_partyScreen.HandleUpdate(onSelected, onBack);
    }

    /// <summary>
    /// 次のポケモンが出る前にポケモンを交換するか
    /// </summary>
    private void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_aboutToUseChoice = !m_aboutToUseChoice;
        }

        m_dialogBox.UpdateChoiceBox(m_aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_dialogBox.EndbleChoiceBox(false);
            if (m_aboutToUseChoice == true)
            {
                //はいを選んだ
                OpenPartySelection();
            }
            else
            {
                //いいえを選んだ
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            m_dialogBox.EndbleChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    /// <summary>
    /// ポケモンを入れ替える時の処理
    /// </summary>
    private IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (m_playerUnit.Pokemon.HP > 0)
        {
            m_dialogBox.EnableDialogText(true);
            yield return m_dialogBox.TypeDialog($"戻れ!\n{m_playerUnit.Pokemon.Base.Name}！");
            m_playerUnit.GameObjectDestroy();
            yield return new WaitForSeconds(2f);
        }

        m_playerUnit.Setup(newPokemon);
        m_dialogBox.SetMoveNames(newPokemon.Moves);
        yield return m_dialogBox.TypeDialog($"ゆけっ!\n {m_playerUnit.Pokemon.Base.Name}！");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            m_state = BattleState.RunningTurn;
        }
    }

    /// <summary>
    /// トレーナーが次のポケモンを繰り出すとき
    /// </summary>
    private IEnumerator SendNextTrainerPokemon()
    {
        m_state = BattleState.Busy;

        var nextPokemon = m_trainerParty.GetHealthyPokemon();
        m_enemyUnit.Setup(nextPokemon);
        yield return m_dialogBox.TypeDialog($"{m_Trainer.Name}は\n {nextPokemon.Base.Name}をくりだした！");

        m_state = BattleState.RunningTurn;
    }

    /// <summary>
    /// 逃げれるか判断
    /// </summary>
    private IEnumerator TryToEscape()
    {
        m_state = BattleState.Busy;

        m_dialogBox.EnableActionSelector(false);
        m_dialogBox.EnableDialogText(true);

        if (isTrainerBattle)
        {
            yield return m_dialogBox.TypeDialog($"トレーナと戦ってる時は逃げられない！");
            m_state = BattleState.RunningTurn;
            yield break;
        }

        ++m_escapeAttempts;

        int playerSpeed = m_playerUnit.Pokemon.Speed;
        int enemySpeed = m_enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return m_dialogBox.TypeDialog($"上手く逃げれた！");
            BattleOver(true);
            m_playerUnit.GameObjectDestroy();
            m_enemyUnit.GameObjectDestroy();
        }
        else
        {
            //参考計算方法https://bulbapedia.bulbagarden.net/wiki/Escape#Generation_III_and_IV
            float f = (playerSpeed * 128) / enemySpeed + 30 * m_escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return m_dialogBox.TypeDialog($"上手く逃げれた！");
                BattleOver(true);
                m_playerUnit.GameObjectDestroy();
                m_enemyUnit.GameObjectDestroy();
            }
            else
            {
                yield return m_dialogBox.TypeDialog($"逃げれなかった");
                m_state = BattleState.RunningTurn;
            }
        }
    }
}
