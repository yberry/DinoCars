using UnityEngine;
using UnityEngine.UI;

public class Quit : MonoBehaviour {

    public Button quit;

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            quit.onClick.Invoke();
        }
    }
}
