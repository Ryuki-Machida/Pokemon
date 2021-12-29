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

    public event Action m_OnShowDialog;
    public event Action m_OnCloseDialog;

    Dialog m_dialog;
    Action m_onDialogFinish;

    int m_currentLine = 0;
    bool m_isTyping;

    public bool IsShowing { get; private set; }

    public static DialogManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// アイテム使用したときに表示する
    /// </summary>
    public IEnumerator ShowDialogText(string text, bool waitForInput = true)
    {
        IsShowing = true;
        m_dialogBox.SetActive(true);

        yield return TypeDialog(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        m_dialogBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinish = null)
    {
        yield return new WaitForEndOfFrame();

        m_OnShowDialog?.Invoke();

        IsShowing = true;
        this.m_dialog = dialog;
        m_onDialogFinish = onFinish;

        m_dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    /// <summary>
    /// 次のテキストを流すか消すか
    /// </summary>
    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !m_isTyping)
        {
            ++m_currentLine;
            if (m_currentLine < m_dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(m_dialog.Lines[m_currentLine]));
            }
            else
            {
                m_currentLine = 0;
                IsShowing = false;
                m_dialogBox.SetActive(false);
                m_onDialogFinish?.Invoke();
                m_OnCloseDialog?.Invoke();
            }
        }
    }

    /// <summary>
    /// テキストを流す
    /// </summary>
    public IEnumerator TypeDialog(string line)
    {
        m_isTyping = true;
        m_dialogText.text = " ";
        foreach (var letter in line.ToCharArray())
        {
            m_dialogText.text += letter;
            yield return new WaitForSeconds(1f / m_lettersPerSecond);
        }
        m_isTyping = false;
    }
}
