
#if true
using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public enum MegaLoftColMode
{
	Loft,
	Path,
}

[System.Serializable]
public class MegaMaterialSection
{
	public int				id;
	public Material			mat;
	public Vector2			UVOffset		= Vector2.zero;
	public Vector2			UVRotate		= Vector2.zero;
	public Vector2			UVScale			= Vector2.one;
	public bool				includeknots	= true;
	public bool				swapuv			= true;
	public bool				physuv			= true;
	public bool				uvcalcy			= false;
	public float			cdist			= 1.0f;
	public int				steps			= 1;
	public bool				show			= false;
	public bool				Enabled			= true;
	public MegaLoftColMode	colmode			= MegaLoftColMode.Path;
	public float			coloffset		= 0.0f;
	public Color			color			= Color.white;
	public AnimationCurve	colR			= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colG			= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colB			= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colA			= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public float			vertscale = 1.0f;
	public bool				collider = false;
}

[System.Serializable]
public class MegaMeshSection
{
	public int				mat;
	public int				vertstart = 0;
	public List<Vector3>	verts	= new List<Vector3>();
	public List<Vector3>	verts1	= new List<Vector3>();
	public List<Vector2>	uvs		= new List<Vector2>();
	public List<Vector3>	cverts	= new List<Vector3>();
	public List<Vector2>	cuvs	= new List<Vector2>();
	public List<Color>		ccols	= new List<Color>();
	public List<Color>		cols	= new List<Color>();
	public List<Vector3>	cnorms	= new List<Vector3>();
	public List<Vector3>	norms	= new List<Vector3>();
	public List<int>		tris	= new List<int>();
	public float			castart = 0.0f;
	public float			caend	= 1.0f;
	public float[]			offsets;
	public float[]			last;
	public int				firstknot;
	public int				lastknot;
	public float			len;
}

public class MegaLoftLayerMultiMat : MegaLoftLayerSimple
{
	public List<MegaMaterialSection>	sections = new List<MegaMaterialSection>();
	public List<MegaMeshSection>	meshsections = new List<MegaMeshSection>();

	public override int GetHelp()
	{
		return 5218;
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		for ( int i = 0; i < meshsections.Count; i++ )
		{
			MegaMeshSection ms = meshsections[i];
			Array.Copy(ms.verts.ToArray(), 0, verts, offset, ms.verts.Count);
			Array.Copy(ms.uvs.ToArray(), 0, uvs, offset, ms.uvs.Count);
			offset += ms.verts.Count;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		for ( int i = 0; i < meshsections.Count; i++ )
		{
			MegaMeshSection ms = meshsections[i];
			Array.Copy(ms.verts.ToArray(), 0, verts, offset, ms.verts.Count);
			Array.Copy(ms.uvs.ToArray(), 0, uvs, offset, ms.uvs.Count);
			Array.Copy(ms.cols.ToArray(), 0, cols, offset, ms.cols.Count);
			offset += ms.verts.Count;
		}
	}

	public override int NumVerts()
	{
		int num = 0;

		for ( int i = 0; i < meshsections.Count; i++ )
			num += meshsections[i].cverts.Count * crosses;

		return num;
	}

	public override Material GetMaterial(int i)
	{
		if ( i >= 0 && i < meshsections.Count )
		{
			int id = meshsections[i].mat;

			if ( id >= 0 && id < sections.Count )
				return sections[meshsections[i].mat].mat;
		}

		return null;
	}

	public override int[] GetTris(int i)
	{
		return meshsections[i].tris.ToArray();	//.tris;
	}

	public override int NumMaterials()
	{
		return meshsections.Count;
	}

	public override bool Valid()
	{
		if ( LayerEnabled && layerSection && layerPath && layerSection.splines != null && layerPath.splines != null && sections.Count > 0 )
		{
			if ( curve < 0 ) curve = 0;

			if ( curve > layerPath.splines.Count - 1 )
				curve = layerPath.splines.Count - 1;

			if ( crosscurve < 0 ) crosscurve = 0;

			if ( crosscurve > layerSection.splines.Count - 1 )
				crosscurve = layerSection.splines.Count - 1;

			return true;
		}

		return false;
	}

	int GetIndexInterp(float val, ref float interp)
	{
		int cindex = (int)val;
		interp = val - cindex;

		return cindex;
	}

	Vector3 GetCross(MegaShapeLoft loft, float ca)
	{
		MegaShape	section = layerSection;

		Matrix4x4 tm1 = Matrix4x4.identity;
		MegaMatrix.Translate(ref tm1, pivot);
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * crossRot.x, Mathf.Deg2Rad * crossRot.y, Mathf.Deg2Rad * crossRot.z));

