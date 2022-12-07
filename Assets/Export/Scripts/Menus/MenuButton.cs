using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public MenuButtonType type;

    public void SetButton(string key)
    {
        gameObject.SetActive(true);
        name = key;
        GetComponent<ButtonConfigHelper>().MainLabelText = key;
        GetComponent<ButtonConfigHelper>().OnClick.AddListener(PressButton);
    }

    private void PressButton()
    {
        EditorMenu.PressButton(name, type);
    }
}
