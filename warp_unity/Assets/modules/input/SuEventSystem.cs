using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SuEventSystem : EventSystem
{
    
    protected override void OnApplicationFocus(bool hasFocus)
    {
        // do nothing so it doesn't loose focus
    }
    
}
