### 
## ProbeVolumePerSceneData ##

In `OnEnable`, `ProbeVolumePerSceneData` will change its guid to whatever scene it exists inside of. This happens to be 00000000 when moved into a companion scene.
Changed to check if guid string is empty, and then to ignore subsequent changes.

## Companion Preview Scene ##

**ProbeVolumePerSceneData** is always moved to this scene, but due to the probe system thinking a **ProbeVolumePerSceneData** doesnt exist, 
each domain/scene reload(from code reloads etc) causes this to constantly add up.  

ProbeVolumeBakingSet.Editor.cs `Ln: 546` `EnsurePerSceneData` appears to move the new  *ProbeVolumePerSceneData* into the companion scene.
