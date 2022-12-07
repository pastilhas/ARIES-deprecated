using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Class Table
 * Represents a table of values
 */

public class Table
{
    private const string timestampIndex = "TIMESTAMP";
    public static int MAX_SIZE = 5000;

    public static readonly Dictionary<string, Table> Tables = new Dictionary<string, Table>();

    public readonly List<string> ColumnNames = new List<string>();
    private readonly List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();
    public Table(string table, string data)
    {
        UpdateTable(data);
        ColumnNames = Rows.First().Keys.ToList();
        Tables[table.ToUpper()] = this;
    }

    public static void UpdateTable(string name, string data)
    {
        Tables[name].UpdateTable(data);
    }

    private void UpdateTable(string data)
    {
        JObject[] array = JArray.Parse(data).ToObject<JObject[]>();
        foreach (JObject obj in array)
        {
            Dictionary<string, string> dict = obj.ToObject<Dictionary<string, string>>();
            Rows.Add(dict.ToDictionary(x => x.Key.ToUpper(), x => x.Value));
            if (Rows.Count > MAX_SIZE) Rows.RemoveAt(0);
        }

        Rows.Sort((x, y) => x[timestampIndex].CompareTo(y[timestampIndex]));
    }

    public static string[] GetTimestamps(string name)
    {
        if (Tables.ContainsKey(name))
        {
            return Tables[name].Rows.Select(x => x[timestampIndex]).ToArray();
        }
        else
        {
            return null;
        }
    }

    public static string[] GetColumn(string name, string column)
    {
        if (Tables.ContainsKey(name) && Tables[name].Rows[0].ContainsKey(column))
        {
            return Tables[name].Rows.Select(x => x[column]).ToArray();
        }
        else
        {
            return null;
        }
    }

    public static string GetValue(string name, string column, int row)
    {
        if (Tables.ContainsKey(name) && Tables[name].Rows.Count > row && Tables[name].Rows[row].ContainsKey(column))
        {
            return Tables[name].Rows[row][column];
        }
        else
        {
            return null;
        }
    }

    public static string GetLastValue(string name, string column)
    {
        if (Tables.ContainsKey(name) && Tables[name].Rows[0].ContainsKey(column))
        {
            return Tables[name].Rows.Last()[column];
        }
        else
        {
            return null;
        }
    }

    public static bool TableExists(string table)
    {
        return Tables.ContainsKey(table);
    }

    public static bool ColumnExists(string table, string column)
    {
        return Tables.ContainsKey(table) && Tables[table].ColumnNames.Contains(column);
    }

    public static bool IsOnline(string machine)
    {
        DateTime last = DateTime.Parse(GetTimestamps(machine).Last());
        double diff1 = last.Subtract(DateTime.UtcNow).TotalMinutes, diff2 = DateTime.UtcNow.Subtract(last).TotalMinutes;
        return Math.Max(diff1, diff2) < 1;
    }

    public static bool IsAlarm(string machine)
    {
        if (!IsOnline(machine))
        {
            return false;
        }

        bool ret = false;
        Element.Elements.Where(x => x.Machine == machine).ToList().ForEach(x =>
        {
            if (!x.hasQuantity || !x.hasLimits)
            {
                return;
            }

            float last = float.Parse(GetLastValue(machine, x.Quantity));
            if (last < x.Min || last > x.Max)
            {
                ret = true;
                return;
            }
        });
        return ret;
    }

    public static IEnumerable<string> GetColumnNames(string machine)
    {
        return Tables[machine].ColumnNames;
    }
}
