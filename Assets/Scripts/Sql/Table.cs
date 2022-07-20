using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class Table
{
    private const float TIMEOFFLINE = 1f;
    private readonly int uidIndex = -1;
    private readonly string[] columnNames;
    private readonly SortedDictionary<string, string[]> rows = new SortedDictionary<string, string[]>();
    private readonly Dictionary<string, TableParser> parsers = new Dictionary<string, TableParser>();

    public string[] Timestamps => GetColumn("TIMESTAMP");
    public string[] ColumnNames => columnNames;
    public SortedDictionary<string, string[]> Rows => rows;
    public Dictionary<string, TableParser> Parsers => parsers;

    public Table(string table)
    {
        JObject[] objects = JArray.Parse(table).ToObject<JObject[]>();
        List<string> _columnNames = new List<string>();
        List<string[]> _rows = new List<string[]>();
        List<TableParser> _parsers = new List<TableParser>();

        foreach (JProperty item in objects[0].Properties())
        {
            _columnNames.Add(item.Name.ToUpper());
            if (item.Name.StartsWith("ID"))
            {
                uidIndex = _columnNames.IndexOf(item.Name);
            }
        }

        foreach (JObject obj in objects)
        {
            List<string> _row = new List<string>();
            foreach (JProperty item in obj.Properties())
            {
                _row.Add(item.Value.ToString());
            }
            _rows.Add(_row.ToArray());
        }

        foreach (string p in _columnNames)
        {
            if (!p.StartsWith("ID") && !p.Equals("TIMESTAMP"))
            {
                _parsers.Add(new TableParser(this, p));
            }
        }

        columnNames = _columnNames.ToArray();
        uidIndex = uidIndex >= 0 && uidIndex < columnNames.Length ? uidIndex : 0;
        foreach (string[] item in _rows)
        {
            rows[item[uidIndex]] = item;
        }

        foreach (TableParser t in _parsers)
        {
            parsers[t.Field] = t;
        }

        string[] times = GetColumn("TIMESTAMP");
        DateTime last = DateTime.Parse(times[times.Length - 1]);
        DateTime now = DateTime.UtcNow.AddMinutes(-TIMEOFFLINE);
        bool isOn = last >= now;
        foreach (TableParser parser in parsers.Values)
        {
            string[] values = GetColumn(parser.Field);
            parser.Timestamps = times;
            parser.Values = values;
            parser.LastValue = values[values.Length - 1];
            parser.IsOnline = isOn;
        }
    }

    public void AddColumns(string table)
    {
        if (rows.Count > 5000)
        {
            string[] k = new List<string>(rows.Keys).ToArray();
            for (int i = 0; i < k.Length - 5000; i++)
            {
                rows.Remove(k[i]);
            }
        }

        JObject[] objects = JArray.Parse(table).ToObject<JObject[]>();
        List<string[]> _rows = new List<string[]>();

        foreach (JObject obj in objects)
        {
            List<string> _row = new List<string>();
            foreach (JProperty item in obj.Properties())
            {
                _row.Add(item.Value.ToString());
            }
            _rows.Add(_row.ToArray());
        }

        foreach (string[] item in _rows)
        {
            rows[item[uidIndex]] = item;
        }

        string[] times = GetColumn("TIMESTAMP");
        DateTime last = DateTime.Parse(times[times.Length - 1]);
        DateTime now = DateTime.UtcNow.AddMinutes(-TIMEOFFLINE);
        bool isOn = last >= now;
        foreach (TableParser parser in parsers.Values)
        {
            string[] values = GetColumn(parser.Field);
            parser.Timestamps = times;
            parser.Values = values;
            parser.LastValue = values[values.Length - 1];
            parser.IsOnline = isOn;
        }
    }

    public string[] GetColumn(string name)
    {
        int n = Array.IndexOf(columnNames, name);
        List<string> column = new List<string>();
        if (n >= 0)
        {
            foreach (string[] row in rows.Values)
            {
                column.Add(row[n]);
            }
        }
        return column.ToArray();
    }
}

public class TableParser
{
    private readonly Table table;
    private readonly string field;
    public Table Table => table;
    public string Field => field;

    public TableParser(Table t, string f)
    {
        (table, field) = (t, f);
    }

    public string[] Timestamps;
    public string[] Values;
    public string LastValue;
    public bool IsOnline;
}