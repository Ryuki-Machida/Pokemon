using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer m_spriteRenderer;
    List<Sprite> m_frames;
    float m_frameRate;

    int m_currntFrame;
    float m_timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate = 0.16f)
    {
        this.m_frames = frames;
        this.m_spriteRenderer = spriteRenderer;
        this.m_frameRate = frameRate;
    }

    public void Start()
    {
        m_currntFrame = 0;
        m_timer = 0f;
        m_spriteRenderer.sprite = m_frames[0];
    }

    public void HandleUpdate()
    {
        m_timer += Time.deltaTime;
        if (m_timer > m_frameRate)
        {
            m_currntFrame = (m_currntFrame + 1) % m_frames.Count;
            m_spriteRenderer.sprite = m_frames[m_currntFrame];
            m_timer -= m_frameRate;
        }
    }

    public List<Sprite> Frames
    {
        get { return m_frames; }
    }
}
