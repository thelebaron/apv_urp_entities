using UnityEngine;

public class SubSceneBaker : MonoBehaviour
{
    
}

/*
public class SubSceneBaker : MonoBehaviour
{
    public SceneAsset targetScene;

    private          double startBakeTime;
    private          double lastLogTime;
    private readonly double logInterval = 1.0;
    private          string originalScenePath;
    private          bool   isBaking;
    private          bool   hasStartedBake;
    private          bool   result;
    private          string selectedGameObjectName; // Store the name of the selected gameobject.
    private          int    selectedGameObjectHash; // Store the name of the selected gameobject.
    
    [ContextMenu("Bake")]
    public void Bake()
    {
        if (isBaking)
            return;

        isBaking = true;
        result = false;

        // Save the current (original) scene path.
        originalScenePath = SceneManager.GetActiveScene().path;

        SaveOriginal();
        
        // Open the target scene.
        var targetScenePath = AssetDatabase.GetAssetPath(targetScene);
        EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
        EditorSceneManager.SaveOpenScenes();

        // Set a delay of 1 second before starting the bake.
        startBakeTime = EditorApplication.timeSinceStartup + 0.2f;
        lastLogTime = EditorApplication.timeSinceStartup;
        hasStartedBake = false;

        // Begin monitoring the bake process.
        EditorApplication.update += WaitAndBake;
    }

    private void WaitAndBake()
    {
        var currentTime = EditorApplication.timeSinceStartup;
        
        // Wait 1 second before starting the bake.
        if (!hasStartedBake && currentTime >= startBakeTime)
        {
            result = Lightmapping.Bake();
            hasStartedBake = true;
            Debug.Log("Started lightmap baking.");
            lastLogTime = currentTime;
        }

        // Log the baking status at 1-second intervals once the bake has started.
        if (hasStartedBake && currentTime >= lastLogTime + logInterval)
        {
            if (Lightmapping.isRunning)
                Debug.Log("Lightmap baking is currently in progress...");
            else
                Debug.Log("Lightmap baking has finished.");
            lastLogTime = currentTime;
        }

        // When the bake is complete, reload the original scene.
        if (hasStartedBake && !Lightmapping.isRunning)
        {
            Debug.Log($"Bake process complete! result: {result}");
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            EditorApplication.update -= WaitAndBake;
            isBaking = false;
            
            SelectOriginal();
        }
    }

    private void SaveOriginal()
    {
        // Store the name of the currently selected gameobject.
        selectedGameObjectName = Selection.activeGameObject ? Selection.activeGameObject.name : string.Empty;
        selectedGameObjectHash = Selection.activeGameObject.GetHashCode();
    }

    private void SelectOriginal()
    {
        // Reselect the previously selected gameobject if it exists.
        if (!string.IsNullOrEmpty(selectedGameObjectName))
        {
            var obj = GameObject.Find(selectedGameObjectName);
            if (obj != null && obj.GetHashCode().Equals(selectedGameObjectHash))
            {
                Selection.activeGameObject = obj;
            }
        }
    }
}*/
