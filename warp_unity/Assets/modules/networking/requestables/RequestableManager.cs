using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Handles requesting objects.
/// </summary>
public class RequestableManager : NetworkBehaviour
{
    public static RequestableManager InstanceLocal;

    private void Start()
    {
        if (isLocalPlayer)
            InstanceLocal = Player.Instance.GetComponent<RequestableManager>(); // idk, did it the same way for ipchanger
    }

    private int m_iLatestRequestID = 0;
    private Dictionary<int, Requestable> m_dictRequestables = new Dictionary<int, Requestable>();

    public IEnumerator coRequest(Requestable _requestable)
    {
        int iRequestID = m_iLatestRequestID + 1;
        m_iLatestRequestID = iRequestID;

        m_dictRequestables.Add(iRequestID, _requestable);

        CmdRequest(iRequestID, _requestable);
        yield return new WaitUntil(() => m_dictRequestables[iRequestID].bReceived);
        _requestable = m_dictRequestables[iRequestID];

        m_dictRequestables.Remove(iRequestID);

        yield return _requestable;
    }

    [Command]
    private void CmdRequest(int _iRequestID, Requestable _requestable)
    {
        _requestable.LoadData(connectionToClient);
        TargetReceive(_iRequestID, _requestable);
    }

    [TargetRpc]
    private void TargetReceive(int _iRequestID, Requestable _requestable)
    {
        _requestable.bReceived = true;
        m_dictRequestables[_iRequestID] = _requestable;
    }

    [Serializable]
    public class Requestable 
    {
        public bool bReceived = false;
        public virtual void LoadData(NetworkConnection _connectionToClient) { }
    }
}






