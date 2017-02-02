using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour {

    public Button quit;
    public string[] scenes;

    List<Button> buttons = new List<Button>();
    AsyncOperation async;
    Text text;

    void Start()
    {
        foreach (string scene in scenes)
        {
            Button button = Instantiate(quit);
            button.name = scene;
            button.GetComponentInChildren<Text>().text = scene;
            button.onClick.AddListener(() => Load(scene));
            button.transform.SetParent(transform);
            button.transform.localScale = Vector3.one;
            buttons.Add(button);
        }

        quit.onClick.AddListener(Quit);
        quit.transform.SetAsLastSibling();

        text = quit.GetComponentInChildren<Text>();
    }

    void Load(string scene)
    {
        foreach (Button button in buttons)
        {
            Destroy(button.gameObject);
        }
        async = SceneManager.LoadSceneAsync(scene);
        StartCoroutine(LoadScene(scene));
    }

    IEnumerator LoadScene(string scene)
    {
        text.text = Mathf.Floor(100f * async.progress) + "%";
        Debug.Log(async.progress);
        while (!async.isDone)
        {
            yield return async;
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
