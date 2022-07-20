using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    [SerializeField] private GameObject loadingSphere;
    [SerializeField] private string dbAddress, database;
    [SerializeField] private float tablesRefreshTime;
    [SerializeField] private float maxMachineDistance;
    [SerializeField] private SqlPreloader preloader;
    [SerializeField] private EditorMenu editorMenu;
    [SerializeField] private HandMenu handMenu;
    [SerializeField] private ClusterGraph clusterGraph;
    [SerializeField] private AlarmDialog alarmDialog;
    [SerializeField] public bool alarmEnabled;

    private readonly List<GaugeController> gauges = new List<GaugeController>();
    private readonly SortedDictionary<string, Table> tables = new SortedDictionary<string, Table>();
    private readonly SortedDictionary<string, ClusterGraph> clusters = new SortedDictionary<string, ClusterGraph>();

    private void Awake()
    {
        Instance = this;
    }

    public static App Instance { get; private set; }
    public static string DBAddress => Instance.dbAddress;
    public static string Database => Instance.database;
    public static float TablesRefreshTime => Instance.tablesRefreshTime;
    public static SqlPreloader Preloader => Instance.preloader;
    public static EditorMenu EditorMenu => Instance.editorMenu;
    public static HandMenu HandMenu => Instance.handMenu;
    public static SortedDictionary<string, Table> Tables => Instance.tables;
    public static List<GaugeController> Gauges => Instance.gauges;
    public static SortedDictionary<string, ClusterGraph> Clusters => Instance.clusters;
    public static AlarmDialog AlarmDialog => Instance.alarmDialog;
    public static float MaxMachineDistance => Instance.maxMachineDistance;
    public static bool AlarmEnabled => Instance.alarmEnabled;
    private static WorldLockingManager world;

    private void Start()
    {
        StartCoroutine(LoadedWorld());
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private IEnumerator LoadedWorld()
    {
        yield return new WaitUntil(() => Preloader.Ready);
        App.Log($"Preloarder ready {string.Join(", ", App.Tables.Keys)}");
        editorMenu.SetupMenus();
        StartCoroutine(UpdateTables());
        
        world = WorldLockingManager.GetInstance();
        yield return new WaitUntil(() => world.Enabled);

        SaveState.LoadStates((ulong)world.FragmentManager.CurrentFragmentId);
        Destroy(loadingSphere, 1f);
    }

    private IEnumerator UpdateTables()
    {
        while (true)
        {
            yield return new WaitForSeconds(TablesRefreshTime);
            foreach (string k in tables.Keys)
            {
                StartCoroutine(UpdateTable(k));
            }
        }
    }

    public static void AddTable(string name, string table)
    {
        try
        {
            AddTable(name, new Table(table));
        }
        catch
        {
            App.Log("Empty table");
        }
    }

    public static void UpdateTable(string name, string table)
    {
        Tables[name].AddColumns(table);
    }

    public static void AddTable(string name, Table table)
    {
        Tables[name] = table;
        ClusterGraph cluster = Instantiate(Instance.clusterGraph);
        cluster.Machine = name;
        Clusters[name] = cluster;
    }

    public static void AddGauge(GaugeController gauge)
    {
        gauge.SetGaugeConstraints();
        Gauges.Add(gauge);
        Save();
    }

    public static void RemGauge(GaugeController gauge)
    {
        Gauges.Remove(gauge);
    }

    public static void SetGaugeConstraints()
    {
        foreach (GaugeController g in Gauges)
        {
            g.SetGaugeConstraints();
        }
    }

    internal static void Save()
    {
        SaveState.SaveStates(Gauges, (ulong)world.FragmentManager.CurrentFragmentId);
    }

    public static IEnumerator RequestTable(string table) { yield return SqlRequester.Request(DBAddress, Database, table, 3000, AddTable); }
    public static IEnumerator UpdateTable(string table) { yield return SqlRequester.Request(DBAddress, Database, table, 10, UpdateTable); }

    public static void OpenEditorMenu(GaugeController gauge)
    {
        EditorMenu.OpenMenu(gauge);
    }

    public static void SetActiveMachine(IEnumerable<GaugeController> machine, bool active)
    {
        foreach (GaugeController g in machine)
        {
            g.gameObject.SetActive(active);
        }
    }

    public static void ActivateAlarm(GaugeController gc)
    {
        if (AlarmEnabled)
            AlarmDialog.SetAlarm(gc.Machine, gc.Quantity, gc.Units, gc.Value, gc.Min, gc.Max);
    }

    public static void ResetAnchors()
    {
        WorldLockingManager.GetInstance().FragmentManager.Reset();
        Application.Quit();
    }

    public static void ResetElements()
    {
        Gauges.ForEach(g => Destroy(g));
        Gauges.Clear();
        Application.Quit();
    }

    public static TableParser GetParser(string machine, string quantity = null, bool hasQuantity = false)
    {
        if (machine == null || !Tables.ContainsKey(machine))
        {
            return null;
        }

        if (hasQuantity && (quantity == null || !Tables[machine].Parsers.ContainsKey(quantity)))
        {
            return null;
        }

        quantity = hasQuantity ? quantity : App.Tables[machine].ColumnNames[2];

        return Tables[machine].Parsers[quantity];
    }

    [SerializeField] private TMPro.TMP_Text debugText;
    public static TMPro.TMP_Text DebugText => Instance.debugText;

    public static void Log(string txt)
    {
        DebugText.text += "\n" + txt;
        Debug.Log(txt);
    }
}
