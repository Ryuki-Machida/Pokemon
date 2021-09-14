using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleManager : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] GameObject m_PlayerHud;
    [SerializeField] GameObject m_EnemyHud;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int m_currentAction;
    int m_currentMove;
    int m_currentMenber;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// バトルが始まった時の処理
    /// </summary>
    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        m_EnemyHud.SetActive(true);
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"あっ！　野生の{enemyUnit.Pokemon.Base.Name}　が飛び出して来た！");
        yield return dialogBox.TypeDialog($"ゆけっ！　{playerUnit.Pokemon.Base.Name}！");
        yield return new WaitForSeconds(1f);

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    /// <summary>
    /// 攻撃を選択してるとき
    /// </summary>
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableDialogText(false);
    }

    /// <summary>
    /// ポケモンを選択してるとき
    /// </summary>
    void OpenPartySelection()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
        dialogBox.EnableActionSelector(false);
    }

    /// <summary>
    /// わざを選択しているとき
    /// </summary>
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

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
            else
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //最初のターン
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit, secondUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }

            if (secondPokemon.HP > 0)
            {
                //2番目のターン
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit, firstUnit);
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[m_currentMenber];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //敵のターン
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit, playerUnit);
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
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        dialogBox.EnableDialogText(true);
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
                yield return RunMoneEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
                yield return new WaitForSeconds(0.5f);
                dialogBox.EnableDialogText(false);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();

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
                        yield return RunMoneEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            //受けた方のHPが0になったら
            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}は たおれた！");
                targetUnit.PokemonDie();
                yield return new WaitForSeconds(1f);

                targetUnit.GameObjectDestroy();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit, sourceUnit);
            }
        }
        else
        {
            dialogBox.EnableDialogText(true);
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}には 攻撃が当たらなかった");
            dialogBox.EnableDialogText(false);
        }
    }

    IEnumerator RunMoneEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
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

    IEnumerator RunAfterTurn(BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //やけど、どく状態だった場合、ターン後にダメージを与える
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}は たおれた！");
            //倒れるアニメーション
            sourceUnit.PokemonDie();
            yield return new WaitForSeconds(1f);

            //オブジェクトを消す
            sourceUnit.GameObjectDestroy();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit, sourceUnit);
        }
    }

    /// <summary>
    /// 命中率
    /// </summary>
    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
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
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var massge = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(massge);
        }
    }

    /// <summary>
    /// ポケモンがパーティーに残っているか
    /// </summary>
    void CheckForBattleOver(BattleUnit faintedUnit, BattleUnit destroyUnit)
    {
        if (faintedUnit.IsPlayerUnit) //ポケモンが残ってたら
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
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
            destroyUnit.GameObjectDestroy(); //バトルが終わったら残っている方の子オブジェクトを消す
            BattleOver(true);
        }
    }

    /// <summary>
    /// 与えたわざのタイプ相性によってtextの文字を変える
    /// </summary>
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
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
    }

    /// <summary>
    /// 行動選択中の処理
    /// </summary>
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (m_currentAction < 3)
            {
                ++m_currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
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
                prevState = state;
                OpenPartySelection();
            }
            else if (m_currentAction == 2)
            {
                //バック
            }
            else if (m_currentAction == 3)
            {
                //にげる
            }
        }
    }

    /// <summary>
    /// たたかうを選択した場合の処理
    /// </summary>
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (m_currentMove < 3)
            {
                ++m_currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (m_currentMove > 0)
            {
                --m_currentMove;
            }
        }

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
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++m_currentMenber;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --m_currentMenber;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_currentMenber += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_currentMenber -= 2;
        }

        m_currentMenber = Mathf.Clamp(m_currentMenber, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelected(m_currentMenber);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var selectedMember = playerParty.Pokemons[m_currentMenber];
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

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    /// <summary>
    /// ポケモンを入れ替える時の処理
    /// </summary>
    IEnumerator SwitchPokemon(Pokemon newPokemon)
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

        state = BattleState.RunningTurn;
    }
}
