using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] string m_name;

    [SerializeField] Dialog m_dialog;
    [SerializeField] Dialog m_dialogAfterBattle;
    [SerializeField] GameObject m_exclamation;
    [SerializeField] GameObject m_fov;

    bool m_battlelost = false;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(m_character.Animator.DefaultDirection);
    }

    void Interactable.Interact(Transform initiator)
    {
        m_character.LookTowards(initiator.position);

        if (!m_battlelost)
        {
            //会話してからバトル
            StartCoroutine(DialogManager.Instance.ShowDialog(m_dialog, () =>
            {
                GamaManager.Instance.StartTrainerBattle(this);
            }));
        }
        else
        {
            //会話のみ
            StartCoroutine(DialogManager.Instance.ShowDialog(m_dialogAfterBattle));
        }
    }

    /// <summary>
    /// プレイヤーが通ったら
    /// </summary>
    public IEnumerator TrainerBattle(PlayerController player)
    {
        //気づいた
        m_exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        m_exclamation.SetActive(false);

        //プレイヤーに向かって移動
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return m_character.Move(moveVec);

        //dialogを表示
        StartCoroutine(DialogManager.Instance.ShowDialog(m_dialog, () =>
        {
            GamaManager.Instance.StartTrainerBattle(this);
        }));
    }

    /// <summary>
    /// トレーナ戦終了
    /// </summary>
    public void BattleLost()
    {
        m_battlelost = true;
        m_fov.gameObject.SetActive(false);
    }

    /// <summary>
    /// Npcの向きによってFovの向きも変える
    /// </summary>
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0;
        if (dir == FacingDirection.Right)
        {
            angle = 90;
        }
        else if(dir == FacingDirection.Up)
        {
            angle = 180;
        }
        else if(dir == FacingDirection.Left)
        {
            angle = 270;
        }
        m_fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public string Name
    {
        get { return m_name; }
    }
}
