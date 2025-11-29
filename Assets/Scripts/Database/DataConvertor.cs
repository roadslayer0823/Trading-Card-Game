using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DataConvertor : MonoBehaviour
{
    private string googleSheetUrl = "https://docs.google.com/spreadsheets/d/1f38pAH0_fSdSwnX_1mqyIHhz9-qbQocQq79RPTb3zhs/export?format=csv&gid=0";

    public void StartDownloadCardData()
    {
        StartCoroutine(DownloadAndConvertCSV());
    }

    private IEnumerator DownloadAndConvertCSV()
    {
        UnityWebRequest request = UnityWebRequest.Get(googleSheetUrl);
        yield return request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("download data failed");
        }
        else
        {
            string csvText = request.downloadHandler.text;
            string json = ConvertCSVToJson(csvText);
            SaveJsonToFile(json);
        }
    }

    private string ConvertCSVToJson(string csvText)
    {
        string[] lines = csvText.Split('\n');
        if (lines.Length <= 1) return "[]";

        string[] header = lines[0].Trim().Split(',');
        var cardList = new List<Dictionary<string, object>>();

        HashSet<string> listFields = new HashSet<string> {
        "skillEffect", "skillValue"
        };

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] values = lines[i].Split(',');

            var card = new Dictionary<string, object>();
            for (int j = 0; j < header.Length && j < values.Length; j++)
            {
                string key = header[j].Trim();
                string value = values[j].Trim();

                if (listFields.Contains(key))
                {
                    value = value.Trim('[', ']');
                    string[] parts = value.Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    List<string> cleaned = new List<string>();
                    foreach (var p in parts)
                    {
                        string trimmed = p.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            cleaned.Add(trimmed);
                    }
                    card[key] = cleaned;
                }
                else
                {
                    card[key] = value;
                }
            }

            cardList.Add(card);
        }

        return JsonConvert.SerializeObject(cardList, Formatting.Indented);
    }

    private void SaveJsonToFile(string json)
    {
        string dir = Application.streamingAssetsPath;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, Terminology.CARDS_JSON_NAME);
        File.WriteAllText(path, json);
        Debug.Log("Save Json to:" + path);
    }
}
