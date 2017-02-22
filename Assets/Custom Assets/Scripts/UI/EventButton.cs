using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventButton : EventTrigger {

    public override void OnSelect(BaseEventData eventData)
    {
        AkSoundEngine.PostEvent("UI_Button_PassOver_Play", gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        AkSoundEngine.PostEvent("UI_Button_PassOver_Play", gameObject);
    }
}
