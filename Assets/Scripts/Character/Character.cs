using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{
    public float m_moveSpeed;
    CharacterAnimator m_charAnim;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        m_charAnim = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        m_charAnim.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        m_charAnim.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, m_moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        m_charAnim.IsMoving = IsMoving;
    }

    /// <summary>
    /// 貫通出来ないようにしている
    /// </summary>
    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.3f, GameLayers.gl.SolidLayer | GameLayers.gl.NpcLayer) != null)
        {
            return false;
        }
        return true;
    }

    public CharacterAnimator Animator
    {
        get { return m_charAnim; }
    }
}
