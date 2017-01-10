
#if true
using UnityEngine;

public class MegaRopeSolver
{
	public virtual void doIntegration1(MegaRope rope, float dt)	{}
	public virtual void Solve() { }
}

public class MegaRopeSolverVertlet : MegaRopeSolver
{
	void doCalculateForces(MegaRope rope)
	{
		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			//rope.masses[i].force = rope.masses[i].mass * rope.gravity;

			rope.masses[i].force.x = rope.masses[i].mass * rope.gravity.x;
			rope.masses[i].force.y = rope.masses[i].mass * rope.gravity.y;
			rope.masses[i].force.z = rope.masses[i].mass * rope.gravity.z;


			//rope.masses[i].force += -rope.masses[i].vel * rope.airdrag;	// Should be vel sqr
			//rope.masses[i].force += rope.masses[i].forcec;
		}

		for ( int i = 0; i < rope.springs.Count; i++ )
			rope.springs[i].doCalculateSpringForce1(rope);
	}

	void DoConstraints(MegaRope rope)
	{
		for ( int i = 0; i < rope.iters; i++ )
		{
			for ( int c = 0; c < rope.constraints.Count; c++ )
			{
				rope.constraints[c].Apply(rope);	//this);
			}
		}
	}

	public override void doIntegration1(MegaRope rope, float dt)
	{
		doCalculateForces(rope);	// Calculate forces, only changes _f

		float t2 = dt * dt;
		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 last = rope.masses[i].pos;
			rope.masses[i].pos += rope.airdrag * (rope.masses[i].pos - rope.masses[i].last) + rope.masses[i].force * rope.masses[i].oneovermass * t2;	// * t;

			//masses[i].pos += airdrag * masses[i].pos - masses[i].last + masses[i].force * masses[i].oneovermass * t2;	// * t;

			//masses[i].pos = masses[i].pos + (masses[i].pos - masses[i].last) * (t / lastt) + masses[i].force * masses[i].oneovermass * t * t;
			rope.masses[i].vel = (rope.masses[i].pos - last) / dt;
			rope.masses[i].last = last;
		}

		if ( rope.SelfCollide )
			SelfCollide(rope);

		DoConstraints(rope);

		if ( rope.DoCollide )
			DoCollisions(rope, dt);

		if ( rope.SelfCollide )
			SelfCollide(rope);
	}

#if false
	void DoCollisions1(MegaRope rope, float dt)
	{
		RaycastHit hit;

		Vector3 dir = Vector3.zero;

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			if ( Mathf.Abs(rope.masses[i].vel.sqrMagnitude) < 0.0001f )
			{
				dir = Vector3.down;
			}
			else
				dir = rope.masses[i].vel.normalized;

			Vector3 start = rope.masses[i].pos - (dir * 10.0f);

			rope.masses[i].collide = false;
			if ( Physics.CheckSphere(rope.masses[i].pos, rope.radius, rope.layer) )
			{
				Collider[] cols = Physics.OverlapSphere(rope.masses[i].pos, rope.radius, rope.layer);

				for ( int c = 0; c < cols.Length; c++ )
				{
					Vector3 cp = cols[c].Raycast.ClosestPointOnBounds(rope.masses[i].pos);

				}


				if ( Physics.SphereCast(rope.masses[i].last, rope.radius, dir, out hit, (rope.masses[i].vel * dt).magnitude, rope.layer) )
				{
					if ( hit.distance < rope.radius )	//10.0f )
					{
						rope.masses[i].pos = hit.point + (hit.normal * (rope.radius * 1.001f));
						Response(i, hit, rope);
						rope.masses[i].collide = true;
					}
				}
			}
		}
	}
