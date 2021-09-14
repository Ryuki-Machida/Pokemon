using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技の管理
/// </summary>
[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string m_name;

    [TextArea]
    [SerializeField] string m_description;

    [SerializeField] PokemonType m_type;
    [SerializeField] Color m_TypeColor;
    [SerializeField] int m_power;
    [SerializeField] int m_accuracy;
    [SerializeField] bool m_alwaysHits;
    [SerializeField] int m_pp;
    [SerializeField] int m_priority;
    [SerializeField] MoveCategory m_category;
    [SerializeField] MoveEffects m_effects;
    [SerializeField] List<SecondaryEffects> m_secondaries;
    [SerializeField] MoveTarget m_target;

    public string Name
    {
        get { return m_name; }
    }

    public string Description
    {
        get { return m_description; }
    }

    public PokemonType Type
    {
        get { return m_type; }
    }

    public Color TypeColor
    {
        get { return m_TypeColor; }
    }

    public int Power
    {
        get { return m_power; }
    }

    public int Accuracy
    {
        get { return m_accuracy; }
    }

    public bool AlwaysHits
    {
        get { return m_alwaysHits; }
    }

    public int PP
    {
        get { return m_pp; }
    }

    public int Priority
    {
        get { return m_priority; }
    }

    public MoveCategory Category
    {
        get { return m_category; }
    }

    public MoveEffects Effects
    {
        get { return m_effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return m_secondaries; }
    }

    public MoveTarget Target
    {
        get { return m_target; }
    }
}

/// <summary>
/// ステータスの+-の設定
/// </summary>
[System.Serializable]
public class MoveEffects
{
    /// <summary>何個のステータスに効果を与えるか設定</summary>
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }
}

/// <summary>
/// 確率で状態異常にする設定
/// </summary>
[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    /// <summary>確率</summary>
    [SerializeField] int chance;
    /// <summary>効果を与える方</summary>
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    /// <summary>どのステータスに効果を与えるか</summary>
    public Stat stat;
    /// <summary>+-いくつにするか</summary>
    public int boost;
}

/// <summary>
/// 技の種類
/// </summary>
public enum MoveCategory
{
    physical, //物理技
    Special,　//特殊技
    Status　　//状態技
}

/// <summary>
/// 効果を与える方
/// </summary>
public enum MoveTarget
{
    Foe, //敵
    Self //自身
}
