using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog m_dialog;

    public void Interact()
    {
        StartCoroutine(DialogManager.Instance.ShowDialog(m_dialog));
    }
}
