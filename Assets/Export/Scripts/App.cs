using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }
    private static string address;
    public static Transform AnchorPrefab => Instance.anchorPrefab;
    public static Transform WorldRootAnchor => Instance.worldRootAnchor;
    public static string QRCodeStart => Instance.qRCodeStart;

    [SerializeField] private string qRCodeStart;
    [SerializeField] private Transform worldRootAnchor;
    [SerializeField, Range(0, 5000)] private int tableMaxSize = 720;
    [SerializeField, Range(0, 100)] private int machineMaxDistance = 10;
    [SerializeField, NonReorderable] private List<string> machineList;
    [SerializeField] private string _address;
    [SerializeField, NonReorderable] private List<Element> elementPrefabs;
    [SerializeField] private AlarmNotification notificationPrefab;
    [SerializeField] private TMPro.TMP_Text debugText;
    [SerializeField] private Transform anchorPrefab;

    private void Start()
    {
        Instance = this;

        Table.MAX_SIZE = tableMaxSize;
        Element.MAX_DIST = machineMaxDistance;
        Element.prefabs = elementPrefabs;
        AlarmNotification.prefab = notificationPrefab;
        address = _address;

        TCPClient.Start();
        StartCoroutine(RequestCodes());
    }

    private void Update()
    {
        TCPClient.Update();
    }

    private static IEnumerator RequestCodes()
    {
        yield return new WaitForSeconds(1f);
        TCPClient.SendMessage(new { msgtype = 1, timestamp = $"{DateTime.UtcNow:yyyyMMddHHmmss}", msg = "" });
        yield return new WaitForSeconds(1f);
        TCPClient.SendMessage(new { msgtype = 2, timestamp = $"{DateTime.UtcNow:yyyyMMddHHmmss}", msg = "" });
    }

    private void OnApplicationQuit()
    {
        TCPClient.SendMessage(new { msgtype = 5, timestamp = $"{DateTime.UtcNow:yyyyMMddHHmmss}", msg = "" });
    }

    public static void UpdateTable(string table, string data)
    {
        if (Table.TableExists(table))
        {
            Table.UpdateTable(table, data);
        }
        else
        {
            new Table(table, data);
        }
    }

    public void ToggleDebug()
    {
        debugText.transform.parent.gameObject.SetActive(!debugText.gameObject.activeInHierarchy);
    }

    private static int debugId = 0;
    private static readonly string[] debugLines = new string[20];
    public static void Log(string txt)
    {
        debugLines[debugId] = txt.Length > 80 ? txt.Substring(0, 80) : txt;
        debugId = (debugId + 1) % debugLines.Length;
        Instance.debugText.text = "Debug\n" + string.Join("\n", debugLines);
    }
}
