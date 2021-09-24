using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    [SerializeField] AudioSource m_source;
    [SerializeField] AudioClip m_clip1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_source.clip = m_clip1;
            m_source.Play();
            SceneManager.LoadScene("MainScene");
        }
    }
}