		int lk	= -1;

		float alpha = crossStart + ca;

		MegaSpline cspl = section.splines[crosscurve];

		if ( cspl.closed )
		{
			if ( alpha < 0.0f )
				alpha += 1.0f;
		}

		Vector3 off = Vector3.zero;
		if ( snap )
			off = section.splines[0].knots[0].p - cspl.knots[0].p;

		if ( cspl.closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

		return tm1.MultiplyPoint3x4(section.splines[crosscurve].InterpCurve3D(alpha, section.normalizedInterp, ref lk) + off);
	}

	// This will need changing
	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		// first need to find the material section we are in from ca
		MegaMeshSection ms = null;
		for ( int i = 0; i < meshsections.Count; i++ )
		{
			ms = meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		// Ok got the section
		float findex = (ms.cverts.Count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		bool flip = false;
		int pindex;
		int pindex1;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pindex1 = 1;
				pinterp = a * GetLength(loft);	// / crosses);
			}
			else
			{
				if ( a >= 0.9999f )
				{
					pindex = crosses - 1;
					pindex1 = crosses - 2;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;
				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;
			pindex1 = pindex + 1;
		}

		Vector3 p1 = ms.verts[(pindex * ms.cverts.Count) + cindex];
		Vector3 p2 = ms.verts[(pindex * ms.cverts.Count) + cindex1];
		Vector3 p3 = ms.verts[(pindex1 * ms.cverts.Count) + cindex];
		Vector3 p4 = ms.verts[(pindex1 * ms.cverts.Count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;
		if ( flip )
		{
			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

	// If beyond the start or end then need to extrapolate last or first rows
	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		MegaMeshSection ms = null;

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			ms = meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		float findex = (ms.cverts.Count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;
		int cindex1 = cindex + 1;

		bool flip = false;
		int pindex;
		int pindex1;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pindex1 = 1;
				pinterp = a * GetLength(loft);	// / crosses);
			}
			else
			{
				if ( a >= 0.9999f )
				{
					pindex = crosses - 1;
					pindex1 = crosses - 2;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;
				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;
			pindex1 = pindex + 1;
		}

		Vector3 p1 = ms.verts[(pindex * ms.cverts.Count) + cindex];
		Vector3 p2 = ms.verts[(pindex * ms.cverts.Count) + cindex1];
		Vector3 p3 = ms.verts[(pindex1 * ms.cverts.Count) + cindex];
		Vector3 p4 = ms.verts[(pindex1 * ms.cverts.Count) + cindex1];

		float pa = pinterp + at;

		p1.x = p1.x + (p2.x - p1.x) * interp;
		p1.y = p1.y + (p2.y - p1.y) * interp;
		p1.z = p1.z + (p2.z - p1.z) * interp;

		p2.x = p3.x + (p4.x - p3.x) * interp;
		p2.y = p3.y + (p4.y - p3.y) * interp;
		p2.z = p3.z + (p4.z - p3.z) * interp;

		if ( flip )
		{
			p2.x = p1.x - p2.x;
			p2.y = p1.y - p2.y;
			p2.z = p1.z - p2.z;
		}
		else
		{
			p2.x = p2.x - p1.x;
			p2.y = p2.y - p1.y;
			p2.z = p2.z - p1.z;
		}

		p3.x = p1.x + (p2.x * pinterp);
		p3.y = p1.y + (p2.y * pinterp);
		p3.z = p1.z + (p2.z * pinterp);

		p.x = p1.x + (p2.x * pa);
		p.y = p1.y + (p2.y * pa);
		p.z = p1.z + (p2.z * pa);

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

	// Do these from sample splines now?
	public override Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		MegaMeshSection ms = null;

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			ms = meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//ca = Mathf.Repeat(ca, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		float findex = (float)(ms.cverts.Count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = ms.verts[(pindex * ms.cverts.Count) + cindex];
		Vector3 p2 = ms.verts[(pindex * ms.cverts.Count) + cindex1];
		Vector3 p3 = ms.verts[(pindex1 * ms.cverts.Count) + cindex];
		Vector3 p4 = ms.verts[(pindex1 * ms.cverts.Count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);
		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

		up = Vector3.Cross(n1, n2).normalized;

		if ( flip )
		{
			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);

			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);

			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}

		return p3;
	}

	// Return angles to allow calc up to work
	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		MegaMeshSection ms = null;

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			ms = meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		float findex = (float)(ms.cverts.Count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;
		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		int ix = (pindex * ms.cverts.Count) + cindex;

		if ( ix < 0 || ix >= ms.verts.Count )
		{
			Debug.Log("pindex " + pindex + " cindex " + cindex + " a " + a);
			Debug.Log("ix " + ix + " verts " + ms.verts.Count);
		}

		Vector3 p1 = ms.verts[(pindex * ms.cverts.Count) + cindex];
		Vector3 p2 = ms.verts[(pindex * ms.cverts.Count) + cindex1];
		Vector3 p3 = ms.verts[(pindex1 * ms.cverts.Count) + cindex];
		Vector3 p4 = ms.verts[(pindex1 * ms.cverts.Count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

		right = n1.normalized;
		fwd = n2.normalized;
		up = Vector3.Cross(n1, n2).normalized;

		if ( flip )
		{
			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);

			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);
		}
		else
		{
			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);

			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);
		}

		return p3;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	public override Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		MegaMeshSection ms = null;

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			ms = meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		float findex = (ms.cverts.Count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = ms.verts[(pindex * ms.cverts.Count) + cindex];
		Vector3 p2 = ms.verts[(pindex * ms.cverts.Count) + cindex1];
		Vector3 p3 = ms.verts[(pindex1 * ms.cverts.Count) + cindex];
		Vector3 p4 = ms.verts[(pindex1 * ms.cverts.Count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);
		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		// Quick calc of face normal
		up = Vector3.Lerp(ms.cnorms[cindex], ms.cnorms[cindex1], interp);

		if ( flip )
		{
			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);

			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);
		}
		else
		{
			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);

			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);
		}

		return p3;
	}

