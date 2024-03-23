using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSwitcher : MonoBehaviour
{

    public GameObject[] arEffects;
    public float m_fSwitchEvery = 10f;
    private int iCurrEffect = 0;

    void Start()
    {
        StartCoroutine(SwitchEffect());
    }

    public IEnumerator SwitchEffect()
    {
        arEffects[iCurrEffect].SetActive(false);
        iCurrEffect = (iCurrEffect + 1) % arEffects.Length;
        arEffects[iCurrEffect].SetActive(true);
        yield return new WaitForSeconds(m_fSwitchEvery);
        StartCoroutine(SwitchEffect());
    }
}
