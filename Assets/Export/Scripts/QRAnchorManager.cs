using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.QR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QRAnchorManager : MonoBehaviour
{
    [SerializeField] private QRCodeContainer qrPrefab;

    private Transform anchor;
    private readonly List<string> qrCache = new List<string>();
    private readonly Dictionary<string, KnownCode> knownCodes = new Dictionary<string, KnownCode>();
    private readonly List<(string, Guid)> foundCodes = new List<(string, Guid)>();

    private async void Start()
    {
        if (!QRCodeWatcher.IsSupported())
        {
            App.Log("QR codes are not supported");
            return;
        }

        QRCodeWatcher qrWatcher;
        try
        {
            QRCodeWatcherAccessStatus status = await QRCodeWatcher.RequestAccessAsync();
            if (status != QRCodeWatcherAccessStatus.Allowed)
            {
                App.Log($"Cannot access QR Code Watcher : {status}");
                return;
            }

            qrWatcher = new QRCodeWatcher();
            qrWatcher.Added += QRCodeAdded;
            qrWatcher.Updated += QRCodeUpded;
            qrWatcher.Start();
            anchor = Instantiate(App.AnchorPrefab, Vector3.zero, Quaternion.identity);
            anchor.name = Guid.NewGuid().ToString();
            App.WorldRootAnchor.SetParent(anchor);
            App.Log("QR Watcher Started");
        }
        catch (Exception e)
        {
            App.Log("ERR - " + e.Message);
        }

        // StartCoroutine(Request());

        /*IEnumerator Request()
        {
            UnityWebRequest req = UnityWebRequest.Get("http://192.168.190.100:1882");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                App.Log("Failed to get QR Codes from server");
                yield break;
            }
            
            try
            {
                App.Log("Starting QR Code Watcher");
                SaveCode(req.downloadHandler.text);
                qrWatcher.Start();
                anchor = Instantiate(App.AnchorPrefab, Vector3.zero, Quaternion.identity);
                anchor.name = Guid.NewGuid().ToString();
                App.WorldRootAnchor.SetParent(anchor);
            }
            catch (Exception ex)
            {
                App.Log("QR code failled start");
                App.Log(ex.Message);
            }
        }*/
    }

    private void Update()
    {
        foreach ((string, Guid) code in foundCodes)
        {
            AddCode(code.Item1, code.Item2);
        }
    }

    private void QRCodeAdded(object sender, QRCodeAddedEventArgs e)
    {
        string code = e.Code.Data;
        App.Log(code);
        if (qrCache.Contains(code))
        {
            return;
        }
        qrCache.Add(code);
        App.Log("Added code " + code);
        Guid id = e.Code.SpatialGraphNodeId;
        foundCodes.Add((code, id));
    }

    private void QRCodeUpded(object sender, QRCodeUpdatedEventArgs e)
    {
        App.Log(".");
        string code = e.Code.Data;
        if (qrCache.Contains(code))
        {
            return;
        }
        qrCache.Add(code);
        App.Log("Added code " + code);
        Guid id = e.Code.SpatialGraphNodeId;
        foundCodes.Add((code, id));
    }

    private void AddCode(string code, Guid nodeId)
    {
        if (!code.StartsWith(App.QRCodeStart))
        {
            App.Log($"Incorrect format {code}");
            return;
        }

        QRCodeContainer container;

        try
        {
            container = Instantiate(qrPrefab, Vector3.zero, Quaternion.identity);
            container.name = code;
            container.node = SpatialGraphNode.FromStaticNodeId(nodeId);

            if (container.node.TryLocate(FrameTime.OnUpdate, out Pose pose))
            {
                transform.SetPositionAndRotation(pose.position, pose.rotation);
            }
        }
        catch (Exception e)
        {
            App.Log("ERR - Failed creating container");
            App.Log(e.Message);
            return;
        }

        try
        {
            if (knownCodes.ContainsKey(code))
            {
                KnownCode oldCode = knownCodes[code];
                Transform tmp = anchor;
                anchor = Instantiate(App.AnchorPrefab, container.transform.position - oldCode.pos, Quaternion.identity);
                anchor.name = oldCode.anchorId;
                App.WorldRootAnchor.SetParent(anchor);
                Vector3 fromTo = anchor.position - tmp.position;
                Destroy(tmp);

                foreach (KeyValuePair<string, KnownCode> oc in knownCodes)
                {
                    knownCodes[oc.Key] = new KnownCode(oc.Key, oc.Value.pos + fromTo, anchor.name);
                }

            }
            else
            {
                KnownCode newCode = new KnownCode(code, container.transform.position, anchor.name);
                knownCodes[code] = newCode;
            }
            // SendCodes(knownCodes.Values.ToArray());
        }
        catch (Exception e)
        {
            App.Log("ERR - Failed adding container");
            App.Log(e.Message);
            return;
        }
    }

    private void SaveCode(string text)
    {
        JObject[] _knownCodes = JArray.Parse(text).ToObject<JObject[]>();
        foreach (JObject code in _knownCodes)
        {
            string cid = code["codeId"].Value<string>();
            string aid = code["anchorId"].Value<string>();
            float x = code["x"].Value<float>();
            float y = code["y"].Value<float>();
            float z = code["z"].Value<float>();
            knownCodes[cid] = new KnownCode(cid, new Vector3(x, y, z), aid);
        }
    }

    private void SendCodes(KnownCode[] codes)
    {
        string json = JsonUtility.ToJson(codes);
        StartCoroutine(Request());

        IEnumerator Request()
        {
            UnityWebRequest req = UnityWebRequest.Post("http://192.168.190.100:1882", json);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            App.Log($"{req.result} status from sending codes");
        }
    }

    private class KnownCode
    {
        public readonly string codeId;
        public readonly Vector3 pos;
        public readonly string anchorId;
        public KnownCode(string i, Vector3 p, string a)
        {
            codeId = i; pos = p; anchorId = a;
        }
    }
}
