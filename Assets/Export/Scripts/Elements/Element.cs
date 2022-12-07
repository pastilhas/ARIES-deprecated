using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectManipulator))]
public class Element : MonoBehaviour
{
    public static List<Element> prefabs;
    public static int MAX_DIST = 10;
    public static float MAX_FREEZE_TIME = 10f;

    public static readonly List<Element> Elements = new List<Element>();

    private float lastUpdate = float.MaxValue;
    private Graph graph;
    [HideInInspector] public string Title, Units, Machine, Quantity, Setpoint;
    [HideInInspector] public float Min, Max, Time;
    public bool hasUnits, hasMachine, hasQuantity, hasSetpoint, hasLimits, hasTimelimit;

    [SerializeField, Range(0f, 10f)] private float updateDelay;
    [SerializeField, NonReorderable] private List<TransformConstraint> constraints;
    [SerializeField] public GameObject deleteConfirm;

    private void Start()
    {
        SetElementRestrictions();
        graph = GetComponent<Graph>();
        StartCoroutine(UpdateElement());
    }

    private void Update()
    {
        if (UnityEngine.Time.time > lastUpdate + MAX_FREEZE_TIME)
        {
            App.Log($"Element {name} froze");
            CreateElement(this);
        }
    }

    private IEnumerator UpdateElement()
    {
        while (true)
        {
            yield return new WaitUntil(() => IsValid());
            lastUpdate = UnityEngine.Time.time;
            string set;
            if (hasSetpoint && (set = Table.GetLastValue(Machine, Setpoint)) != null)
            {
                Max = float.Parse(set);
            }
            graph.UpdateValues(this);
            if (hasMachine && hasQuantity && hasLimits)
            {
                float last = float.Parse(Table.GetLastValue(Machine, Quantity));
                AlarmNotification.SetAlarm(this, (last < Min || last > Max) && Table.IsOnline(Machine));
            }
            yield return new WaitForSeconds(updateDelay);
        }
    }

    private bool IsValid()
    {
        bool machineValid = hasMachine && Table.TableExists(Machine) || !hasMachine;
        bool quantityValid = hasQuantity && Table.ColumnExists(Machine, Quantity) || !hasQuantity;
        bool setpointValid = hasSetpoint && Table.ColumnExists(Machine, Setpoint) || !hasSetpoint;
        return machineValid && quantityValid && setpointValid;
    }

    public void Interact(ManipulationEventData arg)
    {
        App.Log("Interact");
        if (ViewMode.Mode == ViewModeType.Edit)
        {
            EditorMenu.OpenEdit(this);
        }
        else if (ViewMode.Mode == ViewModeType.Delete)
        {
            if (deleteConfirm == null || deleteConfirm.activeInHierarchy)
            {
                RemoveElement(this);
            }
            else
            {
                deleteConfirm.SetActive(true);
            }
        }
    }

    public void SetElementRestrictions()
    {
        bool isMove = ViewMode.Mode == ViewModeType.Move;
        constraints.ForEach(x => x.enabled = !isMove);
    }

    public static Element CreateElement(Element original)
    {
        Element copy = Instantiate(original);
        RemoveElement(original);
        Elements.Add(copy);
        return copy;
    }

    public static Element CreateElement(int type)
    {
        Element elem = Instantiate(prefabs[type], App.WorldRootAnchor);

        elem.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward,
            Quaternion.Euler(Vector3.Scale(new Vector3(0, 1, 1), Camera.main.transform.rotation.eulerAngles)));

        Elements.Add(elem);
        return elem;
    }

    public static void RemoveElement(Element elem)
    {
        Elements.Remove(elem);
        Destroy(elem);
    }
}
