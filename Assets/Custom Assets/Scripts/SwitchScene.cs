using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour {

    public Button quit;
    public string[] scenes;

    public LayerMask mask;

    List<Button> buttons = new List<Button>();
    Text text;

    static IEnumerable<string> SceneNames
    {
        get
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string[] path = SceneUtility.GetScenePathByBuildIndex(i).Split('/');
                string scene = path[path.Length - 1];
                yield return scene.Substring(0, scene.Length - 6);
            }
        }
    }

    public static string[] SceneArray
    {
        get
        {
            return SceneNames.ToArray();
        }
    }

    void Awake()
    {
        int i = 0;
        foreach (string scene in SceneNames)
        {
            int sc = 1 << i;
            if ((mask & sc) != 0)
            {
                Button button = Instantiate(quit, transform);
                button.name = scene;
                button.GetComponentInChildren<Text>().text = scene;
                button.onClick.AddListener(() => Load(scene));
                button.transform.localScale = Vector3.one;
                buttons.Add(button);
            }
            i++;
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
        quit.interactable = false;
        quit.onClick.RemoveAllListeners();
        StartCoroutine(LoadScene(scene));
    }

    IEnumerator LoadScene(string scene)
    {
        yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            text.text = Mathf.Floor(100f * progress) + "%";

            if (async.progress == 0.9f)
            {
                text.text = "Start";
                quit.onClick.AddListener(() => async.allowSceneActivation = true);
                quit.interactable = true;
            }

            yield return null;
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
