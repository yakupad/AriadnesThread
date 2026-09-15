using AriadnesThread.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AriadnesThread.EditorTools
{
    public static class SceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("AriadnesThread/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapGO = new GameObject("MazeBootstrap");
            bootstrapGO.AddComponent<MazeBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Prototype scene written to {ScenePath}");
        }
    }
}
