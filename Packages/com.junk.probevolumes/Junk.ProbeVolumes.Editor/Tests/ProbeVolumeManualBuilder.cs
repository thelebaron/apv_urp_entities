
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.ProbeVolumes.Editor
{
    public static class ProbeVolumeSelectedBuilder
    {
        // Builds a single binary file from the selected ProbeVolumePerSceneData.
        // The output file is placed at Assets/StreamingAssets/APVStreamingAssets/{sceneGUID}.bytes.
        [MenuItem("Tools/Build Selected Probe Volume Data")]
        public static void BuildSelectedProbeVolumeData()
        {
            // Get the selected GameObject and its ProbeVolumePerSceneData component.
            var selectedGO = Selection.activeGameObject;
            if (selectedGO == null)
            {
                Debug.LogError("No GameObject selected.");
                return;
            }

            var perSceneData = selectedGO.GetComponent<ProbeVolumePerSceneData>();
            if (perSceneData == null)
            {
                Debug.LogError("Selected GameObject does not have a ProbeVolumePerSceneData component.");
                return;
            }

            // Retrieve the serialized baking set and scene GUID.
            //var bakingSet = perSceneData.serializedBakingSet;
        
            // Set internal 'serializedBakingSet' using reflection.
            var bakingSetField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("serializedBakingSet", BindingFlags.Instance | BindingFlags.NonPublic);
            if (bakingSetField == null)
                throw new System.Exception("Failed to get serializedBakingSet field");
            var bakingSet = (ProbeVolumeBakingSet)bakingSetField.GetValue(perSceneData);

            //var stringGuidField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("sceneGUID", BindingFlags.Instance | BindingFlags.NonPublic);
            string sceneGUID = perSceneData.sceneGUID;
            if (string.IsNullOrEmpty(sceneGUID))
            {
                Debug.LogError("ProbeVolumePerSceneData does not contain a valid scene GUID.");
                return;
            }

            // Determine streaming asset mode and max SH bands.
            bool useStreamingAsset = true;//!GraphicsSettings.GetRenderPipelineSettings<ProbeVolumeGlobalSettings>().probeVolumeDisableStreamingAssets;

            var maxSHBands = ProbeVolumeSHBands.SphericalHarmonicsL1;
            var currentRP  = GraphicsSettings.defaultRenderPipeline;
            if (currentRP is IProbeVolumeEnabledRenderPipeline probeRP && probeRP.maxSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
                maxSHBands = ProbeVolumeSHBands.SphericalHarmonicsL2;

            // Dictionary to hold asset GUID and its binary content.
            var assetData = new Dictionary<string, byte[]>();

            // Local helper to process an asset.
            void ProcessAsset(ProbeVolumeStreamableAsset asset, bool includeAsset)
            {
                if (asset == null)
                    return;

                if (includeAsset)
                {
                    if (useStreamingAsset)
                        asset.ClearAssetReferenceForBuild();
                    else
                        asset.EnsureAssetLoaded();

                    string assetPath = asset.GetAssetPath();
                    if (!File.Exists(assetPath))
                    {
                        Debug.LogError($"Missing APV data asset at {assetPath}. Ensure that lighting has been baked.");
                        return;
                    }

                    try
                    {
                        byte[] bytes = File.ReadAllBytes(assetPath);
                        assetData[asset.assetGUID] = bytes;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error reading asset at {assetPath}: {ex.Message}");
                    }
                }
                else
                {
                    asset.ClearAssetReferenceForBuild();
                }
            }

            // Process main baking set assets.
            ProcessAsset(bakingSet.cellSharedDataAsset, true);
            ProcessAsset(bakingSet.cellBricksDataAsset, true);
            // Support data is stripped.
            ProcessAsset(bakingSet.cellSupportDataAsset, false);

            // Process scenario assets.
            foreach (var scenario in bakingSet.scenarios)
            {
                ProcessAsset(scenario.Value.cellDataAsset, true);
                if (maxSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
                    ProcessAsset(scenario.Value.cellOptionalDataAsset, true);
                else
                    ProcessAsset(scenario.Value.cellOptionalDataAsset, false);
                ProcessAsset(scenario.Value.cellProbeOcclusionDataAsset, true);
            }

            // Write combined data into a single binary file.
            string outputFolder = "Assets/StreamingAssets/APVStreamingAssets";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string outputPath = Path.Combine(outputFolder, sceneGUID + ".bytes");

            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
                {
                    // Write a header: version and number of asset entries.
                    writer.Write(1); // Version number.
                    writer.Write(assetData.Count);
                    foreach (var kvp in assetData)
                    {
                        string assetGuid = kvp.Key;
                        byte[] data      = kvp.Value;
                        writer.Write(assetGuid);
                        writer.Write(data.Length);
                        writer.Write(data);
                    }
                }
                Debug.Log("Probe Volume data built successfully at: " + outputPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error writing combined probe volume data: " + ex.Message);
            }

            AssetDatabase.Refresh();
        }
    }
}

#endif