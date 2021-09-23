using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    /// <summary>アイテムの名前</summary>
    [SerializeField] string m_name;
    /// <summary>アイテムの説明</summary>
    [SerializeField] string m_description;
    /// <summary>アイテムの画像</summary>
    [SerializeField] Sprite m_icon;

    public string Name
    {
        get { return m_name; }
    }

    public string Description
    {
        get { return m_description; }
    }

    public Sprite Icon
    {
        get { return m_icon; }
    }

    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }
}
