using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveState
{
    private static string LocalFile(ulong id)
    {
        return Path.Combine(Application.persistentDataPath, Convert.ToString(id));
    }

    private static string GlobalFile(ulong id)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Convert.ToString(id));
    }

    [Serializable]
    public struct State
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public int Type;
        public string Machine;
        public string Quantity;
        public string Setpoint;
        public float Min;
        public float Max;
        public float Time;
    }

    public static void SaveStates(IEnumerable<GaugeController> gauges, ulong id)
    {
        App.Log($"Saving {gauges.Count()} elements");
        string[] states = new string[gauges.Count()];

        for (int i = 0; i < states.Length; i++)
        {
            GaugeController g = gauges.ElementAt(i);
            State s = new State()
            {
                Position = g.transform.position,
                Rotation = g.transform.rotation.eulerAngles,
                Scale = g.transform.localScale,
                Type = g.Type,
                Machine = g.Machine,
                Quantity = g.Quantity,
                Setpoint = g.Setpoint,
                Min = g.Min,
                Max = g.Max,
                Time = g.Time
            };

            states[i] = JsonUtility.ToJson(s);
        }

        try
        {
            File.WriteAllLines(LocalFile(id), states);
        }
        catch (Exception ex)
        {
            App.Log($"ERR {ex.Message}");
        }

        try
        {
            File.WriteAllLines(GlobalFile(id), states);
        }
        catch (Exception ex)
        {
            App.Log($"ERR {ex.Message}");
        }
    }

    public static void LoadStates(ulong id)
    {
        App.Log($"Loading id {id}");
        string local = LocalFile(id);
        string global = GlobalFile(id);
        string file = File.Exists(local) ? local : (File.Exists(global) ? global : null);

        if (file != null)
        {
            try
            {
                App.Log($"Loading file {file}");
                string[] states = File.ReadAllLines(file);
                foreach (string s in states)
                {
                    State state = JsonUtility.FromJson<State>(s);
                    GaugeController gc = App.HandMenu.CreateElement(state.Type);
                    gc.transform.SetPositionAndRotation(state.Position, Quaternion.Euler(state.Rotation));
                    gc.transform.localScale = state.Scale;
                    gc.Machine = state.Machine;
                    gc.Quantity = state.Quantity;
                    gc.Setpoint = state.Setpoint;
                    gc.Min = state.Min;
                    gc.Max = state.Max;
                    gc.Time = state.Time;
                }
            }
            catch (Exception ex)
            {
                App.Log($"ERR {ex.Message}");
                Debug.LogException(ex);
            }
        }
        else
        {
            App.Log($"Did not load a file");
        }
    }
}
