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
                
                List<string> effectNames = (effectIndex < values.Length && card.ContainsKey("skillEffect")) ? (List<string>)card["skillEffect"] : new List<string>();
                List<string> effectValues = (valueIndex < values.Length && card.ContainsKey("skillValue")) ? (List<string>)card["skillValue"] : new List<string>();

                if (!string.IsNullOrEmpty(timing) && effectNames.Count > 0)
                {
                    var trigger = new ModelDatas.TriggerConfig
                    {
                        skillTiming = timing,
                        skillTarget = target,
                        effects = new List<ModelDatas.EffectData>(),
                        description = (textIndex >= 0 && textIndex < values.Length) ? values[textIndex].Trim() : ""
                    };

                    for (int k = 0; k < effectNames.Count; k++)
                    {
                        var effectData = new ModelDatas.EffectData
                        {
                            effectType = effectNames[k],
                            effectValue = (k < effectValues.Count) ? effectValues[k] : ""
                        };
                        ParseEffectDetails(effectData);
                        trigger.effects.Add(effectData);
                    }
                    triggers.Add(trigger);
                }
            }

            card["triggers"] = triggers;

            cardList.Add(card);
        }

        return JsonConvert.SerializeObject(cardList, Formatting.Indented);
    }

    private void ParseEffectDetails(ModelDatas.EffectData data)
    {
        if (string.IsNullOrEmpty(data.effectValue)) return;
        string raw = data.effectValue.Trim();

        // 1. Handle Status effects like "Freeze(1)"
        var statusMatch = System.Text.RegularExpressions.Regex.Match(raw, @"(\w+)\s*\(\s*(\d+)\s*\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (statusMatch.Success)
        {
            data.subType = statusMatch.Groups[1].Value;
            data.duration = int.Parse(statusMatch.Groups[2].Value);
            data.value = data.duration; // Fallback for some systems that use value as duration
            return;
        }

        // 2. Handle Buff effects like "HP +5" or "Damage+2"
        if (data.effectType == "Buff")
        {
            if (raw.Contains("ATK") || raw.Contains("Damage")) data.subType = "ATK";
            else if (raw.Contains("HP")) data.subType = "HP";
        }

        // 3. Extract generic numbers for Damage, Heal, etc.
        string numStr = "";
        foreach (char c in raw)
        {
            if (char.IsDigit(c) || c == '-') numStr += c;
        }

        if (int.TryParse(numStr, out int val))
        {
            data.value = val;
        }
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
