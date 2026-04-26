#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class CardDataImporter
{
    private const string googleSheetUrl = "https://docs.google.com/spreadsheets/d/1f38pAH0_fSdSwnX_1mqyIHhz9-qbQocQq79RPTb3zhs/export?format=csv&gid=0";
    
    // Save generated .asset files to the requested folder
    private const string savePath = "Assets/ScriptableObjects/Cards";

    [MenuItem("TradingCardGame/Import Cards from Google Sheets")]
    public static async void ImportCards()
    {
        EditorUtility.DisplayProgressBar("Downloading Card Data", "Fetching CSV from Google Sheets...", 0.2f);
        
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(googleSheetUrl);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download data failed: " + request.error);
                return;
            }

            string csvText = request.downloadHandler.text;
            EditorUtility.DisplayProgressBar("Processing Card Data", "Parsing CSV and generating assets...", 0.6f);
            ParseCSVAndCreateAssets(csvText);
        }
        catch (Exception e)
        {
            Debug.LogError("Error importing cards: " + e.Message);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void ParseCSVAndCreateAssets(string csvText)
    {
        // Make sure the target directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string[] lines = csvText.Split('\n');
        if (lines.Length <= 1) return;

        string[] header = lines[0].Trim().Split(',');
        
        int timingIndex = Array.IndexOf(header, "skillTiming");
        int effectIndex = Array.IndexOf(header, "skillEffect");
        int valueIndex = Array.IndexOf(header, "skillValue");
        int targetIndex = Array.IndexOf(header, "skillTarget");
        int textIndex = Array.IndexOf(header, "skillText");

        HashSet<string> listFields = new HashSet<string> { "skillEffect", "skillValue" };

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] values = lines[i].Split(',');

            var cardDict = new Dictionary<string, object>();
            for (int j = 0; j < header.Length && j < values.Length; j++)
            {
                string key = header[j].Trim();
                string value = values[j].Trim();

                if (listFields.Contains(key))
                {
                    value = value.Trim('[', ']');
                    string[] parts = value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> cleaned = new List<string>(parts.Select(p => p.Trim()));
                    cardDict[key] = cleaned;
                }
                else
                {
                    cardDict[key] = value;
                }
            }

            // Extract ID to use as filename
            string id = GetString(cardDict, "id");
            if (string.IsNullOrEmpty(id)) continue;

            // Check if the ScriptableObject already exists, so we don't break existing references
            string assetPath = $"{savePath}/{id}.asset";
            CardDataSO cardAsset = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            bool isNew = false;
            
            if (cardAsset == null)
            {
                cardAsset = ScriptableObject.CreateInstance<CardDataSO>();
                isNew = true;
            }

            // Map standard properties
            cardAsset.id = id;
            cardAsset.cardName = GetString(cardDict, "cardName");
            cardAsset.element = GetString(cardDict, "element");
            cardAsset.type = GetString(cardDict, "type");
            cardAsset.cost = GetInt(cardDict, "cost");
            cardAsset.atk = GetInt(cardDict, "atk");
            cardAsset.hp = GetInt(cardDict, "hp");
            cardAsset.skillType = GetString(cardDict, "skillType");
            cardAsset.skillText = GetString(cardDict, "skillText");
            cardAsset.cardCount = GetInt(cardDict, "cardCount");

            // Build Triggers list
            cardAsset.triggers = new List<CardTrigger>();
            if (timingIndex >= 0 && effectIndex >= 0 && valueIndex >= 0 && targetIndex >= 0)
            {
                string timing = (timingIndex < values.Length) ? values[timingIndex].Trim() : "";
                string target = (targetIndex < values.Length) ? values[targetIndex].Trim() : "";
                List<string> effects = cardDict.ContainsKey("skillEffect") ? (List<string>)cardDict["skillEffect"] : new List<string>();
                List<string> vals = cardDict.ContainsKey("skillValue") ? (List<string>)cardDict["skillValue"] : new List<string>();

                if (!string.IsNullOrEmpty(timing) && effects.Count > 0)
                {
                    var trigger = new CardTrigger
                    {
                        skillTiming = timing,
                        skillTarget = target,
                        effects = new List<CardEffect>(),
                        description = (textIndex >= 0 && textIndex < values.Length) ? values[textIndex].Trim() : ""
                    };

                    for (int k = 0; k < effects.Count; k++)
                    {
                        string eType = effects[k];
                        string eVal = k < vals.Count ? vals[k] : "";

                        CardEffect ce = new CardEffect { type = eType };

                        if (int.TryParse(eVal, out int num))
                        {
                            ce.value = num;
                        }
                        else if (eType == "Status")
                        {
                            ce.status = eVal;
                        }
                        else if (eType == "Buff" || eType == "DamageReduction")
                        {
                            ce.stat = eVal;
                        }
                        else 
                        {
                            ce.stat = eVal; // fallback
                        }

                        trigger.effects.Add(ce);
                    }

                    cardAsset.triggers.Add(trigger);
                }
            }

            // Save the asset
            if (isNew)
            {
                AssetDatabase.CreateAsset(cardAsset, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(cardAsset);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log($"[CardDataImporter] Successfully imported cards from Google Sheets to {savePath}");
    }


    private static string GetString(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) ? dict[key].ToString() : "";
    }

    private static int GetInt(Dictionary<string, object> dict, string key)
    {
        if (dict.ContainsKey(key) && int.TryParse(dict[key].ToString(), out int val))
        {
            return val;
        }
        return 0;
    }
}
#endif
