using UnityEngine;
using UnityEngine.UI;

public class ManageButtons : MonoBehaviour {

    public Button[] buttons;

    public Button quit;

    void Start()
    {
        foreach (Button button in buttons)
        {
            button.gameObject.AddComponent<EventButton>();
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            quit.onClick.Invoke();
        }
    }
}
