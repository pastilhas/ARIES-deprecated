using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GaugeController : MonoBehaviour
{
    [SerializeField] public GameObject DeathBall;
    [SerializeField] private TMPro.TMP_Text title, units;
    [SerializeField] private float updateTime;
    [SerializeField] private bool hasUnits, hasMachine, hasQuantity, hasSetPoint, hasLimits, hasTimeLimit;

    private System.DateTime? nextUpdate = null;

    // Values
    public int Type { get; set; }
    public string Title { get => title.text; set => title.text = value; }
    public string Units { get => units.text; set => units.text = value; }
    public string Machine { get; set; }
    public string Quantity { get; set; }
    public string Setpoint { get; set; }
    public float Value { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public float Time { get; set; }
    public float SetpointValue { get; set; }

    // Menu Access
    public bool HasUnits => hasUnits;
    public bool HasMachine => hasMachine;
    public bool HasQuantity => hasQuantity;
    public bool HasSetPoint => hasSetPoint;
    public bool HasLimits => hasLimits;
    public bool HasTimeLimit => hasTimeLimit;

    // Graph
    public IGraphElement Graph { get; private set; }

    private void Start()
    {
        SetGaugeConstraints();
        GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(Interact);
        Graph = GetComponent<IGraphElement>();
        StartCoroutine(UpdateValues());
    }

    private void Update()
    {
        if (nextUpdate.HasValue && System.DateTime.UtcNow > nextUpdate.Value)
        {
            App.Log($"{name} frozen at {nextUpdate.Value:HH:mm:ss}");

            GaugeController gc = App.HandMenu.CreateElement(Type);
            gc.transform.SetPositionAndRotation(transform.position, transform.rotation);
            gc.transform.localScale = transform.localScale;
            gc.Title = Title;
            gc.Units = Units;
            gc.Machine = Machine;
            gc.Quantity = Quantity;
            gc.Setpoint = Setpoint;
            gc.Min = Min;
            gc.Max = Max;
            gc.Time = Time;

            App.RemGauge(this);
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator UpdateValues()
    {
        while (true)
        {
            yield return new WaitUntil(() => App.GetParser(Machine, Quantity, hasQuantity) != null);
            nextUpdate = System.DateTime.UtcNow.AddSeconds(2 * updateTime);

            TableParser stp = App.GetParser(Machine, Setpoint, true);
            if (stp != null)
            {
                Max = float.Parse(stp.LastValue);
            }

            TableParser tp = App.GetParser(Machine, Quantity, hasQuantity);
            Value = float.Parse(tp.LastValue);
            Graph.UpdateValues(tp, Value, Min, Max, Time);

            App.ActivateAlarm(this);

            yield return new WaitForSeconds(updateTime);
        }
    }

    private void Interact(ManipulationEventData args)
    {
        if (MenuMode.Mode == ViewMode.Delete)
        {
            if (DeathBall == null || DeathBall.activeInHierarchy)
            {
                App.RemGauge(this);
                Destroy(gameObject);
            }
            else
            {
                DeathBall.SetActive(true);
            }
        }

        if (MenuMode.Mode == ViewMode.Editor)
        {
            App.OpenEditorMenu(this);
        }
    }

    public void SetGaugeConstraints()
    {
        // Only move when in move
        GetComponent<MoveAxisConstraint>().enabled = MenuMode.Mode != ViewMode.Move;
        GetComponent<MinMaxScaleConstraint>().enabled = MenuMode.Mode != ViewMode.Move;
        GetComponents<RotationAxisConstraint>().First(x => x.ExecutionPriority == 1).enabled = MenuMode.Mode != ViewMode.Move;
    }

    public void CreateCopy()
    {
        (int typ, Vector3 pos, Vector3 rot, Vector3 sca, string mac, string qua, string set, float min, float max, float tim) = (Type, transform.position, transform.eulerAngles, transform.localScale, Machine, Quantity, Setpoint, Min, Max, Time);

        GaugeController gc = App.HandMenu.CreateElement(typ);
        gc.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
        gc.transform.localScale = sca;
        gc.Machine = mac;
        gc.Quantity = qua;
        gc.Setpoint = set;
        gc.Min = min;
        gc.Max = max;
        gc.Time = tim;

        App.RemGauge(this);
        Destroy(gameObject);
    }
}

public interface IGraphElement
{
    public void UpdateValues(TableParser table, float value, float min, float max, float time);
}