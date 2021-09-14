using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    /// <summary>会話文の数<summary>
    [SerializeField] List<string> m_lines;

    public List<string> Lines
    {
        get { return m_lines; }
    }
}
