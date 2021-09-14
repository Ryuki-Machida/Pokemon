using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject m_dialogBox;
    [SerializeField] Text m_dialogText;
    [SerializeField] int m_lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    Dialog dialog;
    int m_currentLine = 0;
    bool isTyping;

    public static DialogManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        this.dialog = dialog;
        m_dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    /// <summary>
    /// 次のテキストを流すか消すか
    /// </summary>
    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping)
        {
            ++m_currentLine;
            if (m_currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[m_currentLine]));
            }
            else
            {
                m_currentLine = 0;
                m_dialogBox.SetActive(false);
                OnCloseDialog?.Invoke();
            }
        }
    }

    /// <summary>
    /// テキストを流す
    /// </summary>
    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        m_dialogText.text = " ";
        foreach (var letter in line.ToCharArray())
        {
            m_dialogText.text += letter;
            yield return new WaitForSeconds(1f / m_lettersPerSecond);
        }
        isTyping = false;
    }
}
