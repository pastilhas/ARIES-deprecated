using UnityEngine;
using UnityEngine.UI;

public class StatusboardGraph : Graph
{
    [SerializeField] private TMPro.TMP_Text title;
    [SerializeField] private SpriteRenderer onoff, alarm;

    public override void UpdateValues(Element elem)
    {
        title.text = elem.Machine;

        onoff.color = Table.IsOnline(elem.Machine) ? Color.green : Color.red;
        alarm.color = Table.IsAlarm(elem.Machine) ? Color.red : Color.white;
    }
}
