
using UnityEngine;
using UnityEditor;

public class MegaCreateLayerWizard : ScriptableWizard
{
	public MegaLoftType	lofttype = MegaLoftType.Simple;

	public bool enabled = true;

	//[MenuItem ("GameObject/Create Light Wizard")]
	static public void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<MegaCreateLayerWizard>("Create Layer", "Create", "Cancel");
		//If you don't want to use the secondary button simply leave it out:
		//ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
	}

	void OnWizardCreate()
	{
		//GameObject go = new GameObject ("New Light");
		//go.AddComponent("Light");
		//go.light.range = range;
		//go.light.color = color;
	}

	void OnWizardUpdate()
	{
		helpString = "Please set the color of the light!";
	}

	// When the user pressed the "Apply" button OnWizardOtherButton is called.
	void OnWizardOtherButton()
	{
	}

	void OnGUI()
	{
		lofttype = (MegaLoftType)EditorGUILayout.EnumPopup("Type", lofttype);
	}
}
