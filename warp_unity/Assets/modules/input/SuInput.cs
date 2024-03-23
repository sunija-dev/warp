using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blish_HUD;
using Blish_HUD.Input;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;

// TODO: Find out why keyboard inputs aren't catched by hook (= go to old version)
// then add keyboard inputs

public class SuInput : MonoBehaviour
{
    public static InputService s_input;
    public static Vector2 s_v2MousePos = Vector2.zero;

    public static bool s_bHooksEnabled { get; private set; } = false;
    public static bool s_bWindowClickable { get; private set; } = false;

    public bool m_bDisableInEditor = true;

    [Header("References")]
    public GameIntegration m_gameIntegration;
    public Texture2D m_texCursorNormal;

    [Header("Testing")]
    public GameObject m_goTestMouseCursor;
    public GameObject m_goTestMouseCursorNoHook;
    public TMP_Text m_textHooked;
    public TMP_Text m_textClickable;
    public TMP_Text m_textRightMouseButton;

    public static Mouse s_mouseDefault;

    private bool m_bWarpFocusLast = false;
    private bool m_bDraggingWarp = false;

    private static bool s_bHooksJustEnabled = false; // no click since hooks enabled?

    void Start()
    {
        s_input = new InputService();
        s_input.Load();

        if (Application.isEditor && m_bDisableInEditor)
        {
            gameObject.SetActive(false);
            return;
        }

        // Mouse
        s_input.Mouse.LeftMouseButtonPressed += (s, args) => { MouseButtonAction(MouseButton.Left, true); };
        s_input.Mouse.LeftMouseButtonReleased += (s, args) => { MouseButtonAction(MouseButton.Left, false); };
        s_input.Mouse.RightMouseButtonPressed += (s, args) => { MouseButtonAction(MouseButton.Right, true); };
        s_input.Mouse.RightMouseButtonReleased += (s, args) => { MouseButtonAction(MouseButton.Right, false); };

        // dragging
        s_input.Mouse.LeftMouseButtonPressed += (s, args) => { if (s_bHooksEnabled) m_bDraggingWarp = true; };
        s_input.Mouse.LeftMouseButtonReleased += (s, args) => { m_bDraggingWarp = false; };

        s_input.Mouse.MouseWheelScrolled += (s, args) => { MouseScroll(args.WheelDelta); };

        s_input.Mouse.SetHudFocused(true);

        // add hook mouse
        s_mouseDefault = Mouse.current;

        Cursor.SetCursor(m_texCursorNormal, Vector2.zero, CursorMode.Auto);
        MouseButtonAction(MouseButton.Left, true); // why?
        MouseButtonAction(MouseButton.Left, false);

        ClientManager.Instance.eWindowReady.AddListener(() => StartCoroutine(coSetNotClickableDelayed()));
    }

