using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class EditorMenu : MonoBehaviour
{
    public static EditorMenu instance;
    [SerializeField] private EditorSubMenu main;
    [SerializeField, NonReorderable] private List<MenuButton> mainButtons;
    [SerializeField, NonReorderable] private List<EditorSubMenu> menus;
    [SerializeField] private TMPro.TMP_Text minTxt, maxTxt, timeTxt;
    private int activeMenu;
    private string title, units, machine, quantity, setpoint;
    private float min, max, time;
    private Element elem;

    private void Start()
    {
        instance = this;
        CloseEdit_();
    }

    public static void OpenEdit(Element elem)
    {
        instance.OpenEdit_(elem);
    }

    public static void CloseEdit()
    {
        instance.CloseEdit_();
    }

    public static void SetMachines()
    {
        instance.menus[(int)EditorSubMenuType.Machine].SetButtons(Table.Tables.Keys);
    }

    public static void PressButton(string name, MenuButtonType type)
    {
        instance.PressButton_(name, type);
    }

    private void OpenEdit_(Element elem)
    {
        CloseEdit_();

        this.elem = elem;
        title = elem.Title;
        units = elem.Units;
        machine = elem.Machine;
        quantity = elem.Quantity;
        setpoint = elem.Setpoint;
        min = elem.Min;
        max = elem.Max;
        time = elem.Time;

        bool[] has = new bool[] { elem.hasUnits, elem.hasMachine, elem.hasQuantity, elem.hasSetpoint, elem.hasLimits, elem.hasTimelimit };
        for (int i = 0; i < has.Length; i++)
        {
            mainButtons[i].gameObject.SetActive(has[i]);
        }
        main.GetComponentInChildren<GridObjectCollection>().UpdateCollection();

        if (!Table.TableExists(elem.Machine))
        {
            activeMenu = (int)EditorSubMenuType.Machine;
            menus[activeMenu].gameObject.SetActive(true);
        }
        else
        {
            activeMenu = -1;
            main.gameObject.SetActive(true);
        }

        transform.position = Camera.main.transform.position + 0.5f * Camera.main.transform.forward;
        transform.rotation = Quaternion.Euler(Vector3.Scale(new Vector3(0, 1, 0), Camera.main.transform.rotation.eulerAngles));
    }

    private void CloseEdit_()
    {
        menus.ForEach(menu => menu.gameObject.SetActive(false));
        main.gameObject.SetActive(false);

        if (elem == null)
        {
            return;
        }

        elem.Title = title;
        elem.Units = units;
        elem.Machine = machine;
        elem.Quantity = quantity;
        elem.Setpoint = setpoint;
        elem.Min = min;
        elem.Max = max;
        elem.Time = time;
    }

    private void PressButton_(string name, MenuButtonType type)
    {
        if (activeMenu == -1)
        {
            OpenSubMenu(type);
            return;
        }

        if (type == MenuButtonType.Units)
        {
            units = name;
        }
        else if (type == MenuButtonType.Machine)
        {
            machine = name;
        }
        else if (type == MenuButtonType.Quantity)
        {
            quantity = name;
        }
        else if (type == MenuButtonType.Setpoint)
        {
            setpoint = name;
        }
        else if (type == MenuButtonType.Min)
        {
            min += float.Parse(name);
        }
        else if (type == MenuButtonType.Max)
        {
            max += float.Parse(name);
        }
        else if (type == MenuButtonType.Time)
        {
            time += float.Parse(name);
        }

        title = $"{machine}:{quantity}";
    }

    private void OpenSubMenu(MenuButtonType type)
    {
        main.gameObject.SetActive(false);
        activeMenu = (int)type;

        if ((EditorSubMenuType)activeMenu == EditorSubMenuType.Quantity || (EditorSubMenuType)activeMenu == EditorSubMenuType.Setpoint)
        {
            menus[activeMenu].SetButtons(Table.GetColumnNames(machine));
        }
        else if ((EditorSubMenuType)activeMenu == EditorSubMenuType.Limits)
        {
            minTxt.text = min.ToString();
            maxTxt.text = max.ToString();
        }
        else if ((EditorSubMenuType)activeMenu == EditorSubMenuType.Time)
        {
            timeTxt.text = time.ToString();
        }

        menus[activeMenu].gameObject.SetActive(true);
    }
}

public enum EditorSubMenuType { Units, Machine, Quantity, Setpoint, Limits, Time, }
public enum MenuButtonType { Units, Machine, Quantity, Setpoint, Min, Max, Time, }
