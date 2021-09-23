using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget , BattleOver }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleManager : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] BattleCamera battleCamera;

    [SerializeField] GameObject m_PlayerHud;
    [SerializeField] GameObject m_EnemyHud;
    [SerializeField] GameObject m_player;
    [SerializeField] GameObject m_trainer;

    public event Action<bool> OnBattleOver;

    BattleState state;
    
    int m_currentAction;
    int m_currentMove;
    bool m_aboutToUseChoice;

    PokemonParty m_playerParty;
    PokemonParty m_trainerParty;
    Pokemon m_wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int m_escapeAttempts;
    MoveBase m_moveToLearn;

    /// <summary>
    /// バトル
    /// </summary>
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.m_playerParty = playerParty;
        this.m_wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
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
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// バトルが始まった時の処理
    /// </summary>
    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)  //野生のポケモンバトル
        {
            m_player.gameObject.SetActive(true);

            playerUnit.Setup(m_playerParty.GetHealthyPokemon());
            enemyUnit.Setup(m_wildPokemon);

            m_EnemyHud.SetActive(true);
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            battleCamera.EnemyCamera();
            yield return dialogBox.TypeDialog($"あっ！　野生の{enemyUnit.Pokemon.Base.Name}　が飛び出して来た！");
            yield return new WaitForSeconds(0.2f);
            battleCamera.PlayerCamera();
            yield return dialogBox.TypeDialog($"ゆけっ！　{playerUnit.Pokemon.Base.Name}！");
            yield return new WaitForSeconds(1f);
        }
        else  //トレーナーバトル
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            m_player.gameObject.SetActive(true);
            m_trainer.gameObject.SetActive(true);

            battleCamera.EnemyCamera();
            yield return dialogBox.TypeDialog($"{trainer.Name}が 勝負を仕掛けてきた！");

            //トレーナーの最初のポケモン
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = m_trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name}が {enemyPokemon.Base.Name}を繰り出してきた！");

            //プレイヤーの最初のポケモン
            battleCamera.PlayerCamera();
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = m_playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"行け！ {playerPokemon.Base.Name}！");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return new WaitForSeconds(1f);
        }

        m_escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        m_playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    /// <summary>
    /// 行動を選択してるとき
    /// </summary>
    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableDialogText(false);
        battleCamera.MoveingCamera();
    }

    /// <summary>
    /// ポケモンを選択してるとき
    /// </summary>
    private void OpenPartySelection()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
        dialogBox.EnableActionSelector(false);
    }

    private void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    /// <summary>
    /// わざを選択しているとき
    /// </summary>
    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// トレーナが次のポケモンを出す前に
    /// </summary>
    private IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name}は　{newPokemon.Base.Name}を出そうとしている。　交換しますか？");

        state = BattleState.AboutToUse;
        dialogBox.EndbleChoiceBox(true);
    }

    /// <summary>
    /// わざを入れ替える
    /// </summary>
    private IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"忘れさせたいワザを選んでください");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        m_moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    private IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        battleCamera.BattlePlayCamera();

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[m_currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            //先制攻撃
            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //誰が最初か確認
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //最初のターン
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            if (state == BattleState.BattleOver)
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
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon) //ポケモン選択
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                //アイテム画面から処理するので、何もせずに敵の移動にスキップする
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run) //逃げる選択
            {
                yield return TryToEscape();
            }

            //敵のターン
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit, playerUnit);
            yield return new WaitForSeconds(0.1f);
            yield return RunAfterTurn(playerUnit, enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    /// <summary>
    /// 戦闘の中身の処理
    /// </summary>
    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        dialogBox.EnableDialogText(true);
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}の　{move.Base.Name}!");

        dialogBox.EnableDialogText(false);
        m_EnemyHud.SetActive(true);

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)) //攻撃が当たったら
        {
            //攻撃アニメーション
            sourceUnit.PokemonAttack();
            yield return new WaitForSeconds(0.8f);
            //受けるアニメーション
            targetUnit.PokemonHit();
            yield return new WaitForSeconds(0.5f);

            if (move.Base.Category == MoveCategory.Status)
            {
                dialogBox.EnableDialogText(true);
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
                yield return new WaitForSeconds(0.5f);
                dialogBox.EnableDialogText(false);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();

                dialogBox.EnableDialogText(true);

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
            dialogBox.EnableDialogText(true);
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}には 攻撃が当たらなかった");
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
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //やけど、どく状態だった場合、ターン後にダメージを与える
        dialogBox.EnableDialogText(true);
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit, targetUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

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
            yield return dialogBox.TypeDialog(massge);
        }
    }

    /// <summary>
    /// ポケモンが気絶した後の処理
    /// </summary>
    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, BattleUnit sourceUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}は たおれた！");
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
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}は　経験値{expGain}貰った");
            yield return playerUnit.Hud.SetExpSmooth();

            //レベルアップ
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}のレベルが　{playerUnit.Pokemon.Level}になった！");

                //新しいワザを覚える
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}は　{newMove.Base.Name}を覚えた！");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}は　{newMove.Base.Name}を覚えようとしている");
                        yield return dialogBox.TypeDialog($"しかし{playerUnit.Pokemon.Base.Name}は　わざを{PokemonBase.MaxNumOfMoves}つ覚えている");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(0.5f);
        }

        CheckForBattleOver(faintedUnit, sourceUnit);
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
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
            yield return dialogBox.TypeDialog("急所に当たった！");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("効果バツグン！");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("効果いまひとつ");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action onItemUsed = () =>
            {
                state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //わざを覚えない
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}は　{m_moveToLearn.Name}を覚えなかった"));
                }
                else
                {
                    //選択したわざを忘れ、新しいわざを覚える
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}は　{selectedMove.Name}を忘れ　{m_moveToLearn.Name}を覚えた！"));

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(m_moveToLearn);
                }

                m_moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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

        dialogBox.UpdateActionSelection(m_currentAction);

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

        m_currentMove = Mathf.Clamp(m_currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(m_currentMove, playerUnit.Pokemon.Moves[m_currentMove]);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var move = playerUnit.Pokemon.Moves[m_currentMove];
            if (move.PP == 0)  //PPがなくなったら選択できない
            {
                return;
            }

            dialogBox.EnableMoveSelector(false);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dialogBox.EnableMoveSelector(false);
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
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("ひんしのためバトルに出せません");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("すでにバトルに出ています");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("手持ちにポケモンが残ってない");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
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

        dialogBox.UpdateChoiceBox(m_aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.EndbleChoiceBox(false);
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
            dialogBox.EndbleChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    /// <summary>
    /// ポケモンを入れ替える時の処理
    /// </summary>
    private IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            dialogBox.EnableDialogText(true);
            yield return dialogBox.TypeDialog($"戻れ！　{playerUnit.Pokemon.Base.Name}！");
            playerUnit.GameObjectDestroy();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"ゆけっ！　{playerUnit.Pokemon.Base.Name}！");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }

    /// <summary>
    /// トレーナーが次のポケモンを繰り出すとき
    /// </summary>
    private IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = m_trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name}が {nextPokemon.Base.Name}を繰り出してきた！");

        state = BattleState.RunningTurn;
    }

    /// <summary>
    /// 逃げれるか判断
    /// </summary>
    private IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(true);

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"トレーナと戦ってる時は逃げられない！");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++m_escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"上手く逃げれた！");
            BattleOver(true);
            playerUnit.GameObjectDestroy();
            enemyUnit.GameObjectDestroy();
        }
        else
        {
            //参考計算方法https://bulbapedia.bulbagarden.net/wiki/Escape#Generation_III_and_IV
            float f = (playerSpeed * 128) / enemySpeed + 30 * m_escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"上手く逃げれた！");
                BattleOver(true);
                playerUnit.GameObjectDestroy();
                enemyUnit.GameObjectDestroy();
            }
            else
            {
                yield return dialogBox.TypeDialog($"逃げれなかった");
                state = BattleState.RunningTurn;
            }
        }
    }
}
