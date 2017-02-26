using UnityEngine;
using UnityEngine.UI;

public class ManageButtons : MonoBehaviour {

    public Rewired.Player pInput;

    public Button[] buttons;
    public Button quit;
    public RectTransform telecommande;

    public RectTransform targetTel { get; set; }

    const float moveSpeed = 1000f;

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
        if (telecommande == null)
        {
            return;
        }

        telecommande.position = Vector3.MoveTowards(telecommande.position, targetTel.position, moveSpeed * Time.deltaTime);
        telecommande.rotation = Quaternion.RotateTowards(telecommande.rotation, targetTel.rotation, moveSpeed * Time.deltaTime);

        if (pInput.GetButtonDown(Globals.BtnAction2))
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
