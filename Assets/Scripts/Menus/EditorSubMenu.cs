using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class EditorSubMenu : MonoBehaviour
{
    [SerializeField] private MenuType type;
    private MenuButton[] buttons;

    public MenuType Type => type;

    public void SetupMachines()
    {
        List<MenuButton> list = new List<MenuButton>();
        foreach (MenuButton button in GetComponentsInChildren<MenuButton>())
        {
            if (button.buttonType != ButtonType.Next && button.buttonType != ButtonType.Close && button.buttonType != ButtonType.None)
            {
                list.Add(button);
            }
        }
        buttons = list.ToArray();

        string[] keys = new List<string>(App.Tables.Keys).ToArray();
        SetButtons(keys);
    }

    public void SetupQuantities(string name)
    {
        List<MenuButton> list = new List<MenuButton>();
        foreach (MenuButton button in GetComponentsInChildren<MenuButton>())
        {
            if (button.buttonType != ButtonType.Next && button.buttonType != ButtonType.Close && button.buttonType != ButtonType.None)
            {
                list.Add(button);
            }
        }
        buttons = list.ToArray();

        List<string> names = new List<string>();
        foreach (string n in App.Tables[name].ColumnNames)
        {
            if (!n.StartsWith("ID") && !n.Equals("TIMESTAMP"))
            {
                names.Add(n);
            }
        }

        SetButtons(names.ToArray());
    }

    public void SetButtons(string[] names, string[] labels = null)
    {
        labels ??= names;
        int min = Mathf.Min(names.Length, buttons.Length);

        // Set 1 button per column
        for (int i = 0; i < min; i++)
        {
            buttons[i].SetButton(true, names[i], labels[i]);
        }

        // Any remaining buttons
        for (int i = min; i < buttons.Length; i++)
        {
            buttons[i].SetButton(false);
        }

        GetComponentInChildren<GridObjectCollection>().UpdateCollection();
    }

    public void SetActive(bool i)
    {
        gameObject.SetActive(i);
    }
}
