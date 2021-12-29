using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog m_dialog;

    /// <summary>方向いくつ進むか<summary>
    [SerializeField] List<Vector2> m_movePattern;
    /// <summary>止まる時間<summary>
    [SerializeField] float m_timePattern;

    NpcState m_state;
    float m_idleTimer = 0f;
    int m_currentPattern = 0;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        if (m_state == NpcState.Idle)
        {
            m_state = NpcState.Dialog;
            m_character.LookTowards(initiator.position); //対象のオブジェクトの方を向く

            StartCoroutine(DialogManager.Instance.ShowDialog(m_dialog, () =>
            {
                m_idleTimer = 0;
                m_state = NpcState.Idle;
            }));
        }
    }

    private void Update()
    {
        if (m_state == NpcState.Idle)
        {
            m_idleTimer += Time.deltaTime;
            if (m_idleTimer > m_timePattern)
            {
                m_idleTimer = 0;
                if (m_movePattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        m_character.HandleUpdate();
    }

    /// <summary>
    /// 移動
    /// </summary>
    IEnumerator Walk()
    {
        m_state = NpcState.Walking;

        var oldPos = transform.position;

        yield return m_character.Move(m_movePattern[m_currentPattern]);

        if (transform.position != oldPos) //プレイヤーにぶつかってる間は止める
        {
            m_currentPattern = (m_currentPattern + 1) % m_movePattern.Count;
        }

        m_state = NpcState.Idle;
    }
}

public enum NpcState { Idle, Walking, Dialog }
