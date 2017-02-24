using UnityEditor;

[CustomEditor(typeof(LevelScene))]
public class LevelSceneEditor : ChooseSceneEditor {

    LevelScene levelScene;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        levelScene = target as LevelScene;

        levelScene.Update(ref levelScene.titre, "Titre");
        if (levelScene.titre.Length > 50)
        {
            levelScene.titre = levelScene.titre.Substring(0, 50);
        }
        levelScene.Update(ref levelScene.available, "Available");
    }
}
