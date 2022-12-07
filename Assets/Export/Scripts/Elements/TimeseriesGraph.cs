using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimeseriesGraph : Graph
{
    [SerializeField] private float secondsPerPoint;
    [SerializeField] private int maxPoints;


    [SerializeField] private SpriteRenderer pointPrefab;
    [SerializeField] private TMPro.TMP_Text title, units;
    [SerializeField, NonReorderable] private TMPro.TMP_Text[] xText, yText;
    [SerializeField] private Transform zeroPoint;
    [SerializeField] private TMPro.TMP_Text floatingText;

    private SpriteRenderer[] points;
    private Vector3 pointStart;
    private Vector3 delta;

    private void Start()
    {
        points = new SpriteRenderer[maxPoints];
        pointStart = zeroPoint.localPosition;

        float xdelta = Vector3.Distance(xText.Last().transform.position, xText[0].transform.position);
        float ydelta = Vector3.Distance(yText.Last().transform.position, yText[0].transform.position);
        delta = new Vector3(xdelta, ydelta);

        for (int i = 0; i < maxPoints; i++)
        {
            SpriteRenderer point = Instantiate(pointPrefab, transform);
            point.enabled = false;
            points[i] = point;
        }
    }

    public override void UpdateValues(Element elem)
    {
        title.text = elem.Title;
        units.text = elem.Units;

        if (elem.Time <= 0 || elem.Time > maxPoints * secondsPerPoint / 60f)
        {
            elem.Time = maxPoints * secondsPerPoint / 60f;
        }

        DateTime last = DateTime.Parse(Table.GetTimestamps(elem.Machine).Last());
        DateTime first = last.AddMinutes(elem.Time);
        int maxTime = (int)last.Subtract(first).TotalMilliseconds;

        IEnumerable<float> values = Table.GetColumn(elem.Machine, elem.Quantity).Select(x => Mathf.Clamp(float.Parse(x), elem.Min, elem.Max));
        IEnumerable<double> times = Table.GetTimestamps(elem.Machine).Select(x => first.Subtract(DateTime.Parse(x)).TotalMilliseconds);
        Dictionary<double, float> dict = times.Zip(values, (k, v) => new { k, v }).Where(x => x.k >= 0 && x.k <= maxTime).ToDictionary(x => x.k, x => x.v);

        while (dict.Count > maxPoints)
        {
            for (int i = 0; i < dict.Count; i++)
            {
                dict.Remove(dict.ElementAt(i).Key);
            }
        }

        int j = 0;
        foreach (int k in dict.Keys)
        {
            Vector3 R = new Vector3(Mathf.InverseLerp(0, maxTime, k), Mathf.InverseLerp(elem.Min, elem.Max, dict[k]));

            points[j].transform.localPosition = pointStart + Vector3.Scale(R, delta);
            points[j].enabled = true;
            j++;
        }

        for (int i = j; i < maxPoints; i++)
        {
            points[i].enabled = false;
        }

        xText[0].text = first.ToString("HH:mm:ss");
        xText[1].text = first.AddSeconds(last.Subtract(first).TotalSeconds).ToString("HH:mm:ss");
        xText[2].text = last.ToString("HH:mm:ss");

        yText[0].text = elem.Min.ToString("0.00");
        yText[1].text = ((elem.Max + elem.Min) / 2).ToString("0.00");
        yText[2].text = elem.Max.ToString("0.00");
    }
}
