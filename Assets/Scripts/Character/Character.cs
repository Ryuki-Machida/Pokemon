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

        if (!IsPathClear(targetPos))
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
    bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dif = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dif, new Vector2(0.2f, 0.2f), 0f, dif, diff.magnitude - 1, GameLayers.gl.SolidLayer | GameLayers.gl.NpcLayer | GameLayers.gl.PlayerLayer) == true)
        {
            return false;
        }
        return true;
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

    /// <summary>
    /// 向きを変える
    /// </summary>
    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            m_charAnim.MoveX = Mathf.Clamp(xdiff, -1, 1);
            m_charAnim.MoveY = Mathf.Clamp(ydiff, -1, 1);
        }
        else
        {
            Debug.LogError("キャラクターに斜めに見るように頼むことはできません");
        }
    }

    public CharacterAnimator Animator
    {
        get { return m_charAnim; }
    }
}
