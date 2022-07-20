using System.Collections;
using UnityEngine;

public class SqlPreloader : MonoBehaviour
{
    [SerializeField] private string[] tables;

    private int wait = 0;

    public bool Ready => wait == 0 && tables == null;

    private void Start()
    {
        foreach (string t in tables)
        {
            StartCoroutine(GetTable(t.ToUpper()));
        }

        tables = null;
    }

    private IEnumerator GetTable(string table)
    {
        wait++; yield return App.RequestTable(table); wait--;
    }
}
