using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChooseScene))]
public class ChooseSceneEditor : Editor {

    ChooseScene chooseScene;

    public override void OnInspectorGUI()
    {
        chooseScene = target as ChooseScene;

        chooseScene.Update(ref chooseScene.scene, "Scene", SwitchScene.SceneArray);
    }
}
