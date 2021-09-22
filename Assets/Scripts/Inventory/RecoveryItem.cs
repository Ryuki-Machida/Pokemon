using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムの設定
/// </summary>
[CreateAssetMenu(menuName = "Item/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int m_hpAmount;
    [SerializeField] bool m_restoreMaxHP;

    [Header("PP")]
    [SerializeField] int m_ppamount;
    [SerializeField] bool m_restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID m_status;
    [SerializeField] bool m_restoreAllStatus;

    [Header("Revive")]
    [SerializeField] bool m_revive;
    [SerializeField] bool m_maxRevive;
}
