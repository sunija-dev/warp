using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntQuestGiver : HuntObject
{
    public List<GameObject> liVisuals;

    private QuestGiverUI ui;
    private QuestGiverData data;

    public void Start()
    {
        ui = HuntManager.Instance.ui;
        ui.buttonYes.onClick.AddListener(CheckKeyword);
        ui.buttonNo.onClick.AddListener(() => ui.gameObject.SetActive(false));
        data = (QuestGiverData)huntObjectData;
    }

    public override void OnClick()
    {
        ui.gameObject.SetActive(true);
        ui.input.gameObject.SetActive(true);
        ui.text.text = data.strText;
    }

    public override void SetVisibility(bool _bVisible)
    {
        StartCoroutine(coSetVisbility(_bVisible));
    }

    private IEnumerator coSetVisbility(bool _bVisible)
    {
        GetComponent<Animator>().SetTrigger("appear");

        yield return new WaitForSeconds(0.25f);

        GetComponent<CapsuleCollider>().enabled = _bVisible;
        foreach (GameObject go in liVisuals)
            go.SetActive(_bVisible);
    }

    public void CheckKeyword()
    {
        if (ui.input.text.Trim().Equals(data.strKeyword, System.StringComparison.InvariantCultureIgnoreCase))
        {
            ui.text.text = data.strTextSuccess;
            ui.input.gameObject.SetActive(false);

            MapManager.Instance.bShowWikiButton = true;
            //MapManager.Instance.bShowWikiMap = true;
        }
        else
        {
            ui.text.text = data.strTextFail;
        }
    }
}
