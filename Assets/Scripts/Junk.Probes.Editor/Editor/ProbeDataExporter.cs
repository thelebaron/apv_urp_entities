#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Unity.Scenes;
using UnityEditor;
using System.Reflection;
using Unity.Scenes.Editor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Hash128 = Unity.Entities.Hash128;

namespace Junk.Probes.Editor
{
    // see ProbeVolumeBuildProcessor & EntitySceneBuildPlayerProcessor if anything changes
    public class ProbeSubsceneProcessor : BuildPlayerProcessor, IPostprocessBuildWithReport
    {
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            Debug.Log("Preparing probe subscene processor");
            //Debug.Log(buildPlayerContext.BuildPlayerOptions.scenes.Length);
            foreach (var scene in buildPlayerContext.BuildPlayerOptions.scenes)
            {
                //Debug.Log(scene);
            }
            //Debug.Log("Finished probe subscene processor");
            Build();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
        }

        [MenuItem("Tools/Build")]
        static void Build()
        {
            // Retrieve list of subscenes to import from the root scenes added to the player settings
            //var rootSceneInfos = new List<RootSceneInfo>();
            var rootSceneGUIDs = new List<Hash128>();
            var subSceneGuids  = new HashSet<Hash128>();
            //var rootScenePath  = buildPlayerContext.BuildPlayerOptions.scenes[i];
            var allSubscenes  = SubScene.AllSubScenes;
            var allScenePaths = BuildHelper.GetScenePaths();
            var rootScenePath = allScenePaths[0];
            var rootSceneGUID = AssetDatabase.GUIDFromAssetPath(rootScenePath);
            var rootScene     = new RootSceneInfo();
            rootScene.Guid = rootSceneGUID;
            rootScene.Path = rootScenePath;

            var subscenes = EditorEntityScenes.GetSubScenes(rootScene.Guid);
            foreach (var subscene in subscenes)
            {
                Debug.Log(subscene);
                var subScenePath = AssetDatabase.GUIDToAssetPath(subscene);
                Debug.Log(subScenePath);
                var bakingSet = ProbeVolumeBakingSet.GetBakingSetForScene(subscene.ToString());
                if (bakingSet != null)
                {
                    var bakingSetPath = AssetDatabase.GetAssetPath(bakingSet);
                    Debug.Log(bakingSetPath);

                    ProcessBakingSet(bakingSetPath, subscene, bakingSet);
                }
            }

            // I think this part is where building an entity scene fails, as it doesnt take account of any subscene.

            var types = TypeCache.GetTypesDerivedFrom<IEntitySceneBuildAdditions>();
        }

        const string kTempAPVStreamingAssetsPath = "TempAPVStreamingAssets";
        const string kAPVStreamingAssetsPath     = "APVStreamingAssets";

        static string GetTempAPVStreamingAssetsPath()
        {
            var libraryPath = Path.GetFullPath("Library");
            return Path.Combine(libraryPath, kTempAPVStreamingAssetsPath);
        }

        static string GetAPVStreamingAssetsPath()
        {
            var libraryPath = Path.GetFullPath("Assets/StreamingAssets");
            return Path.Combine(libraryPath, kAPVStreamingAssetsPath);
        }

        private static void LogBakingSetGuids(ProbeVolumeBakingSet bakingSet)
        {
            Debug.Log($" bakingSet.cellSharedDataAsset: {bakingSet.cellSharedDataAsset.assetGUID}");
            Debug.Log($" bakingSet.cellSupportDataAsset: {bakingSet.cellSupportDataAsset.assetGUID}");
            Debug.Log($" bakingSet.cellBricksDataAsset: {bakingSet.cellBricksDataAsset.assetGUID}");
            Debug.Log($" bakingSet scenarios");
            foreach (var scenarioData in bakingSet.scenarios)
            {
                Debug.Log($" cellDataAsset: {scenarioData.Value.cellDataAsset.assetGUID}");
                Debug.Log($" cellOptionalDataAsset: {scenarioData.Value.cellOptionalDataAsset.assetGUID}");
                Debug.Log($" cellProbeOcclusionDataAsset: {scenarioData.Value.cellProbeOcclusionDataAsset.assetGUID}");
            }
        }


