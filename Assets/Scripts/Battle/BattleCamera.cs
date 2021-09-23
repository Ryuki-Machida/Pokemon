using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCamera : MonoBehaviour
{
    [SerializeField] Camera m_camera;
    [SerializeField] Camera m_camera1;
    [SerializeField] Camera m_camera2;
    [SerializeField] Camera m_camera3;


    /// <summary>
    /// 敵を写すカメラ
    /// </summary>
    public void EnemyCamera()
    {
        m_camera.gameObject.SetActive(false);
        m_camera1.gameObject.SetActive(false);
        m_camera2.gameObject.SetActive(true);
        m_camera3.gameObject.SetActive(false);
    }

    /// <summary>
    /// プレイヤーを写すカメラ
    /// </summary>
    public void PlayerCamera()
    {
        m_camera.gameObject.SetActive(false);
        m_camera1.gameObject.SetActive(false);
        m_camera2.gameObject.SetActive(false);
        m_camera3.gameObject.SetActive(true);
    }

    /// <summary>
    /// 行動選択中のカメラ
    /// </summary>
    public void MoveingCamera()
    {
        m_camera.gameObject.SetActive(false);
        m_camera1.gameObject.SetActive(true);
        m_camera2.gameObject.SetActive(false);
        m_camera3.gameObject.SetActive(false);
    }

    /// <summary>
    /// 戦闘開始のカメラ
    /// </summary>
    public void BattlePlayCamera()
    {
        m_camera.gameObject.SetActive(true);
        m_camera1.gameObject.SetActive(false);
        m_camera2.gameObject.SetActive(false);
        m_camera3.gameObject.SetActive(false);
    }
}
