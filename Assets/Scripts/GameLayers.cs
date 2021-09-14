using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レイヤーを管理する
/// </summary>
public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask m_solidLayer;
    [SerializeField] LayerMask m_grassLayer;
    [SerializeField] LayerMask m_npcLayer;
    [SerializeField] LayerMask m_playerLayer;

    public static GameLayers gl { get; set; }

    private void Awake()
    {
        gl = this;
    }

    public LayerMask SolidLayer
    {
        get { return m_solidLayer; }
    }

    public LayerMask GrassLayer
    {
        get { return m_grassLayer; }
    }

    public LayerMask NpcLayer
    {
        get { return m_npcLayer; }
    }

    public LayerMask PlayerLayer
    {
        get { return m_playerLayer; }
    }
}
