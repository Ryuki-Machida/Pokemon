using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float m_moveSpeed;

    public LayerMask m_solidLayer;
    public LayerMask m_grassLayer;

    private bool isMoving;
    private Vector2 input;

    Animator m_anim;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
    }

    void Update()
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

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
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

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.3f, m_solidLayer) != null)
        {
            return false;
        }
        return true;
    }

    void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, m_grassLayer) != null)
        {
            if (Random.Range(1, 50) <= 10) // 1マスごとにランダムで取得
            {
                Debug.Log("a");
            }
        }
    }
}
