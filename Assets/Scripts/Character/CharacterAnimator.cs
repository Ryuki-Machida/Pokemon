using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> m_walkDown;
    [SerializeField] List<Sprite> m_walkUp;
    [SerializeField] List<Sprite> m_walkRight;
    [SerializeField] List<Sprite> m_walkLeft;

    [SerializeField] FacingDirection m_facingDirection = default;

    //パラメータ
    public float MoveX { get; set; }

    public float MoveY { get; set; }

    public bool IsMoving { get; set; }

    //ステータス
    SpriteAnimator m_downSpanim;
    SpriteAnimator m_upSpanim;
    SpriteAnimator m_rightSpanim;
    SpriteAnimator m_leftSpanim;

    SpriteAnimator m_currentAnim;

    SpriteRenderer m_spriteRenderer;
    bool m_previouslyMoving;

    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_downSpanim = new SpriteAnimator(m_walkDown, m_spriteRenderer);
        m_upSpanim = new SpriteAnimator(m_walkUp, m_spriteRenderer);
        m_rightSpanim = new SpriteAnimator(m_walkRight, m_spriteRenderer);
        m_leftSpanim = new SpriteAnimator(m_walkLeft, m_spriteRenderer);
        SetFacingDirection(m_facingDirection);

        m_currentAnim = m_downSpanim;
    }

    void Update()
    {
        var prevAnim = m_currentAnim;

        if (MoveX == 1)
        {
            m_currentAnim = m_rightSpanim;
        }
        else if (MoveX == -1)
        {
            m_currentAnim = m_leftSpanim;
        }
        else if (MoveY == 1)
        {
            m_currentAnim = m_upSpanim;
        }
        else if (MoveY == -1)
        {
            m_currentAnim = m_downSpanim;
        }

        if (m_currentAnim != prevAnim || IsMoving != m_previouslyMoving)
        {
            m_currentAnim.Start();
        }

        if (IsMoving)
        {
            m_currentAnim.HandleUpdate();
        }
        else
        {
            m_spriteRenderer.sprite = m_currentAnim.Frames[0];
        }

        m_previouslyMoving = IsMoving;
    }

    /// <summary>
    /// Npcの最初の向きを変える
    /// </summary>
    public void SetFacingDirection(FacingDirection dir)
    {
        if (dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
        else if (dir == FacingDirection.Left)
        {
            MoveX = -1;
        }
        else if (dir == FacingDirection.Down)
        {
            MoveY = -1;
        }
        else if (dir == FacingDirection.Up)
        {
            MoveY = 1;
        }
    }

    public FacingDirection DefaultDirection
    {
        get { return m_facingDirection; }
    }
}

public enum FacingDirection { Up, Down, Right, Left }
