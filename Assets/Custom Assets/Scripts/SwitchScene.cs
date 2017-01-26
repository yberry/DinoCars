using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour {

    public Button button;
    public Object[] scenes;

    void Start()
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            Button b = Instantiate(button);
            b.gameObject.SetActive(true);
            b.transform.SetParent(transform);
            b.transform.localScale = Vector3.one;
            b.GetComponentInChildren<Text>().text = scenes[i].name;
            int index = i;
            b.onClick.AddListener(() => Load(index));
        }
    }

    public void Load(int index)
    {
        SceneManager.LoadScene(scenes[index].name);
    }
}
