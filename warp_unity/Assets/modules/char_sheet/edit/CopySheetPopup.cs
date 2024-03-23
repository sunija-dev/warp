using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class CopySheetPopup : MonoBehaviour
{
    public CategorySheet categorySheet;
    public TMP_Dropdown m_dropdown;
    public Button m_buttonOk;
    public Button m_buttonAbort;

    public void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(coOpen());
    }

    public void Close()
    {
        m_dropdown.ClearOptions();
        gameObject.SetActive(false);
    }

    private IEnumerator coOpen()
    {
        m_buttonAbort.onClick.AddListener(Close);

        SheetListRequest sheetList = new SheetListRequest();
        CoroutineWithData coRequest = new CoroutineWithData(this, RequestableManager.InstanceLocal.coRequest(sheetList));
        yield return coRequest.coroutine;
        sheetList = (SheetListRequest) coRequest.result;

        m_dropdown.ClearOptions();
        m_dropdown.AddOptions(sheetList.liSheets);

        m_buttonOk.onClick.AddListener(ClickOk);
        m_buttonAbort.onClick.AddListener(Close);
    }

    public void ClickOk()
    {
        StartCoroutine(coLoadSheet());
    }

    private IEnumerator coLoadSheet()
    {
        SheetRequest sheetRequest = new SheetRequest();
        sheetRequest.strCharName = m_dropdown.options[m_dropdown.value].text;
        CoroutineWithData coRequest = new CoroutineWithData(this, RequestableManager.InstanceLocal.coRequest(sheetRequest));
        yield return coRequest.coroutine;
        sheetRequest = (SheetRequest) coRequest.result;

        categorySheet.SetCharSheet(sheetRequest.charSheet);
        Close();
    }


    [Serializable]
    public class SheetListRequest : RequestableManager.Requestable
    {
        public List<string> liSheets = null;

        public override void LoadData(Mirror.NetworkConnection _connectionToClient)
        {
            string strAccountID = ServerManager.s_dictPlayers[_connectionToClient].account.accountInfo.id;
            liSheets = Database.Instance.liLoadAllCharSheetNames(strAccountID);
        }
    }

    [Serializable]
    public class SheetRequest : RequestableManager.Requestable
    {
        public string strCharName = "";
        public CharSheet charSheet = null;

        public override void LoadData(Mirror.NetworkConnection _connectionToClient)
        {
            string strAccountID = ServerManager.strGetAccountID(_connectionToClient);

            charSheet = Database.Instance.charSheetLoad(strAccountID, strCharName);
        }
    }

    /*
    public void OpenWindow(string user)
    {
        gameObject.SetActive(true);
        StartCoroutine(coOpenWindow);
    }

    public IEnumerator coOpenWindow(string user)
    {
        // set parameters for request
        HaikuInfo haikuInfo = new HaikuInfo();
        haikuInfo.user = user;

        // wait for request
        CoroutineWithData request = new CoroutineWithData(this, RequestableManager.Instance.coRequest(haikuInfo));
        yield return request.coroutine;
        haikuInfo = (HaikuInfo)request.result;

        // display values
        textDisplay.text = haikuInfo[0];
    }

    public class HaikuInfo : RequestableManager.Requestable
    {
        public string user; // parameter
        public List<string> haikus; // output

        public override void LoadData(Mirror.NetworkConnection _connectionToClient)
        {
            string accountRequesting = ServerManager.players[_connectionToClient].account.accountInfo.id;
            haikus = Database.Instance.LoadHaikus(accountRequesting, user);
        }
    }
    */
}
