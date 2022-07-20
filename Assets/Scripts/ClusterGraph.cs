using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ClusterGraph : MonoBehaviour
{
    [SerializeField] private Color on, off, alarm;
    [SerializeField] private GameObject cube;
    public string Machine { get; set; }
    public int Alarm => AlarmDialog.AlarmsList.Count(x => x.Key.StartsWith(Machine));
    private bool isOnline;

    private void Update()
    {
        isOnline = App.GetParser(Machine).IsOnline;
        GaugeController[] elements = GetElements();

        if (elements.Length == 0)
        {
            cube.SetActive(false);
            return;
        }

        UpdateCoordinates(elements);

        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        cube.SetActive(Alarm > 0 || dist > App.MaxMachineDistance);
        App.SetActiveMachine(elements, dist < App.MaxMachineDistance);
        cube.GetComponent<Renderer>().material.color = Alarm > 0 ? alarm : (isOnline ? on : off);

        GaugeController[] GetElements()
        {
            List<GaugeController> elements = new List<GaugeController>();
            foreach (GaugeController c in App.Gauges)
            {
                if (c.Machine == Machine)
                {
                    elements.Add(c);
                }
            }
            return elements.ToArray();
        }

        void UpdateCoordinates(GaugeController[] gauges)
        {
            Vector3 pos = Vector3.zero;
            foreach (GaugeController gc in gauges)
            {
                pos += gc.transform.position;
            }
            transform.position = pos / gauges.Length;
        }
    }
}
