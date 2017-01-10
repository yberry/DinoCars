using UnityEngine;
using System.Collections;


public static class Globals {
#if PROJECT_NOVA
	public static readonly int MaxPlayers = 4;
    public const string ProjectFolderName = "Nova";

#elif PROJECT_1SP2
    //fallback
    public static readonly int MaxPlayers = 2;
    public static readonly string ProjectFolderName = "1SP2";
#else
    public static readonly int MaxPlayers = 4;
    public const  string ProjectFolderName = "ProjectName";
#endif

	public const string AssetFolder = ProjectFolderName + " Assets/";//{ get { return ProjectFolderName + " Assets/"; } }
	public const string PrefabFolder = "Prefabs/";// {  get { return "Prefabs/"; } }
	public const string SceneFolder = "Scenes/";// {  get { return "Scenes/"; } }
	public const string GameplayPrefabFolder = PrefabFolder + "Gameplay/";
	public const string UIPrefabFolder = PrefabFolder + "UI/";
	public const string FXPrefabFolder = PrefabFolder + "FX/";
	public const string CharacterPrefabFolder = PrefabFolder + "Characters/";

	public const string PizzaPrefab = GameplayPrefabFolder + "Pizza";//{ get { return PrefabFolder +"Pizza"; } }
	public const string PlayerPrefab = GameplayPrefabFolder + "Player";//{ get { return PrefabFolder + "Player"; } }

	public const string MaterialFolder = "Materials/";//{ get { return "Materials/"; } }
	
	public static readonly string Axis_X1 = "AnalogX1";  //leftstick X
    public static readonly string Axis_Y1 = "AnalogY1";    //leftstick Y

    public static readonly string Axis_X2 = "AnalogX2";  //rightstick X
    public static readonly string Axis_Y2 = "AnalogY2";    //rightstick Y

    public static readonly string Axis_Z = "AnalogZ";       //triggers 
    public static readonly string Axis_Z1 = "AnalogZ1";       //triggers 
    public static readonly string Axis_Z2 = "AnalogZ2";       //triggers 
    public static readonly string Axis_ZPos = "AnalogZ+";
    public static readonly string Axis_ZNeg = "AnalogZ-";

    public static readonly string BtnAction1 = "Action1";   //A
    public static readonly string BtnAction2 = "Action2";   //B
    public static readonly string BtnAction3 = "Action3";   //X
    public static readonly string BtnAction4 = "Action4";   //Y

    public static readonly string BtnAction5 = "Action5";   //L
    public static readonly string BtnAction6 = "Action6";   //R

    public static readonly string BtnAction7 = "Action7";   //LStick
    public static readonly string BtnAction8 = "Action8";   //RStick

    public static readonly string BtnUp = "D-Up";   
    public static readonly string BtnDown = "D-Down"; 
    public static readonly string BtnLeft = "D-Left";  
    public static readonly string BtnRight = "D-right";

    public static readonly string BtnStart = "Start";   //LStick
    public static readonly string BtnBack = "Back";   //RStick

}
