using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;

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

        int timingIndex = Array.IndexOf(header, "skillTiming");
        int effectIndex = Array.IndexOf(header, "skillEffect");
        int valueIndex = Array.IndexOf(header, "skillValue");
        int targetIndex = Array.IndexOf(header, "skillTarget");
        int textIndex = Array.IndexOf(header, "skillText");

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
                    string[] parts = value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> cleaned = new List<string>(parts.Select(p => p.Trim()));
                    card[key] = cleaned;
                }
                else
                {
                    card[key] = value;
                }
            }

            List<ModelDatas.TriggerConfig> triggers = new List<ModelDatas.TriggerConfig>();

            if (timingIndex >= 0 && effectIndex >= 0 && valueIndex >= 0 && targetIndex >= 0)
            {
                string timing = (timingIndex < values.Length) ? values[timingIndex].Trim() : "";
                string target = (targetIndex < values.Length) ? values[targetIndex].Trim() : "";
                List<string> effects = (effectIndex < values.Length) ? (List<string>)card["skillEffect"] : new List<string>();
                List<string> vals = (valueIndex < values.Length) ? (List<string>)card["skillValue"] : new List<string>();

                if (!string.IsNullOrEmpty(timing) && effects.Count > 0)
                {
                    // 轉成 TriggerConfig
                    var trigger = new ModelDatas.TriggerConfig
                    {
                        skillTiming = timing,
                        skillTarget = target,
                        skillEffect = effects,
                        skillValue = vals,
                        description = (textIndex >= 0 && textIndex < values.Length) ? values[textIndex].Trim() : ""
                    };
                    triggers.Add(trigger);
                }
            }

            card["triggers"] = triggers;

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
