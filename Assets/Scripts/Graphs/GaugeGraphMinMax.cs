using UnityEngine;

public class GaugeGraphMinMax : MonoBehaviour, IGraphElement
{
    [Header("Gauge text")]
    [SerializeField] private TMPro.TMP_Text currentValue;
    [SerializeField] private TMPro.TMP_Text min3Value;
    [SerializeField] private TMPro.TMP_Text min2Value;
    [SerializeField] private TMPro.TMP_Text minValue;
    [SerializeField] private TMPro.TMP_Text maxValue;
    [SerializeField] private TMPro.TMP_Text max2Value;
    [SerializeField] private TMPro.TMP_Text max3Value;

    [Header("Gauge Pointers")]
    [SerializeField] private Transform needle;

    public void UpdateValues(TableParser table, float value, float min, float max, float time)
    {
        float curr = value;
        float min2 = min - (max - min) / 4;
        float min3 = min - (max - min) / 2;
        float max2 = max + (max - min) / 4;
        float max3 = max + (max - min) / 2;

        float r = Mathf.InverseLerp(min3, max3, curr);

        currentValue.text = $"{curr:0.00}";
        min3Value.text = $"{min3}";
        min2Value.text = $"{min2}";
        minValue.text = $"{min}";
        maxValue.text = $"{max}";
        max2Value.text = $"{max2}";
        max3Value.text = $"{max3}";

        needle.localRotation = Quaternion.Euler(0, 0, -180 * r);
    }
}
