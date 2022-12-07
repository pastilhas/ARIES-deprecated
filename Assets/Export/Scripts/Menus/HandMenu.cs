using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] private Transform activeView;

    private void Start()
    {
        GetComponent<HandConstraintPalmUp>().OnFirstHandDetected.AddListener(() => Element.Elements.ForEach(x => x.deleteConfirm.SetActive(false)));
    }

    public void SetView(int view)
    {
        ViewMode.Mode = (ViewModeType)view;
        Element.Elements.ForEach(e => e.SetElementRestrictions());
        float x = (view / 2 == 0 ? -0.017f : 0.017f);
        float y = (view % 2 == 0 ? 0.034f : 0f);
        activeView.localPosition = new Vector3(x, y);
    }

    public void CreateElement(int elem)
    {
        Element.CreateElement(elem);
    }
}
