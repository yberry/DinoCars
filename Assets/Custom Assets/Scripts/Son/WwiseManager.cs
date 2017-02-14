using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseManager : MonoBehaviour {

    public static WwiseManager Instance;

    public bool modifyBussesVolume = false;

    // modify the busses volumes
    [Range(0.0f, 100.0f)]
    public float volumeGlobal = 100f;
    [Range(0.0f, 100.0f)]
    public float volumeMusic = 100f;
    [Range(0.0f, 100.0f)]
    public float volumeVoice = 100f;
    [Range(0.0f, 100.0f)]
    public float volumeSFX = 100f;

    //----------------------------------------------------------------------------------------
    // start
    void Start()
    {
        Instance = this;
        loadWwiseBanks("Music");
        loadWwiseBanks("Ambiance");
        loadWwiseBanks("Car_Motor");
        

        // set the volume of Busses
        if (modifyBussesVolume)
        {
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_GLOBAL_VOL", volumeGlobal);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_MUSIC_VOL", volumeMusic);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_VOICES_VOL", volumeVoice);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_SFX_VOL", volumeSFX);
        }
        else
        {
           // volumeGlobal = 100f;
            volumeMusic = 100f;
            volumeVoice = 100f;
            volumeSFX = 100f;
        }
    }


    //----------------------------------------------------------------------------------------
    // update
    void Update()
    {
        // change masters
        if (modifyBussesVolume)
        {
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_GLOBAL_VOL", volumeGlobal);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_MUSIC_VOL", volumeMusic);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_VOICES_VOL", volumeVoice);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_SFX_VOL", volumeSFX);
        }
        else
        {

          //  AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_GLOBAL_VOL", 100f);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_MUSIC_VOL", 100f);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_VOICES_VOL", 100f);
            AkSoundEngine.SetRTPCValue("RTPC_BUS_MASTER_SFX_VOL", 100f);
        }
    }

    //---------------------------------------------------------------------------------------/
    //----------------------------------BANKS------------------------------------------------/
    //----------------------------------------------------------------------------------------
    // load a soundbank
    public void loadWwiseBanks(string wwiseBanks)
    {
        uint bankID; // Not used
        AkSoundEngine.LoadBank(wwiseBanks, AkSoundEngine.AK_DEFAULT_POOL_ID, out bankID);
    }
}
