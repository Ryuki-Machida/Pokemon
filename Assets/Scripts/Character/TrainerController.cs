using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] string m_name;

    [SerializeField] Dialog m_dialog;
    [SerializeField] GameObject m_exclamation;
    [SerializeField] GameObject m_fov;

    Character m_character;

    private void Awake()
    {
        m_character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(m_character.Animator.DefaultDirection);
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
