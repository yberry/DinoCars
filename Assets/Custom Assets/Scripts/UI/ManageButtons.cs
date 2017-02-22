using UnityEngine;
using UnityEngine.UI;

public class ManageButtons : MonoBehaviour {

    public Rewired.Player pInput;

    public Button[] buttons;

    public Button quit;

    public RectTransform telecommande;

    public RectTransform targetTel { get; set; }

    public bool IsNear
    {
        get
        {
            return Vector3.Distance(telecommande.position, targetTel.position) < 0.1f;
        }
    }

    void Start()
    {
        foreach (Button button in buttons)
        {
            button.gameObject.AddComponent<EventButton>();
            button.onClick.AddListener(PlayClick);
        }
        quit.gameObject.AddComponent<EventButton>();
        quit.onClick.AddListener(QuitClick);

        pInput = Rewired.ReInput.players.GetPlayer(0);
    }

    void Update()
    {
        telecommande.position = Vector3.MoveTowards(telecommande.position, targetTel.position, 1000f * Time.deltaTime);
        telecommande.rotation = Quaternion.RotateTowards(telecommande.rotation, targetTel.rotation, 1000f * Time.deltaTime);

        if (pInput.GetAxis(Globals.BtnAction2) > 0f)
        {
            quit.onClick.Invoke();
        }
    }

    void PlayClick()
    {
        AkSoundEngine.PostEvent("UI_Button_Fwd_Play", gameObject);
    }

    void QuitClick()
    {
        AkSoundEngine.PostEvent("UI_Button_Bkwd_Play", gameObject);
    }
}
