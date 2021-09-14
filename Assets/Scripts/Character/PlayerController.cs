using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float m_moveSpeed;

    public LayerMask m_solidLayer;
    public LayerMask m_grassLayer;

    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;

    Animator m_anim;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
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
                m_anim.SetFloat("MoveX", input.x);
                m_anim.SetFloat("MoveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                StartCoroutine(Move(targetPos));
            }
        }
        m_anim.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, m_moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters();
    }

    void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, m_grassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 50) <= 10) // 1マスごとにランダムで取得
            {
                m_anim.SetBool("isMoving", false);
                OnEncountered(); //切り替え
            }
        }
    }
}
