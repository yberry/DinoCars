
using UnityEngine;
using System;
using System.Collections.Generic;

// IDEA: Allow full track to be built no vert limit, then split that into meshes
// also could just build current section over multiple frames

public class MegaLoftLayerMultiMatComplex : MegaLoftLayerMultiMat
{
	public List<MegaLoftSection>	loftsections	= new List<MegaLoftSection>();
	public MegaLoftEaseType			easeType		= MegaLoftEaseType.Sine;
	public MegaLoftEase				ease			= new MegaLoftEase();
	public int						PathSteps		= 16;
	public float					PathTeeter		= 1.0f;
	Matrix4x4						wtm1;
	Vector3							locup			= Vector3.up;

	public bool advancedParams = false;
	public float	handlesize = 1.0f;
	public bool	showsections = false;

	public override void Notify(MegaSpline spline, int reason)
	{
		if ( layerPath && layerPath.splines != null)
		{
			if ( curve < layerPath.splines.Count )
			{
				if ( layerPath.splines[curve] == spline )
				{
					MegaShapeLoft loft = GetComponent<MegaShapeLoft>();
					loft.rebuild = true;
					loft.BuildMeshFromLayersNew();
					return;
				}
			}
			else
			{
				curve = 0;
			}
		}

		for ( int i = 0; i < loftsections.Count; i++ )
		{
			if ( loftsections[i].shape )
			{
				if ( loftsections[i].shape.splines[loftsections[i].curve] == spline )
				{
					MegaShapeLoft loft = GetComponent<MegaShapeLoft>();
					loft.rebuild = true;
					loft.BuildMeshFromLayersNew();
					return;
				}
			}
		}
	}

	public override int GetHelp()
	{
		return 5218;
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		MegaLoftSection ls = loftsections[0];
		for ( int i = 0; i < ls.meshsections.Count; i++ )
		{
			MegaMeshSection ms = ls.meshsections[i];
			//Debug.Log("verts " + ms.verts.Count);
			Array.Copy(ms.verts.ToArray(), 0, verts, offset, ms.verts.Count);
			Array.Copy(ms.uvs.ToArray(), 0, uvs, offset, ms.uvs.Count);
			offset += ms.verts.Count;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		MegaLoftSection ls = loftsections[0];
		for ( int i = 0; i < ls.meshsections.Count; i++ )
		{
			MegaMeshSection ms = ls.meshsections[i];
			Array.Copy(ms.verts.ToArray(), 0, verts, offset, ms.verts.Count);
			Array.Copy(ms.uvs.ToArray(), 0, uvs, offset, ms.uvs.Count);
			Array.Copy(ms.cols.ToArray(), 0, cols, offset, ms.cols.Count);
			offset += ms.verts.Count;
		}
	}

	public override int NumVerts()
	{
		int num = 0;

		MegaLoftSection ls = loftsections[0];
		for ( int i = 0; i < ls.meshsections.Count; i++ )
			num += ls.meshsections[i].cverts.Count * crosses;

		//Debug.Log("num verts " + num + " crosses " + crosses);
		return num;
	}

	public override Material GetMaterial(int i)
	{
		MegaLoftSection ls = loftsections[0];

		if ( i >= 0 && i < ls.meshsections.Count )
		{
			int id = ls.meshsections[i].mat;

			if ( id >= 0 && id < sections.Count )
				return sections[ls.meshsections[i].mat].mat;
		}

		return null;
	}

	public override int[] GetTris(int i)
	{
		MegaLoftSection ls = loftsections[0];

		//Debug.Log("tris " + ls.meshsections[i].tris.Count);
		return ls.meshsections[i].tris.ToArray();	//.tris;
	}

	public override int NumMaterials()
	{
		MegaLoftSection ls = loftsections[0];

		//Debug.Log("materials " + ls.meshsections.Count);
		return ls.meshsections.Count;
	}

	public override bool Valid()
	{
		if ( LayerEnabled && layerPath && layerPath.splines != null && loftsections.Count > 0 && sections.Count > 0 )
		{
			for ( int i = 0; i < loftsections.Count; i++ )
			{
				if ( loftsections[i].shape == null )
				{
					//Debug.Log("Not valid 1");
					return false;
				}
			}

			if ( curve < 0 ) curve = 0;

			if ( curve > layerPath.splines.Count - 1 )
				curve = layerPath.splines.Count - 1;

			if ( crosscurve < 0 ) crosscurve = 0;

			//if ( crosscurve > layerSection.splines.Count - 1 )
				//crosscurve = layerSection.splines.Count - 1;

			return true;
		}

		//Debug.Log("Not valid end");
		return false;
	}

		// Get the different id sections
	public void FindSections(MegaLoftSection lsection, MegaSpline spline)
	{
		if ( spline != null )
		{
			lsection.meshsections.Clear();

			int id = spline.knots[0].id - 1;

			MegaMeshSection msect = null;

			for ( int i = 0; i < spline.knots.Count; i++ )
			{
				if ( spline.knots[i].id != id )
				{
					id = spline.knots[i].id;
					if ( msect != null )
						msect.lastknot = i;

					msect = new MegaMeshSection();

					msect.firstknot = i;
					msect.mat = FindMaterial(id);

					lsection.meshsections.Add(msect);
				}
			}

			lsection.meshsections[lsection.meshsections.Count - 1].lastknot = spline.knots.Count - 1;
		}
	}


	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		MegaMeshSection ms = null;
		MegaLoftSection lsection = loftsections[0];

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < lsection.meshsections.Count; i++ )
		{
			ms = lsection.meshsections[i];
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

	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		MegaMeshSection ms = null;
		MegaLoftSection lsection = loftsections[0];

		//Debug.Log("cain " + ca);
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < lsection.meshsections.Count; i++ )
		{
			ms = lsection.meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
				break;
		}

