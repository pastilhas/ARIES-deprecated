using UnityEngine;

public class GaugeGraph : MonoBehaviour, IGraphElement
{
    [Header("Gauge text")]
    [SerializeField] private TMPro.TMP_Text currentValue;
    [SerializeField] private TMPro.TMP_Text minValue;
    [SerializeField] private TMPro.TMP_Text maxValue;
    [SerializeField] private TMPro.TMP_Text max2Value;
    [SerializeField] private TMPro.TMP_Text max3Value;

    [Header("Gauge Pointers")]
    [SerializeField] private Transform needle;

    public void UpdateValues(TableParser table, float value, float min, float max, float time)
    {
        float curr = value;
        float max2 = max + (max - min) / 2;
        float max3 = max + (max - min);

        float r = Mathf.InverseLerp(min, max3, curr);

        currentValue.text = $"{curr:0.00}";
        minValue.text = $"{min}";
        maxValue.text = $"{max}";
        max2Value.text = $"{max2}";
        max3Value.text = $"{max3}";

        needle.localRotation = Quaternion.Euler(0, 0, -180 * r);
    }
}