        private static void ProcessBakingSet(string bakingSetPath, Hash128 subscene, ProbeVolumeBakingSet bakingSet)
        {
            LogBakingSetGuids(bakingSet);
            GetProbeVolumeProjectSettings(out var supportsProbeVolumes, out var maxSHBands);
            // temp
            var streamingAssetsPath = GetAPVStreamingAssetsPath();


            if (!bakingSet.cellSharedDataAsset.IsValid()) // Not baked
                return;

            var bakingSetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bakingSet));
            var basePath      = Path.Combine(streamingAssetsPath, bakingSetGUID);

            Directory.CreateDirectory(basePath);

            bool useStreamingAsset = !GraphicsSettings.GetRenderPipelineSettings<ProbeVolumeGlobalSettings>().probeVolumeDisableStreamingAssets;
            Debug.Log("useStreamingAsset " + useStreamingAsset);
            IncludeStreamableAsset(bakingSet.cellSharedDataAsset, basePath, useStreamingAsset);
            IncludeStreamableAsset(bakingSet.cellBricksDataAsset, basePath, useStreamingAsset);
            IncludeStreamableAsset(bakingSet.cellSupportDataAsset, basePath, useStreamingAsset);
            // For now we always strip support data in build as it's mostly unsupported.
            // Later we'll need a proper option to strip it or not.
            /*bool stripSupportData = true;
            if (stripSupportData)
                StripStreambleAsset(bakingSet.cellSupportDataAsset);
            else
                IncludeStreamableAsset(bakingSet.cellSupportDataAsset, basePath, useStreamingAsset);
            */
            foreach (var scenario in bakingSet.scenarios)
            {
                IncludeStreamableAsset(scenario.Value.cellDataAsset, basePath, useStreamingAsset);
                if (maxSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
                    IncludeStreamableAsset(scenario.Value.cellOptionalDataAsset, basePath, useStreamingAsset);
                else
                    StripStreambleAsset(scenario.Value.cellOptionalDataAsset);
                IncludeStreamableAsset(scenario.Value.cellProbeOcclusionDataAsset, basePath, useStreamingAsset);
            }

            //s_BakingSetsProcessedLastBuild.Add(bakingSet);
        }

        // Include an asset in the build. The mechanism for doing so depends on whether we are using StreamingAssets path.
        static void IncludeStreamableAsset(ProbeVolumeStreamableAsset asset, string basePath, bool useStreamingAsset)
        {
            if (useStreamingAsset)
            {
                asset.ClearAssetReferenceForBuild();
                CopyStreamableAsset(asset, basePath);
            }
            else
            {
                asset.EnsureAssetLoaded();
            }
        }

        static void CopyStreamableAsset(ProbeVolumeStreamableAsset asset, string basePath)
        {
            var assetPath = asset.GetAssetPath();
            if (!File.Exists(assetPath))
            {
                Debug.LogError($"Missing APV data asset {assetPath}. Please make sure that the lighting has been baked properly.");
                return;
            }

            Debug.Log($"Copying {assetPath} to {basePath}");
            const bool overwrite = true; // maybe add a toggle later?
            File.Copy(assetPath, Path.Combine(basePath, asset.assetGUID + ".bytes"), overwrite);
        }

        // Ensure that an asset is not included in the build.
        static void StripStreambleAsset(ProbeVolumeStreamableAsset asset)
        {
            asset.ClearAssetReferenceForBuild();
        }

        static void GetProbeVolumeProjectSettings(out bool supportProbeVolume, out ProbeVolumeSHBands maxSHBands)
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            maxSHBands         = ProbeVolumeSHBands.SphericalHarmonicsL1;
            supportProbeVolume = false;

            var probeVolumeEnabledRenderPipeline = asset as IProbeVolumeEnabledRenderPipeline;
            // If at least one asset needs L2 then we can return.
            if (probeVolumeEnabledRenderPipeline != null)
            {
                supportProbeVolume |= probeVolumeEnabledRenderPipeline.supportProbeVolume;

                if (probeVolumeEnabledRenderPipeline.maxSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
                    maxSHBands = ProbeVolumeSHBands.SphericalHarmonicsL2;
            }
        }
    }


    internal struct RootSceneInfo
    {
        public string  Path;
        public Hash128 Guid;
    }

    public static class BuildHelper
    {
        public static string[] GetScenePaths()
        {
            //Debug.Log("GetScenePaths");
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            var scenes     = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                scenes[i] = path; //System.IO.Path.GetFileNameWithoutExtension(path);
                //Debug.Log(path);
                //Debug.Log(scenes[i]);
            }

            return scenes;
        }

        public static BuildPlayerContext GetBuildPlayerContext()
        {
            // Get the BuildPlayerWindow type
            var buildPlayerWindowType = typeof(BuildPlayerWindow);
            // Get an instance of the BuildPlayerWindow
            var window = EditorWindow.GetWindow(buildPlayerWindowType);
            // Get the internal field "m_BuildPlayerContext"
            var field = buildPlayerWindowType.GetField("m_BuildPlayerContext", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                return (BuildPlayerContext)field.GetValue(window);
            return null;
        }
    }
}

#endif