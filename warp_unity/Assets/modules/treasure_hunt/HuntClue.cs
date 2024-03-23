using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntClue : HuntObject
{
    public GameObject m_goEffect;

    private Coroutine coTransitionAnimation = null;
    private float fEffectSize = 1f;

    private void Start()
    {
        fEffectSize = m_goEffect.transform.localScale.x;
    }

    public override void OnClick()
    {
        ClientManager.Instance.windowPopup.Init(
                            new WindowPopup.ButtonInfo(true, null, "ok"),
                            new WindowPopup.ButtonInfo(false, null, ""),
                            ((ClueData)huntObjectData).strText);
    }

    public override void SetVisibility(bool _bVisible)
    {
        if (coTransitionAnimation != null)
            StopCoroutine(coTransitionAnimation);
        coTransitionAnimation = StartCoroutine(coSetVisible(_bVisible));

        GetComponent<SphereCollider>().enabled = _bVisible;
    }

    public IEnumerator coSetVisible(bool _bVisible)
    {
        float fTargetSize = _bVisible ? fEffectSize : 0f;
        while (Mathf.Abs(fTargetSize - m_goEffect.transform.localScale.x) > 0.01f)
        {
            m_goEffect.transform.localScale = Vector3.one * Mathf.Lerp(m_goEffect.transform.localScale.x, fTargetSize, Time.deltaTime * 5f);
            yield return null;
        }

        m_goEffect.transform.localScale = Vector3.one * fTargetSize;
        coTransitionAnimation = null;
    }
}
