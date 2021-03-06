using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase m_base;
    [SerializeField] int m_level;

    public PokemonBase Base
    {
        get { return m_base; }
    }

    public int Level
    {
        get { return m_level; }
    }

    public int Exp { get; set; }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }

    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }

    public int StatusTime { get; set; }

    public Condition VolatileStatus { get; set; }

    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public event System.Action OnStatusChamged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        //動きを生成する
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    /// <summary>
    /// ステータスを計算する
    /// </summary>
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.攻撃, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.防御, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.特攻, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.特防, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.素早さ, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
    }

    /// <summary>
    /// 変わったステータスをリセットする
    /// </summary>
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.攻撃, 0},
            {Stat.防御, 0},
            {Stat.特攻, 0},
            {Stat.特防, 0},
            {Stat.素早さ, 0},
            {Stat.命中率, 0},
            {Stat.回避率, 0},
        };
    }

    /// <summary>
    /// ステータスがいくつ上がって（下がって）いるかによって値を返す
    /// </summary>
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //何段階になっているか
        int boost = StatBoosts[stat];
        //                              0    1    2    3    4    5    6
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    /// <summary>
    /// ステータスがいくつ上がって（下がって）いるか
    /// </summary>
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            //Mathf.Clampを使うと制限をかけれる
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}の {stat}があがった!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}の {stat}がさがった");
            }

            Debug.Log($"{stat}が{StatBoosts[stat]}された");
        }
    }

    /// <summary>
    /// レベルが上がったか確認
    /// </summary>
    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(m_level + 1))
        {
            ++m_level;
            ++HP;
            ++MaxHp;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == m_level).FirstOrDefault();
    }

    /// <summary>
    /// ワザを覚える
    /// </summary>
    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack
    {
        get { return GetStat(Stat.攻撃); }
    }

    public int Defense
    {
        get { return GetStat(Stat.防御); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.特攻); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.特防); }
    }

    public int Speed
    {
        get { return GetStat(Stat.素早さ); }
    }

    public int MaxHp { get; private set; }

    /// <summary>
    /// ダメージの計算
    /// </summary>
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        //参考計算方法https://bulbapedia.bulbagarden.net/wiki/Damage

        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical; //乱数を含めてる
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);
        return damageDetails;
    }

    /// <summary>
    /// HP回復
    /// </summary>
    /// <param name="amount"></param>
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    /// <summary>
    /// 状態のダメージを受ける計算
    /// </summary>
    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }

        Status = ConditionDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessge}");
        OnStatusChamged?.Invoke();
    }

    /// <summary>
    /// ステータスを治す
    /// </summary>
    public void CureStatus()
    {
        Status = null;
        OnStatusChamged?.Invoke();
    }

    /// <summary>
    /// 状態変化のセット
    /// </summary>
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }

        VolatileStatus = ConditionDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessge}");
    }

    /// <summary>
    /// 状態変化を治す
    /// </summary>
    public void CureVolayileStatus()
    {
        VolatileStatus = null;
    }

    /// <summary>
    /// 敵の攻撃を決める
    /// </summary>
    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    /// <summary>
    /// 行動できるか
    /// </summary>
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
