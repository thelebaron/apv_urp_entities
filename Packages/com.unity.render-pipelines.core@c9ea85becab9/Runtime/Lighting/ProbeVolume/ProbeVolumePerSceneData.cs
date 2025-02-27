using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Rendering
{
    /// <summary>
    /// A component that stores baked probe volume state and data references. Normally hidden in the hierarchy.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")] // Hide.
    public class ProbeVolumePerSceneData : MonoBehaviour
    {
        /// <summary>The baking set this scene is part of.</summary>
        public ProbeVolumeBakingSet bakingSet => serializedBakingSet;

        // Warning: this is the baking set this scene was part of during last bake
        // It shouldn't be used while baking as the scene may have been moved since then
        [SerializeField, FormerlySerializedAs("bakingSet")] internal ProbeVolumeBakingSet serializedBakingSet;
        [SerializeField] public string sceneGUID = "";

        // All code bellow is only kept in order to be able to cleanup obsolete data.
        [Serializable]
        internal struct ObsoletePerScenarioData
        {
            public int sceneHash;
            public TextAsset cellDataAsset; // Contains L0 L1 SH data
            public TextAsset cellOptionalDataAsset; // Contains L2 SH data
        }

        [Serializable]
        struct ObsoleteSerializablePerScenarioDataItem
        {
#pragma warning disable 649 // is never assigned to, and will always have its default value
            public string scenario;
            public ObsoletePerScenarioData data;
#pragma warning restore 649
        }

        [FormerlySerializedAs("asset")]
        [SerializeField] internal ObsoleteProbeVolumeAsset obsoleteAsset;
        [FormerlySerializedAs("cellSharedDataAsset")]
        [SerializeField] internal TextAsset obsoleteCellSharedDataAsset; // Contains bricks and validity data
        [FormerlySerializedAs("cellSupportDataAsset")]
        [SerializeField] internal TextAsset obsoleteCellSupportDataAsset; // Contains debug data
        [FormerlySerializedAs("serializedScenarios")]
        [SerializeField] List<ObsoleteSerializablePerScenarioDataItem> obsoleteSerializedScenarios = new();

#if UNITY_EDITOR
        void DeleteAsset(Object asset)
        {
            if (asset != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long instanceID))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
#endif

        internal void Clear()
        {
            QueueSceneRemoval();
            serializedBakingSet = null;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        internal void QueueSceneLoading()
        {
            if (serializedBakingSet == null)
            {
                Debug.LogError($"ProbeVolumePerSceneData: serializedBakingSet is null. Cannot queue scene loading. {gameObject.name} {gameObject.scene.name}");
                return;
            }

            #if UNITY_EDITOR
            // Check if we are trying to load APV data for a scene which has not enabled APV (or it was removed)
            var bakedData = serializedBakingSet.GetSceneBakeData(sceneGUID);
            if (bakedData != null && bakedData.hasProbeVolume == false)
            {
                Debug.LogError($"ProbeVolumePerSceneData: Scene {sceneGUID} has no probe volume data. Cannot queue scene loading.");
                return;
            }
            #endif

            var refVol = ProbeReferenceVolume.instance;
            refVol.AddPendingSceneLoading(sceneGUID, serializedBakingSet);
            //Debug.Log($"ProbeVolumePerSceneData: AddPendingSceneLoading.");
        }

        internal void QueueSceneRemoval()
        {
            if (serializedBakingSet != null)
                ProbeReferenceVolume.instance.AddPendingSceneRemoval(sceneGUID);
        }

        void OnEnable()
        {
            #if UNITY_EDITOR
            // In the editor, always refresh the GUID as it may become out of date is scene is duplicated or other weird things
            // This field is serialized, so it will be available in standalones, where it can't change anymore
            var newGUID = gameObject.scene.GetGUID();
            if (newGUID != sceneGUID)
            {
                sceneGUID = newGUID;
                EditorUtility.SetDirty(this);
            }
            // test delete invalid guid
            if(sceneGUID.Equals("00000000000000000000000000000000"))
            {
                var nullData = serializedBakingSet == null;
                Debug.LogError($"ProbeVolumePerSceneData: Scene {gameObject.scene.name} has invalid GUID. Cannot RegisterPerSceneData. serializedBakingSet null: {nullData} {serializedBakingSet }");
                //DestroyImmediate(gameObject);
                gameObject.name = "ProbeVolumePerSceneData (Invalid GUID)";
                //DestroyImmediate(gameObject);
                return;
            }
            
            #endif

            ProbeReferenceVolume.instance.RegisterPerSceneData(this);
        }

        void OnDisable()
        {
            QueueSceneRemoval();
            ProbeReferenceVolume.instance.UnregisterPerSceneData(this);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Cleanup old obsolete data.
            if (obsoleteAsset != null)
            {
                DeleteAsset(obsoleteAsset);
                DeleteAsset(obsoleteCellSharedDataAsset);
                DeleteAsset(obsoleteCellSupportDataAsset);
                foreach(var scenario in obsoleteSerializedScenarios)
                {
                    DeleteAsset(scenario.data.cellDataAsset);
                    DeleteAsset(scenario.data.cellOptionalDataAsset);
                }

                obsoleteAsset = null;
                obsoleteCellSharedDataAsset = null;
                obsoleteCellSupportDataAsset = null;
                obsoleteSerializedScenarios = null;

                EditorUtility.SetDirty(this);
            }
#endif
        }

        internal void Initialize()
        {
            ProbeReferenceVolume.instance.RegisterBakingSet(this);

            Debug.Log(sceneGUID);
            QueueSceneRemoval();
            QueueSceneLoading();
        }

        internal bool ResolveCellData()
        {
            if (serializedBakingSet != null)
                return serializedBakingSet.ResolveCellData(serializedBakingSet.GetSceneCellIndexList(sceneGUID));

            return false;
        }
    }
}
