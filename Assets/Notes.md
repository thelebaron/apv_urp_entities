### 
## ProbeVolumePerSceneData ##

In `OnEnable`, `ProbeVolumePerSceneData` will change its guid to whatever scene it exists inside of. This happens to be 00000000 when moved into a companion scene.
Changed to check if guid string is empty, and then to ignore subsequent changes.

## Companion Preview Scene ##

**ProbeVolumePerSceneData** is always moved to this scene, but due to the probe system thinking a **ProbeVolumePerSceneData** doesnt exist, 
each domain/scene reload(from code reloads etc) causes this to constantly add up.  


ProbeVolumeBuildProcessor.cs line 113 as a main scene is not an entity scene the guid wont be available for any baking set for scene in the method.

### ProbeVolumeBuildProcessor ###
This currently doesnt get subscenes when building scenes. `ProbeSubsceneProcessor` does this, you can view its code but essentially it gets the main build 
scene list, gets all subscenes for that scene and then duplicates the ProbeVolumeBuildProcessor's behaviour without many changes. I noticed `cellSupportDataAsset` 
was being stripped or not included so this was the only change in behaviour that I made, to include it specifically.

### ProbeVolumePerSceneData ###

For making this component work with entities, the OnEnable behaviour must be changed to prevent the `sceneGUID` from being changed to a scene it doesnt represent.
I think this stems from the larger issue that *ProbeVolumePerSceneData* doesnt follow the norms of how other unity features like lights or cameras have 
PipelineAdditionalData components attached to them. In viewing the TimeGhost Environment demo, it appears only one *ProbeVolumePerSceneData* is used by multiple 
probe volumes, so I propose refactoring this component to be attached to the `ProbeVolume` and given the baking set is shared, it can be sorted and loaded as needed,
but not remain hidden.

```csharp
    void OnEnable()
    {
        //Debug.Log($"ProbeVolumePerSceneData: onenable {gameObject.scene.GetGUID()} {gameObject.name}" );
        #if UNITY_EDITOR
        // check if we are changing it for a companion scene WHICH IS A BIG OOPSIE NONO
        var companionSceneGuid = "00000000000000000000000000000000";
        var currentGuid        = gameObject.scene.GetGUID();
        if (currentGuid.Equals(companionSceneGuid) && sceneGUID != currentGuid)
        {
            //Debug.LogError($"Companion scene strikes again! {gameObject.name} {gameObject.scene.name} old guid was {oldGUID} {currentGUID}");
        }
        else
        {
            // Initialization check, compare to empty or null
            if (sceneGUID.Equals(""))
            {
                sceneGUID = gameObject.scene.GetGUID();
                EditorUtility.SetDirty(this);
            }
            
            // new finding: cant disable this code as this is what is used when the apv bake process creates a new perscene object and populates the guid
            
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