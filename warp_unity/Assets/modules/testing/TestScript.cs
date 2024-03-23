using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blish_HUD;
using Blish_HUD.Input;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    [Header("References")]
    public Text textHookFeedback;
    public Text textPosFeedback;

    void Update()
    {
        // Set debug mouse
        textPosFeedback.text = "Hooked Mousepos: " + SuInput.s_v2MousePos
            + "\nNo Hook Mousepos: " + SuInput.s_v2MousePos
            + "\nMouse.current.position: " + Mouse.current.position.ReadValue()
            + "\n" + string.Format("GW2 focus: {0} - WARP focus: {1} - GW2 running: {2}", GameIntegration.Instance.m_bGw2HasFocus, GameIntegration.s_bWarpHasFocus, GameIntegration.Instance.m_bGw2IsRunning);
            //+ "\nGw2Focus " + SuInput.s_warpUtility.bGw2HasFocus + " - WarpFocus " + SuInput.s_warpUtility.bWarpHasFocus + "- GW2Run " + SuInput.s_warpUtility.bGw2IsRunning;
        textHookFeedback.text = SuInput.s_bHooksEnabled ? "HOOKED" : "";
    }

    public void ClickedButton()
    {
        Debug.Log("Button was clicked");
    }

}