#endif

	void DoCollisions(MegaRope rope, float dt)
	{
		RaycastHit hit;

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 start = rope.masses[i].last - (Vector3.down * 10.0f);

			rope.masses[i].collide = false;
			//if ( Physics.CheckSphere(rope.masses[i].last, rope.radius, rope.layer) )
			if ( Physics.CheckSphere(rope.masses[i].pos, rope.radius, rope.layer.value) )
			{
				if ( Physics.SphereCast(start, rope.radius, Vector3.down, out hit, (rope.masses[i].vel.magnitude * dt) + 20.0f, rope.layer.value) )
				{
					if ( hit.distance < 10.0f )
					{
						rope.masses[i].pos = hit.point + (hit.normal * (rope.radius * 1.001f));
						Response(i, hit, rope);
						rope.masses[i].collide = true;
					}
				}
			}
		}
	}

	void Response(int i, RaycastHit hit, MegaRope rope)
	{
		// CALCULATE Vn
		float VdotN = Vector3.Dot(hit.normal, rope.masses[i].vel);
		Vector3 Vn = hit.normal * VdotN;
		// CALCULATE Vt
		//Vector3 Vt = (rope.masses[i].vel - Vn) * rope.floorfriction;
		// SCALE Vn BY COEFFICIENT OF RESTITUTION
		Vn *= rope.bounce;
		// SET THE VELOCITY TO BE THE NEW IMPULSE
		rope.masses[i].vel = Vn;	//Vt - Vn;

		rope.masses[i].last = rope.masses[i].pos;
	}

	// Dont need to self collide with n masses either side
	void SelfCollide2(MegaRope rope)
	{
		float rad = rope.radius * 2.0f;	//(rope.radius * rope.radius);
		rad *= rad;

		// Check for gaps betweem masses
		//for ( int i = 0; i < rope.masses.Count - 1; i++ )
		//{
		//	float dst = Vector3.Distance(rope.masses[i].pos, rope.masses[i + 1].pos);
		//	if ( dst > rad )
		//	{
		//		Debug.Log("gap " + i);
		//	}
		//}

		Vector3 delta = Vector3.zero;

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 p = rope.masses[i].pos;
			Vector3 lp = rope.masses[i].last;
			//float pspd = rope.masses[i].vel.magnitude;	//Vector3.Distance(p, lp);

			Vector3 delta1 = (p - lp).normalized;

			for ( int j = i + 2; j < rope.masses.Count; j++ )
			{
				Vector3 p1 = rope.masses[j].pos;

				delta.x = p1.x - p.x;
				delta.y = p1.y - p.y;
				delta.z = p1.z - p.z;

				float dsqr = delta.sqrMagnitude;	//Vector3.SqrMagnitude(rope.masses[j].pos - p);
				//float dsqr = Vector3.Magnitude(rope.masses[j].pos - p);

				if ( dsqr < rad )
				{
					float do1 = 1.0f / Mathf.Sqrt(dsqr);
					delta.x *= do1;
					delta.y *= do1;
					delta.z *= do1;


					//Debug.Log("Mass " + i + " hit mass " + j + " dist " + dsqr);
					//Vector3 delta = (rope.masses[j].pos - p).normalized;
					//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
					//p = mid - (delta * rope.radius);
					//rope.masses[i].pos = p;
					//rope.masses[j].pos = mid + (delta * rope.radius);

					//float hspd = Vector3.Distance(rope.masses[j].pos, rope.masses[j].vel;

					if ( rope.masses[i].collide )
					{
						rope.masses[j].pos = p + (delta * rad);
						rope.masses[j].last = p + (delta * rad);
					}
					else
					{
						if ( rope.masses[j].collide )
						{
							p = rope.masses[j].pos - (delta * rad);
							rope.masses[i].pos = p;
							rope.masses[i].last = p;
						}
						else
						{
							// back each mass back along its path until they dont touch
							Vector3 delta2 = (rope.masses[j].pos - rope.masses[j].last).normalized;

							int max = 8;
							while ( dsqr < rad )
							{
								float ds = (rad - dsqr);// * 0.5f;

								p -= delta1 * ds;

								rope.masses[j].pos -= delta2 * ds;
								dsqr = Vector3.Magnitude(rope.masses[j].pos - p);
								max--;
								if ( max < 0 )
									break;
							}


							//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
							//p = mid + (delta * rope.radius);
							rope.masses[i].pos = p;
							rope.masses[i].last = p;
							//p = mid - (delta * rope.radius);
							//rope.masses[i].pos = p;
							rope.masses[j].last = rope.masses[j].pos;
						}
					}
				}
			}
		}
	}

	void SelfCollide(MegaRope rope)
	{
		float rad = rope.radius * 2.0f;	//(rope.radius * rope.radius);

		// Check for gaps betweem masses
		//for ( int i = 0; i < rope.masses.Count - 1; i++ )
		//{
		//	float dst = Vector3.Distance(rope.masses[i].pos, rope.masses[i + 1].pos);
		//	if ( dst > rad )
		//	{
		//		Debug.Log("gap " + i);
		//	}
		//}

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 p = rope.masses[i].pos;
			//Vector3 lp = rope.masses[i].last;
			//float pspd = rope.masses[i].vel.magnitude;	//Vector3.Distance(p, lp);

			for ( int j = i + 2; j < rope.masses.Count; j++ )
			{
				//float dsqr = Vector3.SqrMagnitude(rope.masses[j].pos - p);
				float dsqr = Vector3.Magnitude(rope.masses[j].pos - p);

				if ( dsqr < rad )
				{
					//Debug.Log("Mass " + i + " hit mass " + j + " dist " + dsqr);
					Vector3 delta = (rope.masses[j].pos - p).normalized;
					//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
					//p = mid - (delta * rope.radius);
					//rope.masses[i].pos = p;
					//rope.masses[j].pos = mid + (delta * rope.radius);

					//float hspd = Vector3.Distance(rope.masses[j].pos, rope.masses[j].vel;

					if ( rope.masses[i].collide )
					{
						rope.masses[j].pos = p + (delta * rad);
						rope.masses[j].last = p + (delta * rad);
					}
					else
					{
						if ( rope.masses[j].collide )
						{
							p = rope.masses[j].pos - (delta * rad);
							rope.masses[i].pos = p;
							rope.masses[i].last = p;
						}
						else
						{
							Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
							p = mid + (delta * rope.radius);
							rope.masses[j].pos = p;
							rope.masses[j].last = p;
							p = mid - (delta * rope.radius);
							rope.masses[i].pos = p;
							rope.masses[i].last = p;
						}
					}
				}
			}
		}
	}


	void SelfCollide1(MegaRope rope)
	{
		float rad = rope.radius * 2.0f;	//(rope.radius * rope.radius);

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 p = rope.masses[i].pos;
			Vector3 lp = rope.masses[i].last;
			if ( Vector3.Distance(p, lp) > rad )
			{
				Debug.Log("Mass " + i + " moved too much");
			}

			for ( int j = i + 2; j < rope.masses.Count; j++ )
			{
				//float dsqr = Vector3.SqrMagnitude(rope.masses[j].pos - p);
				float dsqr = Vector3.Magnitude(rope.masses[j].pos - p);

				if ( dsqr < rad )
				{
					//Debug.Log("Mass " + i + " hit mass " + j + " dist " + dsqr);
					Vector3 delta = (rope.masses[j].pos - p).normalized;
					//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
					//p = mid - (delta * rope.radius);
					//rope.masses[i].pos = p;
					//rope.masses[j].pos = mid + (delta * rope.radius);

					if ( p.y > rope.masses[j].pos.y )
					{
						//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
						p = rope.masses[j].pos - (delta * rad);
						rope.masses[i].pos = p;
						rope.masses[i].last = p;
					}
					else
					{
						//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
						//p = p - (delta * rad);
						rope.masses[j].pos = p + (delta * rad);
						rope.masses[j].last = p + (delta * rad);
					}
				}
			}
		}
	}

}


