using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class PowerDriftFX : SkidFX
	{
		public ArcadeCarController car;

		protected override void Start()
		{
			base.Start();
			if (!car)
				car = GetComponentInParent<ArcadeCarController>();
		}

		protected override void FixedUpdate()
		{
			PlayFX(car.IsDrifting);

		}


		protected override void RefreshParticleFX(ParticleSystem ps)
		{
			//base.RefreshParticleFX(ps);
			ps.transform.position = wheel.contactInfo.hit.point;
			float velMag = Mathf.Clamp(wheel.contactInfo.rootVelocity.magnitude, 0, 10);
			float sideFriction = Mathf.Abs(wheel.contactInfo.sidewaysRatio.Cubed());

			var main = ps.main;
			var startCol = main.startColor;
			var col = startCol.color;
			col.a = Mathf.Clamp01(velMag * sideFriction * 0.333f);
			startCol = col;
			main.startColor = startCol;
		}

	}

}
