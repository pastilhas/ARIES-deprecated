using System.Collections.Generic;
using UnityEngine;

public class EditorSubMenu : MonoBehaviour
{
    public EditorSubMenuType Type;

    public void SetButtons(IEnumerable<string> keys)
    {
        MenuButton[] buttons = GetComponentsInChildren<MenuButton>(true);
        int i = 0;
        foreach (string key in keys)
        {
            buttons[i++].SetButton(key);
        }

        while (i < buttons.Length)
        {
            buttons[i++].gameObject.SetActive(false);
        }
    }
}
