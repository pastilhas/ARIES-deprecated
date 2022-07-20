using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class EditorMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private EditorSubMenu title, units, machines, quantities, setpoints, limits, timelimit;
    [SerializeField] private TMPro.TMP_Text minValue, maxValue, timeValue;

    private EditorSubMenu[] menus;
    private EditorSubMenu activeMenu;
    private MenuButton[] mainButtons;

    private GaugeController gauge;
    public string Title { get => tmpTitle; set => tmpTitle = value; }
    public string Units { get => tmpUnits; set => tmpUnits = value; }
    private string tmpTitle, tmpUnits, tmpMachine, tmpQuantity, tmpSetpoint;
    private float tmpMin, tmpMax, tmpTime;

    public void SetupMenus()
    {
        machines.SetupMachines();
        menus = new EditorSubMenu[] { title, units, machines, quantities, setpoints, limits, timelimit };
        mainButtons = mainMenu.GetComponentInChildren<GridObjectCollection>().GetComponentsInChildren<MenuButton>(true);
    }

    public void OpenMenu(GaugeController gauge)
    {
        if (isActiveAndEnabled)
        {
            CloseEditorMenu();
        }

        this.gauge = gauge;
        tmpTitle = gauge.Title;
        tmpUnits = gauge.Units;
        tmpMachine = gauge.Machine;
        tmpQuantity = gauge.Quantity;
        tmpSetpoint = gauge.Setpoint;
        tmpMin = gauge.Min;
        tmpMax = gauge.Max;
        tmpTime = gauge.Time;

        foreach (EditorSubMenu menu in menus)
        {
            menu.SetActive(false);
        }

        foreach (MenuButton button in mainButtons)
        {
            button.SetActive(
                button.name.Equals("units") && gauge.HasUnits ||
                button.name.Equals("machine") && gauge.HasMachine ||
                button.name.Equals("quantity") && gauge.HasQuantity ||
                button.name.Equals("setpoint") && gauge.HasSetPoint ||
                button.name.Equals("limits") && gauge.HasLimits ||
                button.name.Equals("timelimit") && gauge.HasTimeLimit
                );
        }
        mainMenu.GetComponentInChildren<GridObjectCollection>().UpdateCollection();

        bool first = tmpMachine == null && gauge.HasMachine;
        machines.SetActive(first);
        mainMenu.SetActive(!first);
        activeMenu = first ? machines : null;
        transform.rotation *= Quaternion.FromToRotation(transform.forward, Camera.main.transform.position - transform.position);
        gameObject.SetActive(true);
    }

    public void OpenSubMenu(int t)
    {
        EditorSubMenu next = menus[t - 1];
        MenuType type = next.Type;
        if (type == MenuType.Title)
        {

        }
        else if (type == MenuType.Units)
        {

        }
        else if (type == MenuType.Machine)
        {

        }
        else if (type == MenuType.Quantity && gauge.HasQuantity && tmpMachine != null)
        {
            next.SetupQuantities(tmpMachine);
        }
        else if (type == MenuType.SetPoint && gauge.HasSetPoint && tmpMachine != null)
        {
            next.SetupQuantities(tmpMachine);
        }
        else if (type == MenuType.Limits)
        {
            minValue.text = tmpMin.ToString();
            maxValue.text = tmpMax.ToString();
        }
        else if (type == MenuType.TimeLimit)
        {
            timeValue.text = tmpTime.ToString();
        }
        else
        {
            return;
        }
        activeMenu = next;
        mainMenu.SetActive(false);
        next.SetActive(true);
    }

    public void CloseEditorMenu()
    {
        App.Save();

        if (gauge != null)
        {
            //gauge.Title = tmpTitle;
            gauge.Title = $"{tmpMachine}:{tmpQuantity}";
            gauge.Units = tmpUnits;
            gauge.Machine = tmpMachine;
            gauge.Quantity = tmpQuantity;
            gauge.Setpoint = tmpSetpoint;
            (gauge.Min, gauge.Max) = tmpMin < tmpMax ? (tmpMin, tmpMax) : (tmpMax, tmpMin);
            gauge.Time = tmpTime;
        }
        gameObject.SetActive(false);
    }

    public void PressButton(ButtonType type, string name, bool next)
    {
        switch (type)
        {
            case ButtonType.Units:
                tmpUnits = name;
                break;
            case ButtonType.Machine:
                tmpMachine = name;
                break;
            case ButtonType.Quantity:
                tmpQuantity = name;
                break;
            case ButtonType.SetPoint:
                tmpSetpoint = name;
                break;
            case ButtonType.MinValueAdd:
                tmpMin += float.Parse(name);
                minValue.text = tmpMin.ToString();
                break;
            case ButtonType.MaxValueAdd:
                tmpMax += float.Parse(name);
                maxValue.text = tmpMax.ToString();
                break;
            case ButtonType.TimeLimitAdd:
                tmpTime += float.Parse(name);
                timeValue.text = tmpTime.ToString();
                break;
            case ButtonType.Close:
                CloseEditorMenu();
                return;
        }

        if (next)
        {
            activeMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
    }
}

public enum MenuType { None, Title, Units, Machine, Quantity, SetPoint, Limits, TimeLimit }
public enum ButtonType { None, Units, Machine, Quantity, SetPoint, MinValueAdd, MaxValueAdd, TimeLimitAdd, Next, Close }
