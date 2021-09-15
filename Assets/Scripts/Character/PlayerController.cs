using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public event Action OnEncountered;
    public event Action<Collider2D> OnTrainersView;

    private Vector2 input;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!m_character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //斜め移動が出来なくなる
            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                StartCoroutine(m_character.Move(input, OnMoveOver));
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
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.gl.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 50) <= 10) // 1マスごとにランダムで取得
            {
                m_character.Animator.IsMoving = false;
                OnEncountered(); //切り替え
            }
        }
    }

    void CheckTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.gl.FovLayer);
        if (collider != null)
        {
            m_character.Animator.IsMoving = false;
            OnTrainersView?.Invoke(collider);
        }
    }
}
