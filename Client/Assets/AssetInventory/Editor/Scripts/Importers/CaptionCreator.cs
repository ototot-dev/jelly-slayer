using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetInventory
{
    public sealed class CaptionCreator : AssetImporter
    {
        public async Task Run()
        {
            List<string> types = new List<string>();
            if (AI.Config.aiForPrefabs) types.AddRange(AI.TypeGroups[AI.AssetGroup.Prefabs]);
            if (AI.Config.aiForImages) types.AddRange(AI.TypeGroups[AI.AssetGroup.Images]);
            if (AI.Config.aiForModels) types.AddRange(AI.TypeGroups[AI.AssetGroup.Models]);

            string typeStr = string.Join("\",\"", types);
            string query = "select *, AssetFile.Id as Id from AssetFile inner join Asset on Asset.Id = AssetFile.AssetId where Asset.Exclude = false and Asset.UseAI = true and AssetFile.Type in (\"" + typeStr + "\") and AssetFile.AICaption is null and (AssetFile.PreviewState = ? or AssetFile.PreviewState = ?) order by Asset.Id desc";
            List<AssetInfo> files = DBAdapter.DB.Query<AssetInfo>(query, AssetFile.PreviewOptions.Custom, AssetFile.PreviewOptions.Provided).ToList();

            await Run(files);
        }

        public async Task Run(List<AssetInfo> files)
        {
            string previewFolder = AI.GetPreviewFolder();

            int chunkSize = AI.Config.blipChunkSize;
            bool toolChainWorking = true;

            MainCount = files.Count;
            for (int i = 0; i < files.Count; i += chunkSize)
            {
                if (CancellationRequested) break;
                await Task.Delay(AI.Config.aiPause * 1000); // crashes system otherwise after a while

                List<AssetInfo> fileChunk = files.Skip(i).Take(chunkSize).ToList();
                List<string> previewFiles = new List<string>();

                foreach (AssetInfo file in fileChunk)
                {
                    SetProgress(file.FileName, i + 1);

                    string previewFile = ValidatePreviewFile(file, previewFolder);
                    if (!string.IsNullOrEmpty(previewFile))
                    {
                        previewFiles.Add(previewFile);
                    }
                }
                if (previewFiles.Count == 0) continue;

                await Task.Run(() =>
                {
                    List<BlipResult> captions = CaptionImage(previewFiles);
                    if (captions != null && captions.Count > 0)
                    {
                        for (int j = 0; j < captions.Count; j++)
                        {
                            if (captions[j].caption != null)
                            {
                                fileChunk[j].AICaption = captions[j].caption;
                                DBAdapter.DB.Execute("update AssetFile set AICaption=? where Id=?", fileChunk[j].AICaption, fileChunk[j].Id);

                                if (AI.Config.logAICaptions)
                                {
                                    Debug.Log($"Caption: {captions[j].caption} ({fileChunk[j].FileName})");
                                }
                            }
                            else if (i == 0)
                            {
                                if (!AI.Config.aiContinueOnEmpty) toolChainWorking = false;
                            }
                        }
                    }
                    else if (i == 0)
                    {
                        if (!AI.Config.aiContinueOnEmpty) toolChainWorking = false;
                    }
                });
                if (!toolChainWorking) break;
            }
        }

        public static List<BlipResult> CaptionImage(List<string> filenames)
        {
            string blipType = AI.Config.blipType == 1 ? "--large" : "";
            string gpuUsage = AI.Config.aiUseGPU ? "--gpu" : "";
            string nameList = "\"" + string.Join("\" \"", filenames.Select(IOUtils.ToShortPath)) + "\"";
            string command = AI.Config.aiToolPath != null ? Path.Combine(AI.Config.aiToolPath, "blip-caption") : "blip-caption";
            string result = IOUtils.ExecuteCommand(command, $"{blipType} {gpuUsage} --json {nameList}");

            if (string.IsNullOrWhiteSpace(result)) return null;

            List<BlipResult> resultList = null;
            try
            {
                resultList = JsonConvert.DeserializeObject<List<BlipResult>>(result);

            }
            catch (Exception e)
            {
                Debug.LogError($"Could not parse Blip result '{result}': {e.Message}");
            }
            return resultList;
        }
    }

    public class BlipResult
    {
        public string path;
        public string caption;
    }
}