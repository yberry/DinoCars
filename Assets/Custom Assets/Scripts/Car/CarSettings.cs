using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CND.Car
{
	[Serializable, CreateAssetMenu(fileName = "CarControllerSettings", menuName = "CND/Cars/Car Controller Settings")]
	public class CarSettings : ScriptableObject
	{
		[Header("Basic Settings")]
		[DisplayModifier( foldingMode: DM_FoldingMode.NoFoldout)]
		public ArcadeCarController.Settings preset;

    }

}
