using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] private GaugeController[] gaugeTypes;
    [SerializeField] private GameObject handMenu, createMenu, debugWindow;
    [SerializeField] private Transform greenCube;

    public void SetMode(int m)
    {
        ViewMode mode = (ViewMode)m;
        if (mode != ViewMode.Editor)
        {
            App.EditorMenu.CloseEditorMenu();
        }

        MenuMode.Mode = mode;
        App.SetGaugeConstraints();
        greenCube.localPosition = new Vector3(0.016f + 0.032f * (m % 2), -0.016f - 0.032f * (m / 2));
    }

    public void OpenDebug()
    {
        debugWindow.SetActive(!debugWindow.activeInHierarchy);
    }

    public GaugeController CreateElement(int i)
    {
        GaugeController gc = Instantiate(gaugeTypes[i]);
        gc.Type = i;
        App.AddGauge(gc);
        return gc;
    }

    public void SpawnElement(int i)
    {
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward;
        Vector3 forward = Vector3.Scale(pos - Camera.main.transform.position, new Vector3(1, 0, 1));
        GaugeController gc = Instantiate(gaugeTypes[i], pos, Quaternion.identity);
        gc.transform.rotation *= Quaternion.FromToRotation(gc.transform.forward, forward);
        gc.Type = i;
        App.AddGauge(gc);
        createMenu.SetActive(false);
        handMenu.SetActive(true);
    }
}

public enum ViewMode { View, Editor, Delete, Move }

public static class MenuMode
{
    public static ViewMode Mode { get; set; }
}