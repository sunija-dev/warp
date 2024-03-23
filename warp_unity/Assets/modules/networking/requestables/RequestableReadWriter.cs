using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public static class RequestableReaderWriter
{
    // UPDATE THIS LIST and in READER
    public static List<Type> liRequestableTypes = new List<Type>
    {
        typeof(CopySheetPopup.SheetListRequest),
        typeof(CopySheetPopup.SheetRequest)
    };



    // internal functions for Mirror

    public static void WriteRequestable(this NetworkWriter writer, RequestableManager.Requestable _requestable)
    {
        writer.WriteInt(liRequestableTypes.FindIndex(x => x == _requestable.GetType()));
        writer.WriteString(JsonUtility.ToJson(_requestable));
    }

    public static RequestableManager.Requestable ReadRequestable(this NetworkReader reader)
    {
        Type type = liRequestableTypes[reader.ReadInt()];

        // UPDATE HERE if new requestable!
        if (type == typeof(CopySheetPopup.SheetListRequest))
            return JsonUtility.FromJson<CopySheetPopup.SheetListRequest>(reader.ReadString());
        else if (type == typeof(CopySheetPopup.SheetRequest))
            return JsonUtility.FromJson<CopySheetPopup.SheetRequest>(reader.ReadString());

        return new RequestableManager.Requestable();
    }
}
