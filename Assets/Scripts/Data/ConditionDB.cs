using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常のデータ管理
/// </summary>
public class ConditionDB : MonoBehaviour
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionID;
        }
    }

    /// <summary>
    /// 状態異常になったらDictionaryに追加する
    /// </summary>
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.どく,
            new Condition()
            {
                Name = "どく",
                StartMessge = "は　どくをうけた",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　どくのダメージをうけている");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                }
            }
        },
        {
            ConditionID.やけど,
            new Condition()
            {
                Name = "やけど",
                StartMessge = "は　やけどをおった",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　やけどのダメージをうけている");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                }
            }
        },
        {
            ConditionID.まひ,
            new Condition()
            {
                Name = "まひ",
                StartMessge = "は　まひをうけた",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //20％の確率で動かない
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　しびれてうごけない");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.こおり,
            new Condition()
            {
                Name = "こおり",
                StartMessge = "は　こおってしまった",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //20％の確率で動けるようになる
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　こおりがとけた");
                        return true;
                    }
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　こおってうごけない");
                    return false;
                }
            }
        },
        {
            ConditionID.ねむり,
            new Condition()
            {
                Name = "ねむり",
                StartMessge = "は　ねむってしまった",
                OnStart = (Pokemon pokemon) =>
                {
                    //1〜3ターン寝る
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"ねむりターン数　{pokemon.StatusTime}");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　めがさめた！");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　ねむっている");
                    return false;
                }
            }
        },


        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "こんらん",
                StartMessge = "は　こんらんしてしまった",
                OnStart = (Pokemon pokemon) =>
                {
                    //1〜4ターンこんらんする
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"こんらんターン数 {pokemon.VolatileStatusTime}");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolayileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　こんらんがなおった！");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;

                    //50％で攻撃
                    if (Random.Range(1, 3) == 1)
                    {
                        return true;
                    }

                    //こんらんによってダメージを受ける
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　こんらんしている");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}は　自分を攻撃した");
                    return false;
                }
            }
        }
    };
}

/// <summary>
/// 状態の種類
/// </summary>
public enum ConditionID
{
    none, どく, やけど, ねむり, まひ, こおり,
    confusion
}