public class MegaRopeSolverDefault : MegaRopeSolver
{
	void doCalculateForces(MegaRope rope)
	{
		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			rope.masses[i].force = rope.masses[i].mass * rope.gravity;
			rope.masses[i].force += -rope.masses[i].vel * rope.airdrag;	// Should be vel sqr
		}

		for ( int i = 0; i < rope.springs.Count; i++ )
			rope.springs[i].doCalculateSpringForce(rope);
	}

	public override void doIntegration1(MegaRope rope, float dt)
	{
		doCalculateForces(rope);	// Calculate forces, only changes _f

		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			rope.masses[i].last = rope.masses[i].pos;
			rope.masses[i].vel += dt * rope.masses[i].force * rope.masses[i].oneovermass;
			rope.masses[i].pos += rope.masses[i].vel * dt;
			rope.masses[i].vel *= rope.friction;
		}

		DoConstraints(rope);
		if ( rope.DoCollide )
			DoCollisions(rope, dt);

		if ( rope.SelfCollide )
			SelfCollide(rope);
	}

	void DoCollisions(MegaRope rope, float dt)
	{
		RaycastHit hit;

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 start = rope.masses[i].last - (Vector3.down * 10.0f);

			if ( Physics.CheckSphere(rope.masses[i].last, rope.radius, rope.layer.value) )
			{
				if ( Physics.SphereCast(start, rope.radius, Vector3.down, out hit, (rope.masses[i].vel.magnitude * dt) + 20.0f, rope.layer.value) )
				{
					if ( hit.distance < 10.0f )
					{
						rope.masses[i].pos = hit.point + (hit.normal * (rope.radius * 1.001f));
						Response(i, hit, rope);
					}
				}
			}
		}
	}

	void DoCollisions1(MegaRope rope, float dt)
	{
		RaycastHit hit;

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			//Vector3 start = rope.masses[i].pos - (rope.masses[i].vel * 10.0f);

			if ( Physics.CheckSphere(rope.masses[i].pos, rope.radius, rope.layer.value) )
			{
				if ( Physics.SphereCast(rope.masses[i].last, rope.radius, rope.masses[i].vel.normalized, out hit, (rope.masses[i].vel.magnitude * dt) * 2.0f, rope.layer.value) )
				{
					//Debug.Log("d " + hit.distance);
					//if ( hit.distance < 10.01f )
					{
						rope.masses[i].pos = hit.point + (hit.normal * (rope.radius * 1.05f));
						Response(i, hit, rope);
					}
				}
			}
		}
	}

	// Satisfy constraints
	void DoConstraints(MegaRope rope)
	{
		for ( int i = 0; i < rope.iters; i++ )
		{
			for ( int c = 0; c < rope.constraints.Count; c++ )
			{
				rope.constraints[c].Apply(rope);	//this);
			}
		}
	}

	void Response(int i, RaycastHit hit, MegaRope rope)
	{
		// CALCULATE Vn
		float VdotN = Vector3.Dot(hit.normal, rope.masses[i].vel);
		Vector3 Vn = hit.normal * VdotN;
		// CALCULATE Vt
		//Vector3 Vt = (rope.masses[i].vel - Vn) * rope.floorfriction;
		// SCALE Vn BY COEFFICIENT OF RESTITUTION
		Vn *= rope.bounce;
		// SET THE VELOCITY TO BE THE NEW IMPULSE
		rope.masses[i].vel = Vn;	//Vt - Vn;
	}


	void SelfCollide(MegaRope rope)
	{
		float rad = rope.radius * 2.0f;	//(rope.radius * rope.radius);

		for ( int i = 0; i < rope.masses.Count; i++ )
		{
			Vector3 p = rope.masses[i].pos;

			for ( int j = i + 2; j < rope.masses.Count; j++ )
			{
				//float dsqr = Vector3.SqrMagnitude(rope.masses[j].pos - p);
				float dsqr = Vector3.Magnitude(rope.masses[j].pos - p);

				if ( dsqr < rad )
				{
					//Debug.Log("Mass " + i + " hit mass " + j + " dist " + dsqr);
					Vector3 delta = (rope.masses[j].pos - p).normalized;
					//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
					//p = mid - (delta * rope.radius);
					//rope.masses[i].pos = p;
					//rope.masses[j].pos = mid + (delta * rope.radius);

					if ( p.y > rope.masses[j].pos.y )
					{
						//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
						p = rope.masses[j].pos - (delta * rad);
						rope.masses[i].pos = p;
						//rope.masses[i].last = p;
					}
					else
					{
						//Vector3 mid = (p + rope.masses[j].pos) * 0.5f;	//(delta * 0.5f);
						//p = p - (delta * rad);
						rope.masses[j].pos = p + (delta * rad);
						//rope.masses[j].last = p + (delta * rad);
					}
				}
			}
		}
	}

	void Collide(int i, float dt, MegaRope rope)
	{
		return;
#if false
		RaycastHit hit;

		//if ( Physics.SphereCast(masses[i].pos, ropeRadius, Vector3.up, out hit, 0.0f, layer) )
		//{
		//masses[i].pos = hit.point + (hit.normal * (ropeRadius * 1.01f));
		//Response(i, hit);
		//}
		Vector3 dir = masses[i].vel.normalized;
		Vector3 start = masses[i].last - (Vector3.down * 10.0f);

		//if ( Physics.SphereCast(masses[i].last, ropeRadius, dir, out hit, masses[i].vel.magnitude * dt, layer) )
		//if ( Physics.SphereCast(start, ropeRadius, dir, out hit, (masses[i].vel.magnitude * dt) + 10.0f, layer) )
		if ( Physics.SphereCast(start, ropeRadius, Vector3.down, out hit, (masses[i].vel.magnitude * dt) + 20.0f, layer) )
		{
			if ( hit.distance < 10.0f )
			{
				masses[i].pos = hit.point + (hit.normal * (ropeRadius * 1.01f));
				Response(i, hit);
			}
		}
#endif
	}

	void CollideCapsule(int i, float dt, MegaRope rope)
	{
		if ( i < rope.masses.Count - 1 )
		{
			// cast a ray from last pos to current against colliders
			RaycastHit hit;

			if ( Physics.CapsuleCast(rope.masses[i].pos, rope.masses[i + 1].pos, rope.radius, rope.masses[i].vel.normalized, out hit, 0.1f, rope.layer.value) )
			{
				rope.masses[i].pos = hit.point + (hit.normal * (rope.radius * 1.01f));
				//masses[i + 1].pos = hit.point + (hit.normal * (ropeRadius * 1.01f));
				Response(i, hit, rope);
			}
		}
	}
}
#endif