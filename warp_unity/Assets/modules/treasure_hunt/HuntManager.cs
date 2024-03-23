using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class HuntManager : MonoBehaviour
{
    public static HuntManager Instance;

    public GameObject goCluePrefab;
    public GameObject goQuestGiverPrefab;
    public List<HuntObjectData> liHuntObjects;

    public QuestGiverUI ui;
    [HideInInspector] public bool bHoveringClickable = false;
    private GameIntegration gameIntegration;
    private List<GameObject> liSpawnedObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameIntegration = GameIntegration.Instance;
        RebuildObjects();
        ClientManager.Instance.eMapChanged.AddListener(RebuildObjects);
        ClientManager.Instance.eCharLoggedIn.AddListener(RebuildObjects);
    }

    void Update()
    {
        if (Camera.main == null)
            return;

        if ((gameIntegration.m_bGw2HasFocus || GameIntegration.s_bWarpHasFocus))
        {
            RaycastHit rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            bHoveringClickable = Physics.Raycast(ray.origin, ray.direction, out rayHit, 1000f) && rayHit.collider.tag == "Clickable";

            if (bHoveringClickable && (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed))
            {
                HuntObject huntObject = rayHit.collider.gameObject.GetComponent<HuntObject>();
                if (huntObject != null)
                    huntObject.OnClick();
            }
        }
    }

    public void RebuildObjects()
    {
        if (Player.Instance == null)
            return;

        //Debug.Log("Rebuilding " + MumbleManager.Instance.iMapID);

        foreach (GameObject go in liSpawnedObjects)
            Destroy(go);
        liSpawnedObjects.Clear();

        foreach (HuntObjectData objectData in liHuntObjects)
        {
            // only on same map
            if (objectData.gw2pos.iMapID != MumbleManager.Instance.iMapID)
                continue;

            // spawn types with right prefabs
            if (objectData.GetType() == typeof(ClueData))
            {
                GameObject goNew = Instantiate(goCluePrefab, objectData.gw2pos.v3Pos, Quaternion.identity);
                goNew.GetComponent<HuntClue>().huntObjectData = objectData;
                goNew.GetComponent<HuntClue>().SetVisibility(false);
                liSpawnedObjects.Add(goNew);
            }

            if (objectData.GetType() == typeof(QuestGiverData))
            {
                GameObject goNew = Instantiate(goQuestGiverPrefab, objectData.gw2pos.v3Pos, Quaternion.identity);
                goNew.GetComponent<HuntQuestGiver>().huntObjectData = objectData;
                goNew.GetComponent<HuntQuestGiver>().SetVisibility(false);
                liSpawnedObjects.Add(goNew);
            }
        }
    }

    public void WriteGW2Pos()
    {
        string strPath = $"{Application.dataPath}/modules/treasure_hunt/saved_pos.txt";

        if (!File.Exists(strPath))
            File.WriteAllText(strPath, "Saves positions\n");

        File.AppendAllText(strPath, $"\n\n{Player.Instance.transform.position}\n{Player.Instance.warpNetworkPosition.iMapId}");

        Debug.Log($"Wrote position to {strPath}.");
    }
}
