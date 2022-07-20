using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimeSeriesGraph : MonoBehaviour, IGraphElement
{
    [SerializeField] private Image pointPrefab;
    [SerializeField] private TMPro.TMP_Text[] xText, yText;
    [SerializeField] private int maxPoints = 5000;
    [SerializeField] private Transform zeroPoint;
    [SerializeField] private Transform canvas;
    [SerializeField] private TMPro.TMP_Text floatingText;

    private Image[] points;
    private Vector3 pointStart;
    private Vector3 delta;

    private float min, max;

    private void Start()
    {
        points = new Image[maxPoints];
        pointStart = zeroPoint.localPosition;

        float xdelta = Vector3.Distance(xText.Last().transform.position, xText[0].transform.position);
        float ydelta = Vector3.Distance(yText.Last().transform.position, yText[0].transform.position);
        delta = new Vector3(xdelta, ydelta);

        for (int i = 0; i < maxPoints; i++)
        {
            Image point = Instantiate(pointPrefab, canvas);
            point.enabled = false;
            points[i] = point;
        }

        GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener(HitPoint);
    }

    private void HitPoint(ManipulationEventData args)
    {
        if (MenuMode.Mode != ViewMode.View)
        {
            return;
        }

        Vector3 target = args.Pointer.Result.Details.Point;
        Transform closest = points.OrderBy(x => Vector3.Distance(target, x.transform.position)).First().transform;
        float result = (closest.localPosition.y - pointStart.y) / delta.y;
        result = Mathf.Lerp(min, max, result);

        floatingText.text = $"{result:0.00}";
        floatingText.transform.parent.localPosition = new Vector3(closest.localPosition.x, closest.localPosition.y + 0.1f * delta.y, floatingText.transform.parent.localPosition.z);
    }

    public void UpdateValues(TableParser table, float value, float min, float max, float time)
    {
        if (time <= 0f || time * 12f > maxPoints)
        {
            time = maxPoints / 12f;
        }

        this.min = min;
        this.max = max;

        DateTime lastDate = DateTime.Parse(table.Timestamps.Last());
        DateTime firstDate = lastDate.AddSeconds(-time * 60f);
        int maxTime = (int)(time * 60f);

        IEnumerable<int> tmp1 = table.Timestamps.Select(x => DateTime.Parse(x)).Select(x => (int)(x.Subtract(firstDate)).TotalSeconds);
        IEnumerable<float> tmp2 = table.Values.Select(x => Mathf.Clamp(float.Parse(x), min, max));
        Dictionary<int, float> dict = tmp1.Zip(tmp2, (k, v) => new { k, v }).Where(x => x.k >= 0 && x.k <= maxTime).ToDictionary(x => x.k, x => x.v);

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
            Vector3 R = new Vector3(Mathf.InverseLerp(0, maxTime, k), Mathf.InverseLerp(min, max, dict[k]));

            points[j].transform.localPosition = pointStart + Vector3.Scale(R, delta);
            points[j].enabled = true;
            j++;
        }

        for (int i = j; i < maxPoints; i++)
        {
            points[i].enabled = false;
        }

        xText[0].text = firstDate.ToString("HH:mm:ss");
        xText[1].text = firstDate.AddSeconds(lastDate.Subtract(firstDate).TotalSeconds).ToString("HH:mm:ss");
        xText[2].text = lastDate.ToString("HH:mm:ss");

        yText[0].text = min.ToString("0.00");
        yText[1].text = ((max + min) / 2).ToString("0.00");
        yText[2].text = max.ToString("0.00");
    }
}
