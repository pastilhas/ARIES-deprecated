using System.Collections.Generic;
using UnityEngine;

public class AlarmNotification : MonoBehaviour
{
    public static AlarmNotification prefab;
    public static Dictionary<string, AlarmNotification> Notifications = new Dictionary<string, AlarmNotification>();

    public static void SetAlarm(Element elem, bool active)
    {
        string key = elem.Machine ?? "" + elem.Quantity ?? "";
        if (active)
        {
            AlarmNotification notif;
            notif = Notifications.ContainsKey(key) ? Notifications[key] : (Notifications[key] = Instantiate(prefab));
            notif.gameObject.SetActive(true);
            notif.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            notif.transform.rotation = Camera.main.transform.rotation;
        }
        else if (Notifications.ContainsKey(key))
        {
            Notifications[key].gameObject.SetActive(false);
        }
    }
}
