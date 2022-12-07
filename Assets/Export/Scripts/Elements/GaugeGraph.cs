using System.Collections.Generic;
using UnityEngine;

public class GaugeGraph : Graph
{
    [SerializeField] private TMPro.TMP_Text title, units, currentValue;
    [SerializeField, NonReorderable] private List<TMPro.TMP_Text> text;
    [SerializeField, NonReorderable] private List<float> textPos;
    [SerializeField] private Transform needle;

    public override void UpdateValues(Element elem)
    {
        title.text = elem.Title;
        units.text = elem.Units;

        float val = float.Parse(Table.GetLastValue(elem.Machine, elem.Quantity));
        float min = elem.Min, max = elem.Max;
        float r = Mathf.InverseLerp(min, max, val);

        currentValue.text = val.ToString("0.00");

        for (int i = 0; i < text.Count; i++)
        {
            float v = Mathf.Lerp(min, max, textPos[i]);
            text[i].text = v.ToString("0.0");
        }

        needle.localRotation = Quaternion.Euler(0, 0, -180 * r);
    }
}
