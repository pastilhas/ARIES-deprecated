using UnityEngine;
using UnityEngine.UI;

public class IconBoard : MonoBehaviour, IGraphElement
{
    [SerializeField] private Image onoffIcon, alarmIcon;
    [SerializeField] private Color onColour, offColour, alarmColour;

    public void UpdateValues(TableParser table, float value, float min, float max, float time)
    {
        onoffIcon.color = table.IsOnline ? onColour : offColour;
        alarmIcon.color = App.Clusters[GetComponent<GaugeController>().Machine].Alarm > 0 ? alarmColour : Color.white;
    }
}
