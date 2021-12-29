using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string m_name;

    const float m_offsetY = 0.3f;

    public event Action m_OnEncountered;
    public event Action<Collider2D> m_OnTrainersView;

    private Vector2 m_input;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!m_character.IsMoving)
        {
            m_input.x = Input.GetAxisRaw("Horizontal");
            m_input.y = Input.GetAxisRaw("Vertical");

            //斜め移動が出来なくなる
            if (m_input.x != 0)
            {
                m_input.y = 0;
            }

            if (m_input != Vector2.zero)
            {
                StartCoroutine(m_character.Move(m_input, OnMoveOver));
            }
        }

        m_character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(m_character.Animator.MoveX, m_character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.gl.NpcLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    void OnMoveOver()
    {
        CheckForEncounters();
        CheckTrainersView();
    }


    void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, m_offsetY), 0.2f, GameLayers.gl.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 50) <= 10) // 1マスごとにランダムで取得
            {
                m_character.Animator.IsMoving = false;
                m_OnEncountered(); //切り替え
            }
        }
    }

    void CheckTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, m_offsetY), 0.2f, GameLayers.gl.FovLayer);
        if (collider != null)
        {
            m_character.Animator.IsMoving = false;
            m_OnTrainersView?.Invoke(collider);
        }
    }

    public string Name
    {
        get { return m_name; }
    }
}
