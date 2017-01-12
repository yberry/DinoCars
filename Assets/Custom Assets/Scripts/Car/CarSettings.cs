using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CND.Car
{
	[Serializable, CreateAssetMenu(fileName = "CarControllerSettings", menuName = "CND/Cars/Car Controller Settings")]
	public class CarSettings : ScriptableObject
	{
		[Header("Basic Settings")]
		[DisplayModifier( foldingMode: DM_FoldingMode.Unparented)]
		public ArcadeCarController.Settings preset;

	}
}