		//Debug.Log("start " + ms.castart + " end " + ms.caend);
		ca = (ca - ms.castart) / (ms.caend - ms.castart);

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		//Debug.Log("ca " + ca);
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

		//Debug.Log("cindex " + cindex + " interp " + interp + " verts " + ms.cverts.Count);
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
		MegaLoftSection lsection = loftsections[0];

		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		for ( int i = 0; i < lsection.meshsections.Count; i++ )
		{
			ms = lsection.meshsections[i];
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

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		// first need to find the material section we are in from ca
		MegaMeshSection ms = null;
		MegaLoftSection lsection = loftsections[0];

		for ( int i = 0; i < lsection.meshsections.Count; i++ )
		{
			ms = lsection.meshsections[i];
			if ( ca >= ms.castart && ca <= ms.caend )
			{
				//Debug.Log("ms " + i);
				break;
			}
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


	void BuildPolyShape(MegaLoftSection lsection, int steps, Vector3 off1, float width)
	{
		int			curve	= lsection.curve;
		int			lk		= -1;
		Matrix4x4	tm1		= Matrix4x4.identity;
		MegaShape shape = lsection.shape;

		//verts.Clear();
		//uvs.Clear();
		//norms.Clear();

		MegaMatrix.Translate(ref tm1, pivot);
		Vector3 rot = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * rot.x, Mathf.Deg2Rad * rot.y, Mathf.Deg2Rad * rot.z));

		svert = verts.Count;

		{
			float dst = crossDist;

			if ( dst < 0.01f )
				dst = 0.01f;

			svert = verts.Count;

			float alpha = crossStart;
			float cend = crossStart + crossEnd;

			if ( alpha < 0.0f )
				alpha = 0.0f;

			if ( cend > 1.0f )
				cend = 1.0f;

			MegaSpline cspl = shape.splines[curve];

			if ( cspl.closed )
			{
				if ( alpha < 0.0f )
					alpha += 1.0f;
			}

			Vector3 off = off1;	//Vector3.zero;
			if ( snap )
				off = cspl.knots[0].p - cspl.knots[0].p;

			if ( cspl.closed )
				alpha = Mathf.Repeat(alpha, 1.0f);

			lsection.meshsections.Clear();
			FindSections(lsection, cspl);

			Vector3 pos = tm1.MultiplyPoint3x4(cspl.InterpCurve3D(alpha, shape.normalizedInterp, ref lk) + off);

			//Debug.Log("MeshSections " + lsection.meshsections.Count);

			for ( int i = 0; i < lsection.meshsections.Count; i++ )
			{
				MegaMeshSection ms = lsection.meshsections[i];	//new MegaMeshSection();
				MegaMaterialSection s = sections[ms.mat];

				int k1 = ms.firstknot;
				int k2 = ms.lastknot;
				float l1 = 0.0f;
				float l2 = 0.0f;

				//Debug.Log("k1 " + k1 + " k2 " + k2);
				if ( k1 > 0 )
					l1 = cspl.knots[k1 - 1].length;
				else
					l1 = 0.0f;

				if ( k2 < cspl.knots.Count - 1 )
					l2 = cspl.knots[k2 - 1].length;
				else
					l2 = cspl.length;

				//float slen = l2 - l1;
				//Debug.Log("l1 " + l1 + " l2 " + l2);

				float a1 = l1 / cspl.length;
				float a2 = l2 / cspl.length;
				ms.castart = a1;
				ms.caend = a2;

				for ( int kn = k1; kn < k2; kn++ )
				{
					pos = cspl.knots[kn].Interpolate(0.0f, cspl.knots[kn + 1]) + lsection.offset;

					pos.x *= lsection.scale.x;
					pos.y *= lsection.scale.y;
					pos.z *= lsection.scale.z;

					pos = tm1.MultiplyPoint3x4(pos + off);
					ms.cverts.Add(pos);

					for ( int j = 1; j < s.steps; j++ )
					{
						float ka = (float)j / (float)s.steps;
						pos = cspl.knots[kn].Interpolate(ka, cspl.knots[kn + 1]) + lsection.offset;

						pos.x *= lsection.scale.x;
						pos.y *= lsection.scale.y;
						pos.z *= lsection.scale.z;

						pos = tm1.MultiplyPoint3x4(pos + off);
						ms.cverts.Add(pos);
					}
				}

				pos = cspl.knots[k2 - 1].Interpolate(1.0f, cspl.knots[k2]) + lsection.offset;

				pos.x *= lsection.scale.x;
				pos.y *= lsection.scale.y;
				pos.z *= lsection.scale.z;

				pos = tm1.MultiplyPoint3x4(pos + off);
				ms.cverts.Add(pos);
				//pos = tm1.MultiplyPoint3x4(cspl.knots[k2].p + off);
				//ms.cverts.Add(pos);
			}

			// Do uv and col now
			for ( int i = 0; i < lsection.meshsections.Count; i++ )
			{
				MegaMeshSection ms1 = lsection.meshsections[i];

				ms1.len = 0.0f;
				//int k1 = ms1.firstknot;
				//int k2 = ms1.lastknot;

				//Debug.Log("k1 " + k1 + " k2 " + k2);
				//float l1 = cspl.knots[k2].length - cspl.knots[k1].length;

				Vector2 uv1 = Vector2.zero;
				ms1.cuvs.Add(uv1);
				ms1.ccols.Add(Color.white);

				for ( int v = 1; v < ms1.cverts.Count; v++ )
					ms1.len += Vector3.Distance(ms1.cverts[v], ms1.cverts[v - 1]);

				for ( int v = 1; v < ms1.cverts.Count; v++ )
				{
					uv1.y += Vector3.Distance(ms1.cverts[v], ms1.cverts[v - 1]) / ms1.len;
					//uv1.y /= len;	//Vector3.Distance(ms1.cverts[v], ms1.cverts[v - 1]);
					ms1.cuvs.Add(uv1);
					ms1.ccols.Add(Color.white);
				}
			}

			// Add end point
			if ( cspl.closed )
				alpha = Mathf.Repeat(cend, 1.0f);
		}

		// Calc normals
		Vector3 up = Vector3.zero;
		Vector3 n1 = Vector3.zero;

		for ( int i = 0; i < lsection.meshsections.Count; i++ )
		{
			MegaMeshSection ms1 = lsection.meshsections[i];

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
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		//Debug.Log("PrepareLoft");
		if ( layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0 )
			return false;

		float loftdist = (layerPath.splines[curve].length * pathLength);
		LoftLength = loftdist;

		ease.SetEasing(easeType);

		locup = loft.up;

		Vector3 off;

		float width = 1.0f;

		if ( loftsections.Count > 0 )
			width = loftsections[0].shape.splines[loftsections[0].curve].length;

		//Debug.Log("Build Poly shapes");
		for ( int c = 0; c < loftsections.Count; c++ )
		{
			if ( loftsections[c].snap )
				off = loftsections[0].shape.splines[loftsections[0].curve].knots[0].p - loftsections[c].shape.splines[loftsections[c].curve].knots[0].p;
			else
				off = Vector3.zero;

			BuildPolyShape(loftsections[c], 0, off, width);
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

	public int GetSection(float alpha, out float lerp)
	{
		if ( loftsections.Count < 2 )
		{
			lerp = alpha;
			return 0;
		}

		int i = 0;
		for ( i = 0; i < loftsections.Count - 1; i++ )
		{
			if ( alpha < loftsections[i + 1].alpha )
				break;
		}

		if ( i == loftsections.Count - 1 )
		{
			lerp = 1.0f;
			i--;
		}
		else
			lerp = (alpha - loftsections[i].alpha) / (loftsections[i + 1].alpha - loftsections[i].alpha);

		return i;
	}

	public Matrix4x4 GetDeformMat(MegaSpline spline, float alpha, bool interp)
	{
		int k = -1;

		Vector3 ps	= spline.InterpCurve3D(alpha, interp, ref k);

		alpha += 0.01f;	// TODO: Tangent value
		if ( spline.closed )
			alpha = alpha % 1.0f;

		Vector3 ps1	= spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x -= ps.x;
		ps1.y -= ps.y;
		ps1.z -= ps.z;

		ps1.y *= PathTeeter;

		MegaMatrix.SetTR(ref wtm1, ps, Quaternion.LookRotation(ps1, locup));

		return wtm1;
	}

	public Matrix4x4 GetDeformMatNewMethod(MegaSpline spline, float alpha, bool interp, ref Vector3 lastup)
	{
		int k = -1;

		Vector3 ps	= spline.InterpCurve3D(alpha, interp, ref k);

		alpha += 0.01f;	// TODO: Tangent value
		if ( spline.closed )
			alpha = alpha % 1.0f;

		Vector3 ps1	= spline.InterpCurve3D(alpha, interp, ref k);
		Vector3 ps2;

		ps1.x = ps2.x = ps1.x - ps.x;
		ps1.y = ps2.y = ps1.y - ps.y;
		ps1.z = ps2.z = ps1.z - ps.z;

		ps1.y *= PathTeeter;

		MegaMatrix.SetTR(ref wtm1, ps, Quaternion.LookRotation(ps1, lastup));	//locup));

		// calc new up value
		ps2 = ps2.normalized;
		Vector3 cross = Vector3.Cross(ps2, lastup);
		lastup = Vector3.Cross(cross, ps2);

		return wtm1;
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if ( Lock )
			return triindex;

		if ( layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0 )
			return triindex;

		if ( loftsections.Count < 2 )	//== 0 )
			return triindex;

		Vector2 uv = Vector2.zero;
		Vector3 p = Vector3.zero;

		//int wc = 1;	//ActualCrossVerts;
		float lerp = 0.0f;

		Matrix4x4 pathtm = Matrix4x4.identity;

		//if ( SnapToPath )
		//	pathtm = layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;

		//Vector3 sclc = Vector2.one;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		MegaSpline pathspline = layerPath.splines[curve];

		//bool clsd = layerPath.splines[curve].closed;

		float uvstart = pathStart;
		if ( UVOrigin == MegaLoftUVOrigin.SplineStart )
		{
			uvstart = 0.0f;
		}

		Vector3 lastup = locup;

		Color col1 = color;

		float calpha = 0.0f;
		//float uvalpha = 0.0f;

		for ( int pi = 0; pi < crosses; pi++ )
		{
			float alpha = (float)pi / (float)(crosses - 1);	//PathSteps;
			float pathalpha = pathStart + (pathLength * alpha);

			//uvalpha = pathalpha;
			//if ( clsd )
			//	pathalpha = Mathf.Repeat(pathalpha, 1.0f);

			//if ( useScaleXCrv )
			//	sclc.x = scaleCrvX.Evaluate(pathalpha);

			//if ( useScaleYCrv )
			//	sclc.y = scaleCrvY.Evaluate(pathalpha);

			if ( useTwistCrv )
			{
				float twist = twistCrv.Evaluate(pathalpha);
				float tw1 = layerPath.splines[curve].GetTwist(pathalpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( frameMethod == MegaFrameMethod.Old )
					tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( frameMethod == MegaFrameMethod.Old )
					tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp);
				else
					tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup);
			}
			// Need to get the crosssection for the given alpha and the lerp value
			int csect = GetSection(pathalpha, out lerp);

			lerp = ease.easing(0.0f, 1.0f, lerp);

			MegaLoftSection cs1 = loftsections[csect];
			MegaLoftSection cs2 = loftsections[csect + 1];

			//MegaSpline sectionspline = cs1.shape.splines[cs1.curve];

			if ( useOffsetX )
				offx = offsetCrvX.Evaluate(pathalpha);

			if ( useOffsetY )
				offy = offsetCrvY.Evaluate(pathalpha);

			if ( useOffsetZ )
				offz = offsetCrvZ.Evaluate(pathalpha);

			for ( int i = 0; i < cs1.meshsections.Count; i++ )
			{
				MegaMeshSection ms0 = loftsections[0].meshsections[i];

				//float slen = sectionspline.knots[ms0.lastknot].length - sectionspline.knots[ms0.firstknot].length;

				MegaMaterialSection mats = sections[ms0.mat];

				if ( mats.Enabled )
				{
					MegaMeshSection ms1 = cs1.meshsections[i];
					MegaMeshSection ms2 = cs2.meshsections[i];

					if ( loft.useColors )
					{
						if ( mats.colmode == MegaLoftColMode.Loft )
							calpha = alpha;
						else
							calpha = pathalpha;

						calpha = Mathf.Repeat(calpha + mats.coloffset, 1.0f);
						col1.r = mats.colR.Evaluate(calpha);
						col1.g = mats.colG.Evaluate(calpha);
						col1.b = mats.colB.Evaluate(calpha);
						col1.a = mats.colA.Evaluate(calpha);
					}

					for ( int v = 0; v < ms1.cverts.Count; v++ )
					{
						p = Vector3.Lerp(ms1.cverts[v], ms2.cverts[v], lerp);	// Easing here?
						//if ( useScaleXCrv )
						//	p.x *= sclc.x;

						//if ( useScaleYCrv )
						//	p.y *= sclc.y;

						p.x += offx;
						p.y += offy;
						p.z += offz;

						p = tm.MultiplyPoint3x4(p);

						p += offset;

						//int ix = (pi * wc) + v;
						if ( conform )
							ms0.verts1.Add(p);
						else
							ms0.verts.Add(p);

						uv.y = Mathf.Lerp(ms1.cuvs[v].y, ms2.cuvs[v].y, lerp);

						uv.x = alpha - uvstart;	//pathStart;
						//uv.y = ms.cuvs[v].y;	// - crossStart;	// again not sure here start;

						if ( mats.physuv )
						{
							uv.x *= pathspline.length;
							uv.y *= ms0.len;	//sectionspline.length;
						}
						else
						{
							if ( mats.uvcalcy )
							{
								//uv.x = ((alpha * LoftLength) / sectionspline.length) - uvstart;
								uv.x = ((alpha * pathspline.length) / ms0.len) - uvstart;
							}
						}

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

						ms0.uvs.Add(uv);	//[vi] = uv;

						if ( loft.useColors )
							ms0.cols.Add(col1);

#if false
					uv.y = uvstart + alpha;

					uv.x *= UVScale.x;
					uv.y *= UVScale.y;

					uv.x += UVOffset.x;
					uv.y += UVOffset.y;
#endif
					//ms0.uvs.Add(uv);
					}
				}
			}
		}

		int index = triindex;
		int	fi = 0;	// Calc this

		if ( enabled )
		{
			if ( flip )
			{
				for ( int m = 0; m < loftsections[0].meshsections.Count; m++ )
				{
					MegaMeshSection ms = loftsections[0].meshsections[m];
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
				for ( int m = 0; m < loftsections[0].meshsections.Count; m++ )
				{
					MegaMeshSection ms = loftsections[0].meshsections[m];
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
		MegaLoftLayerMultiMatComplex layer =  go.AddComponent<MegaLoftLayerMultiMatComplex>();

		//meshsections = new List<MegaMeshSection>();	//null;	//.Clear();

		Copy(this, layer);

		for ( int i = 0; i < layer.loftsections.Count; i++ )
		{
			MegaLoftSection ls = layer.loftsections[i];
			ls.meshsections = new List<MegaMeshSection>();
		}
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

		for ( int i = 0; i < loftsections[0].meshsections.Count; i++ )
		{
			MegaMeshSection ms = loftsections[0].meshsections[i];

			for ( int v = 0; v < ms.verts1.Count; v++ )
			{
				if ( ms.verts1[v].y < conminz )
					conminz = ms.verts1[v].y;
			}
		}
	}

	void InitConform()
	{
		for ( int m = 0; m < loftsections[0].meshsections.Count; m++ )
		{
			MegaMeshSection ms = loftsections[0].meshsections[m];

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
			for ( int m = 0; m < loftsections[0].meshsections.Count; m++ )
			{
				MegaMeshSection ms = loftsections[0].meshsections[m];
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
			for ( int m = 0; m < loftsections[0].meshsections.Count; m++ )
			{
				MegaMeshSection ms = loftsections[0].meshsections[m];
				for ( int i = 0; i < ms.verts1.Count; i++ )
					ms.verts.Add(ms.verts1[i]);
			}
		}
	}

	Vector3 GetCross(int csect, float ca, Vector3 off)
	{
		MegaLoftSection lsection = loftsections[csect];

		int			curve	= lsection.curve;
		int			k		= -1;
		Matrix4x4	tm1		= Matrix4x4.identity;

		float start = crossStart;
		float len = crossEnd;

		if ( lsection.uselen )
		{
			start = lsection.start;
			len = lsection.length;
		}

		MegaShape shape = lsection.shape;

		MegaMatrix.Translate(ref tm1, pivot);
		Vector3 rot = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * rot.x, Mathf.Deg2Rad * rot.y, Mathf.Deg2Rad * rot.z));

		float alpha = start + (ca * len);

		//float alpha = start + ca;

		if ( shape.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

		Vector3 pos = tm1.MultiplyPoint3x4(shape.splines[curve].InterpCurve3D(alpha, shape.normalizedInterp, ref k) + off + lsection.offset);

		pos.x *= lsection.scale.x;
		pos.y *= lsection.scale.y;
		pos.z *= lsection.scale.z;

		return pos;
	}

	public override Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)
	{
		Vector3 p = Vector3.zero;

		float lerp = 0.0f;

		Matrix4x4 pathtm = Matrix4x4.identity;

		//if ( SnapToPath )
			//pathtm = layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;

		//Vector3 sclc = Vector2.one;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		//bool clsd = layerPath.splines[curve].closed;

		Vector3 lastup = locup;

		float alpha = pa;	//(float)pi / (float)PathSteps;
		float pathalpha = pathStart + (pathLength * alpha);

		//if ( clsd )
			//pathalpha = Mathf.Repeat(pathalpha, 1.0f);

		//if ( useScaleXCrv )
			//sclc.x = scaleCrvX.Evaluate(pathalpha);

		//if ( useScaleYCrv )
			//sclc.y = scaleCrvY.Evaluate(pathalpha);

		if ( useTwistCrv )
		{
			float twist = twistCrv.Evaluate(pathalpha);
			float tw1 = layerPath.splines[curve].GetTwist(pathalpha);

			MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
			if ( frameMethod == MegaFrameMethod.Old )
				tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp) * twisttm;
			else
				tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup) * twisttm;
		}
		else
		{
			if ( frameMethod == MegaFrameMethod.Old )
				tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp);
			else
				tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup);
		}
		// Need to get the crosssection for the given alpha and the lerp value
		int csect = GetSection(pathalpha, out lerp);

		lerp = ease.easing(0.0f, 1.0f, lerp);

		Vector3 off = Vector3.zero;

		if ( loftsections[csect].snap )
			off = loftsections[0].shape.splines[loftsections[0].curve].knots[0].p - loftsections[csect].shape.splines[loftsections[csect].curve].knots[0].p;
		else
			off = Vector3.zero;

		Vector3 crossp1 = GetCross(csect, ca, off);

		if ( loftsections[csect + 1].snap )
			off = loftsections[0].shape.splines[loftsections[0].curve].knots[0].p - loftsections[csect + 1].shape.splines[loftsections[csect + 1].curve].knots[0].p;
		else
			off = Vector3.zero;

		Vector3 crossp2 = GetCross(csect + 1, ca, off);

		if ( useOffsetX )
			offx = offsetCrvX.Evaluate(pathalpha);

		if ( useOffsetY )
			offy = offsetCrvY.Evaluate(pathalpha);

		if ( useOffsetZ )
			offz = offsetCrvZ.Evaluate(pathalpha);

		//float size = 1.0f / layerPath.splines[0].length;

		p = Vector3.Lerp(crossp1, crossp2, lerp);
		//if ( useScaleXCrv )
			//p.x *= sclc.x;

		//if ( useScaleYCrv )
			//p.y *= sclc.y;

		p.x += offx;
		p.y += offy;
		p.z += offz;

		p = tm.MultiplyPoint3x4(p);
		p += offset;

		return p;
	}

}
// 1814
// 1349
// 1335
// 1377
// 1309
// 2002
// 1415
// 584
// 580