    private IEnumerator coSetNotClickableDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        SetWindowClickable(false);
    }

    void Update()
    {
        if (!GameIntegration.s_bWarpIsVisible)
            return;

        // update inputs
        s_input.Update();

        // no hook mousepos
        float[] arPos = InputService.v2GetMousePosWithoutHook();
        s_v2MousePos.x = arPos[0] * (GameIntegration.s_rectGW2.width / Screen.width) - GameIntegration.s_rectGW2.x;
        s_v2MousePos.y = (Screen.height - arPos[1]) * (GameIntegration.s_rectGW2.height / Screen.height) + GameIntegration.s_rectGW2.y;

        InputSystem.QueueDeltaStateEvent(Mouse.current.position, s_v2MousePos);

        // Hover Ui? Start mouse hook
        bool bOverInputField;
        bool bPointingAtUI = IsPointerOverUIObject(out bOverInputField);
        bool bPointingAtClickable = HuntManager.Instance.bHoveringClickable;

        bool bDraggingCamera = !s_bHooksEnabled && s_input.Mouse.SuState.bRightButton;

        bool bHooksShouldBeActive = (bPointingAtUI || bPointingAtClickable) && !bDraggingCamera;
        bool bWindowShouldBeClickable = bOverInputField;
        if (bOverInputField)
            bHooksShouldBeActive = false;

        if (bHooksShouldBeActive && !s_bHooksEnabled)
            EnableHooks();
        if (!bHooksShouldBeActive && s_bHooksEnabled && !m_bDraggingWarp)
            DisableHooks();

        // Hover text field? Make window clickable (so it goes into focus)
        if (bWindowShouldBeClickable && !s_bWindowClickable)
            SetWindowClickable(true);
        if (!bWindowShouldBeClickable && s_bWindowClickable)
            SetWindowClickable(false);

        // switched focus to warp via click (only on text boxes)? Repeat the click in unity
        if (!m_bWarpFocusLast && GameIntegration.s_bWarpHasFocus)
        {
            MouseButtonAction(MouseButton.Left, true);
            MouseButtonAction(MouseButton.Left, false);
            MouseButtonAction(MouseButton.Right, true); 
            MouseButtonAction(MouseButton.Right, false);
        }
        m_bWarpFocusLast = GameIntegration.s_bWarpHasFocus;

        if (GameIntegration.s_bWarpHasFocus && !IsInputFieldFocused())
            FocusGW2();
        if (GameIntegration.s_bWarpHasFocus && Mouse.current.leftButton.isPressed && !bWindowShouldBeClickable)
            FocusGW2();

        // DEBUG DISPLAY
        if (m_goTestMouseCursor)
            m_goTestMouseCursor.transform.position = Mouse.current.position.ReadValue();
        if (m_goTestMouseCursorNoHook)
            m_goTestMouseCursorNoHook.transform.position = s_v2MousePos;

        m_textHooked.text = s_bHooksEnabled ? "HOOKED" : "";
        m_textClickable.text = s_bWindowClickable ? "CLICKABLE" : "";
    }

    public static void EnableHooks()
    {
        s_input.EnableHooks();
        s_bHooksEnabled = true;
        s_bHooksJustEnabled = true;

        // DEBUG remove real mouse
        /*
        InputSystem.RemoveDevice(Mouse.current);
        Mouse mouse = InputSystem.AddDevice<Mouse>();
        mouse.MakeCurrent();
        */
    }

    public static void DisableHooks()
    {
        s_input.DisableHooks();
        s_bHooksEnabled = false;

        // DEBUG add real mouse
        /*
        InputSystem.RemoveDevice(Mouse.current);
        InputSystem.AddDevice(s_mouseDefault);
        s_mouseDefault.MakeCurrent();
        */
    }

    public static void SetWindowClickable(bool _bClickable)
    {
        s_bWindowClickable = _bClickable;
        TransparentWindow.SetClickthrough(!_bClickable);
    }

    private void OnApplicationQuit()
    {
        s_input?.Unload();
    }

    // from: https://answers.unity.com/questions/967170/detect-if-pointer-is-over-any-ui-element.html
    public bool IsPointerOverUIObject(out bool o_bInputField)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(s_v2MousePos.x, s_v2MousePos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        List<RaycastResult> liResults = new List<RaycastResult>(results);

        // only search first 4 elements, so it doesn't x-ray. 4 because text, and text preview is in front of it (and sth else?)
        bool bPointerOverUi = results.Count > 0;

        o_bInputField = liResults.Count > 0 
            && bPointerOverUi 
            && liResults.GetRange(0, Mathf.Min(liResults.Count, 4))
            .Any(x => x.gameObject.GetComponent<TMP_InputField>() != null);

        /*string strTestString = "";
        for (int i = 0; i < liResults.Count; i++)
            strTestString += liResults[i].gameObject.GetComponent<TMP_InputField>() != null ? 1 : 0;
        m_textClickable.text = strTestString;*/

        //Debug.Log("Inputfield hover: " + o_bInputField);

        return bPointerOverUi;
    }

    public bool IsPointerOverClickable3DObject()
    {
        return false;
    }

    private static bool bInputFieldFocusedLast = false;
    // from: https://forum.unity.com/threads/checking-if-an-input-field-is-focused.272148/
    public static bool IsInputFieldFocused()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        bool bInputFieldFocused = (obj != null && obj.GetComponent<TMP_InputField>() != null);
        //if (bInputFieldFocused != bInputFieldFocusedLast)
        //    Debug.Log("Inputfield focus changed: " + bInputFieldFocused);
        bInputFieldFocusedLast = bInputFieldFocused;
        return bInputFieldFocused;
    }

    public void MouseButtonAction(MouseButton _mouseButton, bool _bValue)
    {
        // if the first thing in warp is releasing the right mouse button, push that also to gw2, so cam doesn't get stuck
        if (s_bHooksJustEnabled && _mouseButton == MouseButton.Right && !_bValue)
        {
            DisableHooks();
            Blish_HUD.Controls.Intern.Mouse.Release(Blish_HUD.Controls.Intern.MouseButton.RIGHT, (int)s_v2MousePos.x, (int)s_v2MousePos.y, true); // to stop bug that has actioncam thingy?
            EnableHooks();
        }
        s_bHooksJustEnabled = false;

        MouseState mouseState = new MouseState { position = s_v2MousePos };
        mouseState.WithButton(_mouseButton, _bValue);
        InputSystem.QueueStateEvent(Mouse.current, mouseState);
    }

    public void MouseScroll(int _iValue)
    {
        // HACK TODO - if scrolling up value gets way too high
        if (_iValue < -1000)
            _iValue = 120;

        MouseState mouseState = new MouseState { position = s_v2MousePos };
        mouseState.scroll = new Vector2(0f, _iValue);
        InputSystem.QueueStateEvent(Mouse.current, mouseState);
    }


    // ===================== TESTING =====================

    // TODO: DOESNT WORK YET
    public void FocusGW2()
    {
        m_gameIntegration.FocusGw2();
    }

    // TODO: DOESNT WORK YET
    public void FocusWarp()
    {
        try
        {
            //WindowUtil.SetForegroundWindowEx(s_hwndWarpFormHandle);
        }
        catch (System.Exception e)
        {
            Debug.Log("Couldnt focus warp: " + e.Message);
        }
    }

    /*
      
    === KEYBOARD PRESS
    Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
    KeyboardState stateA = new KeyboardState();
    KeyboardState stateB = new KeyboardState();
    stateA.Press(Key.I);
    stateB.Release(Key.I);
     
    InputSystem.QueueStateEvent(keyboard, stateA);
    InputSystem.QueueStateEvent(keyboard, stateB);
 
    === KEEP REAL KEYBOARD FROM FIRING
    InputSystem.DisableDevice(InputDevice)


    === MOVE MOUSE LONG
    var inputEvent = InputSystem.CreateEvent<PointerMoveEvent>();
    inputEvent.deviceType = typeof(Mouse);
    inputEvent.deviceIndex = 0;
    inputEvent.delta = myMouseDelta;
    inputEvent.position = myMousePosition;
    InputSystem.QueueEvent(inputEvent);

    === MOUSE CLICK LONG
    var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
    inputEvent.deviceType = typeof(Mouse);
    inputEvent.deviceIndex = 0;
    inputEvent.controlIndex = 0;
    inputEvent.value = clicked ? 1.0f : 0.0f;
    InputSystem.QueueEvent(inputEvent);

    == MOVE WINDOWS MOUSE
    Mouse.current.WarpCursorPosition(new Vector2(123, 234));

    */
}
