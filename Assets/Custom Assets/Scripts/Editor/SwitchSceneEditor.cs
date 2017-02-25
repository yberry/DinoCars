using UnityEditor;

[CustomEditor(typeof(SwitchScene))]
public class SwitchSceneEditor : Editor {

    SwitchScene switchScene;

    public override void OnInspectorGUI()
    {
        switchScene = target as SwitchScene;

        switchScene.Update(ref switchScene.quit, "Quit");

        switchScene.mask = EditorGUILayout.MaskField("Scene", switchScene.mask, SwitchScene.SceneArray);
    }
}
