using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSmallWindow : MonoBehaviour
{
    public TransparentWindow transparentWindow;

    private Vector2 v2ScreenSize = Vector2.zero;

    void Start()
    {
        if (Application.isEditor)
        {
            gameObject.SetActive(false);
            return;
        }

        v2ScreenSize.x = Screen.currentResolution.width;
        v2ScreenSize.y = Screen.currentResolution.height;

        PlayerPrefs.DeleteAll(); // just in case, so it doesn't remember screen sizes
        StartCoroutine(transparentWindow.coSetupOverlay());
    }

    private void Update()
    {
        return;
        Debug.Log("Fullscreen: " + Screen.fullScreen);
        Debug.Log("Fullscreen mode: " + Screen.fullScreenMode);
        Debug.Log("Cam pixelwidth: " + Camera.main.pixelWidth);
        Debug.Log("Time.deltatime: " + Time.deltaTime);
        Debug.Log("Targetframerate: " + Application.targetFrameRate);
        Debug.Log("");
    }



    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll(); // just in case, so it doesn't remember screen sizes
    }
}
