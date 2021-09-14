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

    NpcState state;
    float m_idleTimer = 0f;
    int m_currentPattern = 0;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    public void Interact()
    {
        if (state == NpcState.Idle)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(m_dialog));
        }
    }

    private void Update()
    {
        if (DialogManager.Instance.IsShowing)
        {
            return;
        }

        if (state == NpcState.Idle)
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
        state = NpcState.Walking;

        yield return m_character.Move(m_movePattern[m_currentPattern]);
        m_currentPattern = (m_currentPattern + 1) % m_movePattern.Count;

        state = NpcState.Idle;
    }
}

public enum NpcState { Idle, Walking }