	public int FindMaterial(int id)
	{
		for ( int i = 0; i < sections.Count; i++ )
		{
			if ( sections[i].id == id )
				return i;
		}

		return 0;
	}

	// Get the different id sections
	void FindSections(MegaSpline spline)
	{
		if ( spline != null )
		{
			meshsections.Clear();

			int id = spline.knots[0].id - 1;

			for ( int i = 0; i < spline.knots.Count; i++ )
			{
				if ( spline.knots[i].id != id )
				{
					id = spline.knots[i].id;
					MegaMeshSection msect = new MegaMeshSection();

					msect.mat = FindMaterial(id);

					meshsections.Add(msect);
				}
			}
		}
	}

	// Have cdist per material
	// Best way is to treat each material section like a layer
	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		MegaShape	section = layerSection;

		// Look at section and find out how many changes in material there are, ie matid
		// That will be the number of materials
		// Each change will have its own cdist, uv mapping, material

		// We seem to do this below as well
		FindSections(section.splines[crosscurve]);

		Matrix4x4 tm1 = Matrix4x4.identity;
		MegaMatrix.Translate(ref tm1, pivot);
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * crossRot.x, Mathf.Deg2Rad * crossRot.y, Mathf.Deg2Rad * crossRot.z));

		// Build the cross for each section
		if ( enabled )
		{
			float dst = crossDist;

			if ( dst < 0.01f )
				dst = 0.01f;

			int k	= -1;
			int lk	= -1;

			svert = verts.Count;
			Vector2 uv = Vector2.zero;

			float alpha = crossStart;
			float cend = crossStart + crossEnd;

			if ( alpha < 0.0f )
				alpha = 0.0f;

			if ( cend > 1.0f )
				cend = 1.0f;

			MegaSpline cspl = section.splines[crosscurve];

			if ( cspl.closed )
			{
				if ( alpha < 0.0f )
					alpha += 1.0f;
			}

			Vector3 off = Vector3.zero;
			if ( snap )
				off = cspl.knots[0].p - cspl.knots[0].p;

			if ( cspl.closed )
				alpha = Mathf.Repeat(alpha, 1.0f);

			meshsections.Clear();

			Vector3 pos = tm1.MultiplyPoint3x4(cspl.InterpCurve3D(alpha, section.normalizedInterp, ref lk) + off);

			int currentid = cspl.knots[lk].id;
			MegaMeshSection msect = new MegaMeshSection();

			msect.castart = 0.0f;
			float uvalpha = crossStart;
			uv.y = uvalpha;	//crossStart;

			msect.vertstart = 0;
			msect.mat = FindMaterial(currentid);
			meshsections.Add(msect);

			// Method to get cdist
			MegaMaterialSection ms = sections[msect.mat];

			msect.cverts.Add(pos);

			bool loop = true;
			//float lastalpha = alpha;
			while ( alpha <= cend )
			{
				if ( loop )
				{
					alpha += ms.cdist;
					loop = false;
				}
				if ( alpha > cend )
					alpha = cend;

				pos = tm1.MultiplyPoint3x4(cspl.InterpCurve3D(alpha, section.normalizedInterp, ref k) + off);

				if ( k != lk )
				{
					while ( lk != k )
					{
						//bool looped = false;
						lk++;
						int lk1 = lk % cspl.knots.Count;
						lk = lk1;

						if ( cspl.knots[lk].id != currentid )
						{
							msect.cverts.Add(tm1.MultiplyPoint3x4(cspl.knots[lk].p + off));
							float caend = ((cspl.knots[lk].length / cspl.length) - crossStart) / (crossEnd - crossStart);
							msect.caend = caend;

							// New material
							msect = new MegaMeshSection();
							msect.castart = caend;

							pos = tm1.MultiplyPoint3x4(cspl.knots[lk].p + off);

							msect.vertstart = 0;
							currentid = cspl.knots[lk].id;
							msect.mat = FindMaterial(currentid);
							meshsections.Add(msect);
							ms = sections[msect.mat];

							msect.cverts.Add(pos);
							lk1 = lk - 1;
							if ( lk1 < 0 )
								lk1 = cspl.knots.Count - 1;

							alpha = (cspl.knots[lk1].length / cspl.length);	// + crossStart;

							break;
						}
						else
						{
							if ( ms.includeknots )
								msect.cverts.Add(tm1.MultiplyPoint3x4(cspl.knots[lk].p + off));
							else
							{
								if ( cspl.knots[k].id != currentid )
								{
									int kk = lk;
									while ( cspl.knots[kk].id == currentid && kk < cspl.knots.Count )
									{
										kk++;
									}
									if ( kk >= cspl.knots.Count )
										kk = cspl.knots.Count - 1;

									msect.cverts.Add(tm1.MultiplyPoint3x4(cspl.knots[kk].p + off));
									break;
								}
								else
									msect.cverts.Add(pos);
							}
						}
					}
				}
				else
					msect.cverts.Add(pos);

				if ( alpha == cend )
				{
					if ( msect.cverts[msect.cverts.Count - 1] != pos )
						msect.cverts.Add(pos);

					break;
				}

				alpha += ms.cdist;
				if ( alpha > cend )
					alpha = cend;
			}

			msect.caend = 1.0f;

			// Do uv and col now
			for ( int i = 0; i < meshsections.Count; i++ )
			{
				MegaMeshSection ms1 = meshsections[i];

				Vector2 uv1 = Vector2.zero;
				ms1.cuvs.Add(uv1);
				ms1.ccols.Add(Color.white);

				for ( int v = 1; v < ms1.cverts.Count; v++ )
				{
					uv1.y += Vector3.Distance(ms1.cverts[v], ms1.cverts[v - 1]);
					ms1.cuvs.Add(uv1);
					ms1.ccols.Add(Color.white);
				}
			}

			// Add end point
			if ( section.splines[crosscurve].closed )
				alpha = Mathf.Repeat(cend, 1.0f);
		}

		// Calc normals
		Vector3 up = Vector3.zero;
		Vector3 n1 = Vector3.zero;

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			MegaMeshSection ms1 = meshsections[i];

			if ( ms1.cverts.Count > 1 )
			{
				for ( int v = 0; v < ms1.cverts.Count; v++ )
				{
					if ( v < ms1.cverts.Count - 1 )
						n1 = (ms1.cverts[v + 1] - ms1.cverts[v]);
					else
						n1 = (ms1.cverts[v] - ms1.cverts[v - 1]);

					up.x = -n1.y;
					up.y = n1.x;
					up.z = 0.0f;
					ms1.cnorms.Add(up);
				}
			}
		}

		Prepare(loft);

		return true;
	}

	// Dont need this as a seperate method me thinks
	void Prepare(MegaShapeLoft loft)
	{
		MegaShape	path = layerPath;

		float loftdist = (path.splines[curve].length * pathLength);

		LoftLength = loftdist;

		crosses = Mathf.CeilToInt(loftdist / pathDist);

		if ( crosses < 2 )
			crosses = 2;

		if ( enabled )
			uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0.0f), Vector3.one);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		trisstart = triindex;

		if ( Lock )
			return triindex + ((crosses - 2) * (evert - svert));

		if ( layerPath == null || layerSection == null )
			return triindex;

		MegaSpline	pathspline = layerPath.splines[curve];
		MegaSpline	sectionspline = layerSection.splines[crosscurve];

		// so for each loft section run through
		int vi = 0;
		Vector2 uv = Vector2.zero;
		Vector3 p = Vector3.zero;
		Vector3 scl = Vector3.one;

		float scalemultx = 1.0f;
		float scalemulty = 1.0f;

		Vector3 cmax = crossmax;
		cmax.x = 0.0f;
		cmax.z = 0.0f;

		Vector3 cmin = crossmin;
		cmin.x = 0.0f;
		cmin.z = 0.0f;

		Vector3 totaloff = Vector3.zero;

		float uvstart = pathStart;
		if ( UVOrigin == MegaLoftUVOrigin.SplineStart )
			uvstart = 0.0f;

		Matrix4x4 twisttm = Matrix4x4.identity;

		Color col1 = color;

		Matrix4x4 tm;
		Vector3 lastup = loft.up;

		float calpha = 0.0f;

		for ( int cr = 0; cr < crosses; cr++ )
		{
			float a = ((float)cr / (float)(crosses - 1));
			float alpha = pathStart + (pathLength * a);

			totaloff = offset;

			if ( useOffsetX )
				totaloff.x += offsetCrvX.Evaluate(alpha);

			if ( useOffsetY )
				totaloff.y += offsetCrvY.Evaluate(alpha);

			if ( useOffsetZ )
				totaloff.z += offsetCrvZ.Evaluate(alpha);

			// get the point on the spline
			if ( frameMethod == MegaFrameMethod.New )
				tm = loft.GetDeformMatNewMethod(pathspline, alpha, true, alignCross, ref lastup);
			else
				tm = loft.GetDeformMatNew(pathspline, alpha, true, alignCross);

			if ( useTwistCrv )
			{
				float twist = twistCrv.Evaluate(alpha) * twistAmt;

				float tw1 = pathspline.GetTwist(alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				tm = tm * twisttm;
			}

			if ( useCrossScaleCrv )
			{
				float sa = Mathf.Repeat(a + scaleoff, 1.0f);
				scalemultx = crossScaleCrv.Evaluate(sa);
			}

			if ( !sepscale )
				scalemulty = scalemultx;
			else
			{
				if ( useCrossScaleCrvY )
				{
					float sa = Mathf.Repeat(a + scaleoffY, 1.0f);
					scalemulty = crossScaleCrvY.Evaluate(sa);
				}
			}

			scl.x = crossScale.x * scalemultx;	// Use plus here and have curve as 0010
			scl.y = crossScale.y * scalemulty;

			Vector3 crrot = cmax;
			crrot.y *= scl.y;

			Vector3 cminrot = cmin;
			cminrot.y *= scl.y;

			// Now need to loop through all the meshsections
			for ( int m = 0; m < meshsections.Count; m++ )
			{
				MegaMeshSection ms = meshsections[m];
				MegaMaterialSection mats = sections[ms.mat];

				if ( mats.Enabled )
				{
					if ( loft.useColors )
					{
						if ( mats.colmode == MegaLoftColMode.Loft )
							calpha = a;
						else
							calpha = alpha;

						calpha = Mathf.Repeat(calpha + mats.coloffset, 1.0f);
						col1.r = mats.colR.Evaluate(calpha);
						col1.g = mats.colG.Evaluate(calpha);
						col1.b = mats.colB.Evaluate(calpha);
						col1.a = mats.colA.Evaluate(calpha);
					}

					for ( int v = 0; v < ms.cverts.Count; v++ )
					{
						p.x = ms.cverts[v].x * scl.x;
						p.y = ms.cverts[v].y * scl.y;	// Curve for this value
						p.z = ms.cverts[v].z * scl.z;

						p = tm.MultiplyPoint3x4(p);

						uv.x = alpha - uvstart;	//pathStart;
						uv.y = ms.cuvs[v].y;	// - crossStart;	// again not sure here start;

						if ( mats.physuv )
						{
							uv.x *= pathspline.length;
							uv.y *= sectionspline.length;
						}
						else
						{
							if ( mats.uvcalcy )
							{
								//uv.x = ((a * LoftLength) / sectionspline.length) - uvstart;
								uv.x = ((alpha * pathspline.length) / sectionspline.length) - uvstart;
							}
						}

						if ( conform )
							ms.verts1.Add(p + totaloff);
						else
							ms.verts.Add(p + totaloff);

						if ( mats.swapuv )
						{
							float ux = uv.x;
							uv.x = uv.y;
							uv.y = ux;
						}

						uv.x *= mats.UVScale.x;
						uv.y *= mats.UVScale.y;

						uv.x += mats.UVOffset.x;
						uv.y += mats.UVOffset.y;

						ms.uvs.Add(uv);	//[vi] = uv;

						if ( loft.useColors )
							ms.cols.Add(col1);

						vi++;
					}
				}
			}
		}

		//OptmizeMesh();
		// Faces
		int index = triindex;
		int	fi = 0;	// Calc this

		if ( enabled )
		{
			if ( flip )
			{
				for ( int m = 0; m < meshsections.Count; m++ )
				{
					MegaMeshSection ms = meshsections[m];
					MegaMaterialSection mats = sections[ms.mat];

					if ( mats.Enabled )
					{
						for ( int cr = 0; cr < crosses - 1; cr++ )
						{
							for ( int f = 0; f < ms.cverts.Count - 1; f++ )
							{
								ms.tris.Add(index + f);
								ms.tris.Add(index + f + 1);
								ms.tris.Add(index + f + 1 + ms.cverts.Count);

								ms.tris.Add(index + f);
								ms.tris.Add(index + f + 1 + ms.cverts.Count);
								ms.tris.Add(index + f + ms.cverts.Count);

								fi += 6;
							}

							index += ms.cverts.Count;
						}
						index += ms.cverts.Count;
					}
				}
			}
			else
			{
				for ( int m = 0; m < meshsections.Count; m++ )
				{
					MegaMeshSection ms = meshsections[m];
					MegaMaterialSection mats = sections[ms.mat];

					if ( mats.Enabled )
					{
						for ( int cr = 0; cr < crosses - 1; cr++ )
						{
							for ( int f = 0; f < ms.cverts.Count - 1; f++ )
							{
								ms.tris.Add(index + f + 1 + ms.cverts.Count);
								ms.tris.Add(index + f + 1);
								ms.tris.Add(index + f);

								ms.tris.Add(index + f + ms.cverts.Count);
								ms.tris.Add(index + f + 1 + ms.cverts.Count);
								ms.tris.Add(index + f);

								fi += 6;
							}

							index += ms.cverts.Count;
						}

						index += ms.cverts.Count;
					}
				}
			}
		}

		if ( conform )
		{
			CalcBounds();
			DoConform(loft);
		}

		return triindex + fi;	//triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerMultiMat layer =  go.AddComponent<MegaLoftLayerMultiMat>();

		//meshsections = new List<MegaMeshSection>();	//null;	//.Clear();

		Copy(this, layer);

		layer.meshsections = new List<MegaMeshSection>();	//.Clear();
		loftverts = null;
		loftverts1 = null;
		loftuvs = null;
		loftcols = null;
		capStartVerts = null;
		capStartUVS = null;
		capStartCols = null;
		capEndVerts = null;
		capEndUVS = null;
		capEndCols = null;
		capStartTris = null;
		capEndTris = null;
		lofttris = null;

		return null;
	}

	void CalcBounds()
	{
		conminz = float.MaxValue;

		for ( int i = 0; i < meshsections.Count; i++ )
		{
			MegaMeshSection ms = meshsections[i];

			for ( int v = 0; v < ms.verts1.Count; v++ )
			{
				if ( ms.verts1[v].y < conminz )
					conminz = ms.verts1[v].y;
			}
		}
	}

	void InitConform()
	{
		for ( int m = 0; m < meshsections.Count; m++ )
		{
			MegaMeshSection ms = meshsections[m];

			ms.offsets = new float[ms.verts1.Count];
			ms.last = new float[ms.verts1.Count];

			for ( int i = 0; i < ms.verts1.Count; i++ )
				ms.offsets[i] = ms.verts1[i].y - conminz;
		}

		// If loft has changed we need to update bounds, could do anyway in builder
		// Only need to do this if target changes, move to SetTarget
		if ( target )
			conformCollider = target.GetComponent<Collider>();
	}

	// We could do a bary centric thing if we grid up the bounds
	void DoConform(MegaShapeLoft loft)
	{
		InitConform();

		if ( target && conformCollider )
		{
			Matrix4x4 loctoworld = transform.localToWorldMatrix;

			Matrix4x4 tm = loctoworld;
			Matrix4x4 invtm = tm.inverse;

			Ray ray = new Ray();
			RaycastHit	hit;

			float ca = conformAmount * loft.conformAmount;

			// When calculating alpha need to do caps sep
			for ( int m = 0; m < meshsections.Count; m++ )
			{
				MegaMeshSection ms = meshsections[m];
				for ( int i = 0; i < ms.verts1.Count; i++ )
				{
					Vector3 origin = tm.MultiplyPoint(ms.verts1[i]);
					origin.y += raystartoff;
					ray.origin = origin;
					ray.direction = Vector3.down;

					ms.verts.Add(ms.verts1[i]);

					if ( conformCollider.Raycast(ray, out hit, raydist) )
					{
						Vector3 lochit = invtm.MultiplyPoint(hit.point);

						Vector3 p = ms.verts[i];
						p.y = Mathf.Lerp(p.y, lochit.y + ms.offsets[i] + conformOffset, ca);	//conformAmount);
						ms.verts[i] = p;
						ms.last[i] = p.y;
					}
					else
					{
						Vector3 ht = ray.origin;
						ht.y -= raydist;
						Vector3 p = ms.verts[i];

						p.y = ms.last[i];
						ms.verts[i] = p;
					}
				}
			}
		}
		else
		{
			for ( int m = 0; m < meshsections.Count; m++ )
			{
				MegaMeshSection ms = meshsections[m];
				for ( int i = 0; i < ms.verts1.Count; i++ )
					ms.verts.Add(ms.verts1[i]);
			}
		}
	}
}

// DONE: Conform
// DONE: functions for walk loft
// TODO: cap ends
// DONE: tidy code
// DONE: rename
// DONE: move file
// TODO: do help page
// TODO: remove track textures
// TODO: Submit update
// TODO: get copy working
// 1814
// 1349
// 1335
#endif
