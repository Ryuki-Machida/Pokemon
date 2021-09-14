using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog : MonoBehaviour
{
    /// <summary>会話の数<summary>
    [SerializeField] List<string> m_lines;

    public List<string> Lines
    {
        get { return m_lines; }
    }
}
