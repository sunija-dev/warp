using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharHoverManager : MonoBehaviour
{
    public static CharHoverManager Instance;
    public WindowCharInfo windowCharInfo;

    public float fAppearTime = 1f;
    public float fDisappearTime = 3f;

    private string strSelected = "";
    private string strHovering = "";
    private float fHoveringNewFor = 0f;
    private float fNotHoveringFor = 0f;

    private bool bShowPersistent = false;
    private bool bShowOnHover = true;
    private GameIntegration gameIntegration;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameIntegration = GameIntegration.Instance;
    }

    void Update()
    {
        // point for 1 sec: show
        // not hover on it for 3 sec: hide
        // hover -> show until close

        if (Camera.main == null)
            return;

        if (bShowOnHover && (gameIntegration.m_bGw2HasFocus || GameIntegration.s_bWarpHasFocus))
        {
            RaycastHit rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool bRayHit = Physics.Raycast(ray.origin, ray.direction, out rayHit, 1000f) && rayHit.collider.tag == "Player";

            if (bRayHit && rayHit.transform.parent != Player.Instance.transform)
            {
                fNotHoveringFor = 0f;
                strHovering = rayHit.collider.transform.parent.name;

                if (strHovering != strSelected)
                    fHoveringNewFor += Time.deltaTime;
            }
            else
            {
                fNotHoveringFor += Time.deltaTime;
                strSelected = "";
            }

            // hovering other person? change display
            if (bRayHit && strSelected != strHovering && fHoveringNewFor > fAppearTime)
            {
                fHoveringNewFor = 0f;
                SelectChar(rayHit.collider.transform.parent);
            }

            if (!bShowPersistent && fNotHoveringFor > fDisappearTime && windowCharInfo.bIsVisible)
                windowCharInfo.Hide();
        }
    }

    public void SelectChar(Transform _transChar, bool _bPersistent = false)
    {
        strSelected = _transChar.name;

        Character character = _transChar.GetComponent<Character>();
        AccountSheet accountSheet = _transChar.GetComponent<Player>().accountSheet;
        windowCharInfo.SetCharacter(character.charSheet, accountSheet);

        if (!windowCharInfo.bIsVisible)
            windowCharInfo.Show();

        if (_bPersistent)
            MakePersistent();
    }

    public void EnteredCharPreview()
    {
        MakePersistent();
    }

    public void ClosedCharWindow()
    {
        bShowPersistent = false;
    }

    private void MakePersistent()
    {
        bShowPersistent = true;
        windowCharInfo.canvasGroup.alpha = 1f;
    }

    public void SetShowOnHover(bool _bShow)
    {
        bShowOnHover = _bShow;
    }
}
