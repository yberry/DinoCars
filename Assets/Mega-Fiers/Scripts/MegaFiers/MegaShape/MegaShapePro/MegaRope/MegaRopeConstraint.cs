
#if true
using UnityEngine;

public class MegaRopeConstraint
{
	public bool active;
	public virtual void Apply(MegaRope soft)
	{

	}
}

public class PointCurveConstraint : MegaRopeConstraint
{
	public int			p1;
	public MegaShape	shape;
	public float		alpha;

	public override void Apply(MegaRope soft)
	{
		// If alpha < 0.0f or > 1.0f then constraint is inactive
		// If shape is closed?
		if ( shape != null )
		{
			Vector3 pos = shape.InterpCurve3D(0, alpha, true);
			soft.masses[p1].pos = pos;
		}
	}
}

public class LengthConstraint : MegaRopeConstraint
{
	public int		p1;
	public int		p2;
	public float	length;
	Vector3 moveVector = Vector3.zero;

	public LengthConstraint(int _p1, int _p2, float _len)
	{
		p1 = _p1;
		p2 = _p2;
		length = _len;
		active = true;
	}

	public override void Apply(MegaRope soft)
	{
		if ( active )
		{
			//calculate direction
			//Vector3 direction = soft.masses[p2].pos - soft.masses[p1].pos;

			moveVector.x = soft.masses[p2].pos.x - soft.masses[p1].pos.x;
			moveVector.y = soft.masses[p2].pos.y - soft.masses[p1].pos.y;
			moveVector.z = soft.masses[p2].pos.z - soft.masses[p1].pos.z;
			//calculate current length
			//float currentLength = direction.magnitude;

			//check for zero vector
			//if ( direction != Vector3.zero )
			//if ( direction.x != 0.0f || direction.y != 0.0f || direction.z != 0.0f )
			if ( moveVector.x != 0.0f || moveVector.y != 0.0f || moveVector.z != 0.0f )
			{
				float currentLength = moveVector.magnitude;

				float do1 = 1.0f / currentLength;
				//normalize direction vector
				//direction.Normalize();

				//move to goal positions
				//Vector3 moveVector = 0.5f * (currentLength - length) * direction;

				float l = 0.5f * (currentLength - length) * do1;
				moveVector.x *= l;	// * direction.x;
				moveVector.y *= l;	// * direction.y;
				moveVector.z *= l;	// * direction.z;

				soft.masses[p1].pos.x += moveVector.x;
				soft.masses[p1].pos.y += moveVector.y;
				soft.masses[p1].pos.z += moveVector.z;

				soft.masses[p2].pos.x -= moveVector.x;
				soft.masses[p2].pos.y -= moveVector.y;
				soft.masses[p2].pos.z -= moveVector.z;

				//soft.masses[p1].pos += moveVector;
				//soft.masses[p2].pos += -moveVector;

				//soft.masses[p1].last = soft.masses[p1].pos;
				//soft.masses[p2].last = soft.masses[p2].pos;
			}
		}
	}
}

public class PointConstraint : MegaRopeConstraint
{
	public int			p1;
	public Transform	obj;

	public PointConstraint(int _p1, Transform trans)
	{
		p1 = _p1;
		obj = trans;
		active = true;
	}

	public override void Apply(MegaRope soft)
	{
		if ( active )
			soft.masses[p1].pos = obj.position;
	}
}

public class NewPointConstraint : MegaRopeConstraint
{
	public int			p1;
	public int			p2;
	public float		length;
	public Transform	obj;

	public NewPointConstraint(int _p1, int _p2, float _len, Transform trans)
	{
		p1 = _p1;
		p2 = _p2;
		length = _len;
		obj = trans;
		active = true;
	}

	public override void Apply(MegaRope soft)
	{
		if ( active )
		{
			//calculate direction
			soft.masses[p1].pos = obj.position;
			soft.masses[p1].last = soft.masses[p1].pos;
			Vector3 direction = soft.masses[p2].pos - soft.masses[p1].pos;

			//calculate current length
			float currentLength = direction.magnitude;

			//check for zero vector
			if ( direction != Vector3.zero )
			{
				//normalize direction vector
				direction.Normalize();

				//move to goal positions
				Vector3 moveVector = 1.0f * (currentLength - length) * direction;
				//soft.masses[p1].pos += moveVector;
				soft.masses[p2].pos += -moveVector;
				soft.masses[p2].last = soft.masses[p2].pos;
			}
		}
	}
}

public class PointConstraint1 : MegaRopeConstraint
{
	public int			p1;
	public Vector3	off;
	public Transform obj;

	public override void Apply(MegaRope soft)
	{
		if ( active )
			soft.masses[p1].pos = obj.position + obj.localToWorldMatrix.MultiplyVector(off);
	}
}

// Could use Length to do this
public class AngleConstraint : MegaRopeConstraint
{
	public int p1;
	public int p2;
	public int p3;
	public float angle;

	public override void Apply(MegaRope soft)
	{
		//Vector3 d1 = soft.masses[p1].pos - soft.masses[p2].pos;
		//Vector3 d2 = soft.masses[p3].pos - soft.masses[p2].pos;

		//float ang = Vector3.Dot(d1, d2);
	}
}
#endif

public class RBodyConstraint : MegaRopeConstraint
{
	public Rigidbody rbody;
	public float alpha = 0.0f;

	public override void Apply(MegaRope soft)
	{
		// find distance bewteen attach point on rbody and place on rope, and apply a spring force to the rbody and to the masses
	}
}