using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource m_bgm;
    [SerializeField] AudioSource m_source;

    [SerializeField] AudioClip m_bgm1;
    [SerializeField] AudioClip m_bgm2;
    [SerializeField] AudioClip m_bgm3;


    [SerializeField] AudioClip m_clip1;
    [SerializeField] AudioClip m_clip2;
    [SerializeField] AudioClip m_clip3;
    [SerializeField] AudioClip m_clip4;

    public void Map()
    {
        m_bgm.clip = m_bgm1;
        m_bgm.Play();
    }

    public void Battle()
    {
        m_bgm.clip = m_bgm2;
        m_bgm.Play();
    }

    public void TrainerBattle()
    {
        m_bgm.clip = m_bgm3;
        m_bgm.Play();
    }

    public void Attack()
    {
        m_source.clip = m_clip1;
        m_source.Play();
    }

    public void Hit()
    {
        m_source.clip = m_clip2;
        m_source.Play();
    }

    public void LevelUp()
    {
        m_source.clip = m_clip3;
        m_source.Play();
    }

    public void ItemUse()
    {
        m_source.clip = m_clip4;
        m_source.Play();
    }
}