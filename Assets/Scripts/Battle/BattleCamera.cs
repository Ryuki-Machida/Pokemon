using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCamera : MonoBehaviour
{
    [SerializeField] Camera m_camera;
    [SerializeField] Camera m_camera1;


    /// <summary>
    /// 敵を写すカメラ
    /// </summary>
    public void EnemyCamera()
    {
        m_camera.gameObject.SetActive(true);
        m_camera1.gameObject.SetActive(false);
        m_camera.transform.position = new Vector3(-5, 3, 29);
        m_camera.transform.Rotate(new Vector3(0, 50, 0));
    }

    /// <summary>
    /// プレイヤーを写すカメラ
    /// </summary>
    public void PlayerCamera()
    {
        m_camera.gameObject.SetActive(true);
        m_camera1.gameObject.SetActive(false);
        m_camera.transform.position = new Vector3(5, 3, 29);
        m_camera.transform.Rotate(new Vector3(0, 260, 0));
    }

    /// <summary>
    /// 行動選択中のカメラ
    /// </summary>
    public void MoveingCamera()
    {
        m_camera.gameObject.SetActive(false);
        m_camera1.gameObject.SetActive(true);
    }

    /// <summary>
    /// 戦闘開始のカメラ
    /// </summary>
    public void BattlePlayCamera()
    {
        m_camera.gameObject.SetActive(true);
        m_camera1.gameObject.SetActive(false);
        m_camera.transform.position = new Vector3(0, 3, 20);
        m_camera.transform.Rotate(new Vector3(0, 50, 0));
    }
}
