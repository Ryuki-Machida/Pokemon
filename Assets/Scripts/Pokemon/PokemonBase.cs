using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ポケモンの管理
/// </summary>
[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string m_name;

    [TextArea]
    [SerializeField] string m_description;

    [SerializeField] GameObject m_PokemonObject;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //ステータス
    [SerializeField] int m_maxHp;
    [SerializeField] int m_attack;
    [SerializeField] int m_defense;
    [SerializeField] int m_spAttack;
    [SerializeField] int m_spDefense;
    [SerializeField] int m_speed;

    /// <summary>貰える経験値<summary>
    [SerializeField] int m_expYield;
    [SerializeField] GrowthRate m_growthRate;

    //覚える技の数
    [SerializeField] List<LearnableMove> m_learnableMoves;

    public static int MaxNumOfMoves { get; set; } = 4; //4つまでしか使えないから

    /// <summary>
    /// レベルのExpを取得
    /// </summary>
    public int GetExpForLevel(int level)
    {
        if (m_growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (m_growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }

    public string Name
    {
        get { return m_name; }
    }

    public string Description
    {
        get { return m_description; }
    }

    public GameObject PokemonObject
    {
        get { return m_PokemonObject; }
    }

    public PokemonType Type1
    {
        get { return type1; }
    }

    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return m_maxHp; }
    }

    public int Attack
    {
        get { return m_attack; }
    }

    public int Defense
    {
        get { return default; }
    }

    public int SpAttack
    {
        get { return m_spAttack; }
    }

    public int SpDefense
    {
        get { return m_spDefense; }
    }

    public int Speed
    {
        get { return m_speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return m_learnableMoves; }
    }

    public int ExpYield
    {
        get { return m_expYield; }
    }

    public GrowthRate GrowthRate
    {
        get { return m_growthRate; }
    }
}

/// <summary>
/// なんレべでなんの技を覚えるか設定
/// </summary>
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

/// <summary>
/// タイプの種類
/// </summary>
public enum PokemonType
{
    None,
    Noraml,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Doragon,
}

/// <summary>
/// 成長速度
/// </summary>
public enum GrowthRate
{
    Fast, MediumFast //トレーナ or 野生か
}

/// <summary>
/// ステータスの種類
/// </summary>
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    //2つはmoveAccuracyをブーストするために使用する
    Accuracy,
    Evasion
}


/// <summary>
/// タイプ相性
/// </summary>
public class TypeChart
{
    static float[][] chart =
    {
        //                           NOR  FIR   WAT   ELE   GRA   ICE   FIG  POI
        /* Normal */   new float[] { 1f,  1f,   1f,   1f,   1f,   1f,   1f,  1f },
        /* Fire */     new float[] { 1f,  0.5f, 0.5f, 1f,   2f,   2f,   1f,  1f },
        /* Water */    new float[] { 1f,  2f,   0.5f, 2f,   0.5f, 1f,   1f,  1f },
        /* Electric */ new float[] { 1f,  1f,   2f,   0.5f, 0.5f, 2f,   1f,  1f },
        /* Grass */    new float[] { 1f,  0.5f, 2f,   2f,   0.5f, 1f,   1f,  0.5f },
        /* Ice */      new float[] { 1f,  0.5f, 0.5f, 1f,   2f,   0.5f, 1f,  1f },
        /* Fighting */ new float[] { 2f,  1f,   1f,   1f,   2f,   1f,   1f,  0.5f },
        /* Poison */   new float[] { 1f,  1f,   1f,   1f,   2f,   1f,   1f,  1f },
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
