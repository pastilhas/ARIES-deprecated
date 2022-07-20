using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlarmDialog : MonoBehaviour
{
    private string key;
    public string Key => key;
    [SerializeField] private TMPro.TMP_Text machineText, quantityText, valueText, limitsText;
    public static readonly List<AlarmDialog> AlarmsList = new List<AlarmDialog>();

    private void OnDisable()
    {
        if (App.Instance != null)
        {
            App.Instance.StartCoroutine(Reenable(gameObject));
        }
    }

    private IEnumerator Reenable(GameObject go)
    {
        yield return new WaitForSeconds(3f);
        go.SetActive(true);
    }

    public static void SetAlarm(string machine, string quantity, string units, float value, float min, float max)
    {
        string key = machine + quantity;
        AlarmDialog[] list = AlarmsList.Where(x => x.key == key).ToArray();


        if (min <= value && value <= max || !App.Tables[machine].Parsers[quantity].IsOnline)
        {
            AlarmsList.RemoveAll(x => x.key == key);
            foreach (AlarmDialog alarm in list)
            {
                Destroy(alarm);
            }
        }
        else
        {
            if (list.Length == 0)
            {
                AlarmDialog alarm = Instantiate(App.AlarmDialog);
                alarm.SetAlarmParams(machine, quantity, units, value, min, max);
                AlarmsList.Add(alarm);
            }

            foreach (AlarmDialog alarm in list)
            {
                alarm.SetAlarmParams(machine, quantity, units, value, min, max);
            }
        }
    }

    private AlarmDialog SetAlarmParams(string machine, string quantity, string units, float value, float min, float max)
    {
        key = machine + quantity;
        machineText.text = machine;
        quantityText.text = quantity;
        valueText.text = $"{value:0.00} {units}";
        limitsText.text = $"{min:0.00} to {max:0.00}";
        return this;
    }
}
