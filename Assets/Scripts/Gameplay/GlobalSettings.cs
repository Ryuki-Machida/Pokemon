using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Textの色を共有するクラス
/// </summary>
public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color m_highlightedColor;

    public Color HighlightedColor
    {
        get { return m_highlightedColor; }
    }

    public static GlobalSettings i { get; set; }

    private void Awake()
    {
        i = this;
    }
}
