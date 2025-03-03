# Adaptive Probe Volume and Entities #

#### Exclusive Subscene compatibility #### 
Currently AdaptiveProbeVolumes only work when placed outside of a subscene, any instance where they are included inside, lighting data wont load, see `IN-96313`. 
I think part of this stems from `ProbeVolumePerSceneData` not being included in `CompanionComponentSupportedTypes` - see AdaptiveProbeVolumeBakers.cs for details on this.
Without this, a *ProbeVolumePerSceneData* is always discarded during the baking process. Existing examples(TimeGhost Environment) of APVs shows their use outside of subscenes, but 
a project built around entities exclusively(using subscenes only as methods for scene/level changes) is incompatible with how it works currently.

#### SceneGuid lazy initialization problems #### 
Another issue is how the sceneguid for *ProbeVolumePerSceneData* is handled. It does a lazy initialization any time its enabled, but this is problematic in the editor due to
the way Companion scenes are handled(gameobjects from subscenes are moved to EditorPreviewScenes, so when when this happens the guid will not match the serialized data).
Ive changed this to only change the guid once when the component is enabled for the first time. Imo this should be set when the scene is baked in editor, not any other time.

```csharp
    void OnEnable()
    {
        //Debug.Log($"ProbeVolumePerSceneData: onenable {gameObject.scene.GetGUID()} {gameObject.name}" );
        #if UNITY_EDITOR
        // check if we are changing it for a companion scene 
        var companionSceneGuid = "00000000000000000000000000000000";
        var currentGuid        = gameObject.scene.GetGUID();
        if (currentGuid.Equals(companionSceneGuid) && sceneGUID != currentGuid)
        {
            //Debug.LogError($"Companion scene strikes again! {gameObject.name} {gameObject.scene.name} old guid was {oldGUID} {currentGUID}");
        }
        else
        {
            // Initialization check, compare to empty
            if (sceneGUID.Equals(""))
            {
                sceneGUID = gameObject.scene.GetGUID();
                EditorUtility.SetDirty(this);
            }
            
            // Old code
            // In the editor, always refresh the GUID as it may become out of date is scene is duplicated or other weird things
            // This field is serialized, so it will be available in standalones, where it can't change anymore
            /*var newGUID = gameObject.scene.GetGUID();
            if (newGUID != sceneGUID)
            {
                sceneGUID = newGUID;
                EditorUtility.SetDirty(this);
            }*/
        }
        #endif
        
        ProbeReferenceVolume.instance.RegisterPerSceneData(this);
    }
```

#### Hidden gameobject data conflicts with established renderpipeline standards #### 
I think this component should follow the example set by other renderpipeline components
such as `HDRPAdditionalLightData`, `UniversalAdditionalLightData`, `UniversalAdditionalCameraData` etc. They shouldnt be hidden, and should just live on the ProbeVolume gameObject.
It certainly makes it harder to debug the problems with this component when problems arise.

#### Built Player fixes #### 
The final problem is the build step for APVs as is, does not factor subscenes into account. I've copied the basic functionality of this in `ProbeSubsceneProcessor`, essentially it gets the main build
scene list, gets all subscenes for that scene and then duplicates the ProbeVolumeBuildProcessor's behaviour without many changes. I noticed `cellSupportDataAsset`
was being stripped or not included so this was the only change in behaviour that I made, to include it specifically.

With the changes included in this package(Junk.ProbeVolumes), I've successfully built and run a scene with ProbeVolumes in a subscene.


#### Other issues & fixes #### 
Currently when lighting is baked, it auto opens all subscenes in a scene. This is problematic if you want two individual scenes to have separate lighting. Ive made a script that opens a 
subscene as the main scene, bakes its lighting and then reloads the previous scene. It would be nice not to need this workaround, to open a subscene and bake its lighting on an 
individual basis.

Finally URP ProbeVolumes should be made officially compatible with Entities Graphics (ie the conditional code to support APV only exists for HDRP which I think might be a holdover from 
APV's making their debut on HDRP)if these fixes are considered for HDRP.