using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    [SerializeField] public ButtonType buttonType;
    [SerializeField] public bool next;

    private void Start()
    {
        if (buttonType != ButtonType.None)
        {
            GetComponent<ButtonConfigHelper>().OnClick.AddListener(PressButton);
        }
    }

    public void SetButton(bool active, string name = "", string label = "")
    {
        gameObject.SetActive(active);
        this.name = name;
        GetComponent<ButtonConfigHelper>().MainLabelText = label;
    }

    private void PressButton()
    {
        App.EditorMenu.PressButton(buttonType, name, next);
    }

    public void SetActive(bool i)
    {
        gameObject.SetActive(i);
    }
}
