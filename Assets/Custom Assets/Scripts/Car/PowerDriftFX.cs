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
				car=GetComponentInParent<ArcadeCarController>();
		}

		protected override void FixedUpdate()
		{

			PlayFX(car.IsDrifting);

		}

		protected override void RefreshParticleFX(ParticleSystem ps)
		{
			base.RefreshParticleFX(ps);
			
		}

	}

}
