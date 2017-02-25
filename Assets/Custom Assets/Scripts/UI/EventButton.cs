using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventButton : EventTrigger {

    const string overMenu = "UI_Button_PassOver_Play";

    public override void OnSelect(BaseEventData eventData)
    {
        AkSoundEngine.PostEvent(overMenu, gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        AkSoundEngine.PostEvent(overMenu, gameObject);
    }
}
