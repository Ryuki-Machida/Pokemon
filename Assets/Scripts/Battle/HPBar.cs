using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject m_health;

    public void SetHP(float hpNormalized)
    {
        m_health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    /// <summary>
    /// HPバーを徐々に減らす処理
    /// </summary>
    public IEnumerator SetHPSmooth(float newHp)
    {
        float curHp = m_health.transform.localScale.x;
        float changeAmt = curHp - newHp;

        while (curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime;
            m_health.transform.localScale = new Vector3(curHp, 1f); //1fはゲージの太さ
            yield return null;
        }
        m_health.transform.localScale = new Vector3(newHp, 1f);
    }
}
