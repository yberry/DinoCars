using UnityEditor;

[CustomEditor(typeof(LevelScene))]
public class LevelSceneEditor : Editor {

    LevelScene levelScene;

    public override void OnInspectorGUI()
    {
        levelScene = target as LevelScene;

        levelScene.Update(ref levelScene.scene, "Scene", SwitchScene.SceneArray);
        levelScene.Update(ref levelScene.titre, "Titre");
        if (levelScene.titre.Length > 50)
        {
            levelScene.titre = levelScene.titre.Substring(0, 50);
        }
        levelScene.Update(ref levelScene.available, "Available");
    }
}
