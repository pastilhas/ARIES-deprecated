using UnityEngine;

public class HandMenuContent : MonoBehaviour
{
    [SerializeField] private GameObject handMenu, createMenu;
    private void OnEnable()
    {
        if (MenuMode.Mode == ViewMode.Delete)
        {
            foreach (GaugeController g in App.Gauges)
            {
                if (g.DeathBall != null)
                {
                    g.DeathBall.SetActive(false);
                }
            }
        }
        createMenu.SetActive(false);
        handMenu.SetActive(true);
    }
}
