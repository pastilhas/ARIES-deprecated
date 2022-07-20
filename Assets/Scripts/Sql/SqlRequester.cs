using System.Collections;
using UnityEngine.Networking;

public static class SqlRequester
{
    public static IEnumerator Request(
        string address,
        string database,
        string table,
        int nEntries,
        System.Action<string, string> callback
    )
    {
        string query = $"{address.TrimEnd('/')}/SELECT+*+FROM+{database}.{table}+ORDER+BY+id{table}+DESC+LIMIT+{nEntries};";
        UnityWebRequest www = UnityWebRequest.Get(query);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            callback(table, www.downloadHandler.text);
        }
        else
        {
            App.Log(www.error);
        }
    }
}
