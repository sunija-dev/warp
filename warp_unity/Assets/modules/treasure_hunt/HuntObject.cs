using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class HuntObject : MonoBehaviour
{
    public HuntObjectData huntObjectData;

    private bool bVisible = false;

    private void Update()
    {
        if (Player.Instance == null || huntObjectData == null)
            return;

        bool bInRange = Vector3.Distance(Player.Instance.transform.position, transform.position) < huntObjectData.fSpawnDistance;

        if (bInRange && !bVisible)
        {
            bVisible = true;
            SetVisibility(true);
        }

        if (!bInRange && bVisible)
        {
            bVisible = false;
            SetVisibility(false);
        }
    }

    public abstract void SetVisibility(bool _bVisible);

    public abstract void OnClick();
}

