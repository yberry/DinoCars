using UnityEditor;

[CustomEditor(typeof(LevelScene))]
public class LevelSceneEditor : Editor {

    LevelScene levelScene;

    public override void OnInspectorGUI()
    {
        levelScene = target as LevelScene;

        levelScene.Update(ref levelScene.scene, "Scene", SwitchScene.SceneArray);
    }
}
