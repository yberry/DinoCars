
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MegaRope))]
public class MegaRopeEditor : Editor
{
	string[] Layers()
	{
		//string[] layers = new string[32];
		List<string>	lays = new List<string>();

		//layers[0] = LayerMask.LayerToName(0);
		//lays.Add(LayerMask.LayerToName(0));
		for ( int i = 0; i < 31; i++ )
		{
			//layers[i + 1] = LayerMask.LayerToName(i);	//1 << i);
			//Debug.Log("i " + i + " name " + LayerMask.LayerToName(i));
			lays.Add(LayerMask.LayerToName(i));
		}

		return lays.ToArray();	//layers;
	}


	public override void OnInspectorGUI()
	{
		MegaRope rope = (MegaRope)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		//DrawDefaultInspector();

		rope.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", rope.axis);
		rope.top = (Transform)EditorGUILayout.ObjectField("Top", rope.top, typeof(Transform), true);
		rope.bottom = (Transform)EditorGUILayout.ObjectField("Bottom", rope.bottom, typeof(Transform), true);
		rope.layer.value = EditorGUILayout.MaskField("Layer", rope.layer.value, Layers());
		rope.fudge = EditorGUILayout.FloatField("Fudge", rope.fudge);
		rope.boxsize = EditorGUILayout.FloatField("Box Size", rope.boxsize);
		rope.RopeLength = EditorGUILayout.FloatField("Rope Length", rope.RopeLength);
		rope.fixedends = EditorGUILayout.Toggle("Fixed Ends", rope.fixedends);
		rope.vel = EditorGUILayout.FloatField("Vel", rope.vel);
		rope.drawsteps = EditorGUILayout.IntField("Draw Steps", rope.drawsteps);
		rope.DisplayDebug = EditorGUILayout.Toggle("Display Debug", rope.DisplayDebug);
		rope.radius = EditorGUILayout.FloatField("Radius", rope.radius);
		//rope.ropeRadius = EditorGUILayout.FloatField("Rope Radius", rope.ropeRadius);
		rope.startShape = (MegaShape)EditorGUILayout.ObjectField("Start Shape", rope.startShape, typeof(MegaShape), true);

		rope.ropeup = EditorGUILayout.Vector3Field("Rope Up", rope.ropeup);
		rope.DoCollide = EditorGUILayout.Toggle("Do Collide", rope.DoCollide);
		rope.SelfCollide = EditorGUILayout.Toggle("Self Collide", rope.SelfCollide);

		//if ( GUI.changed )	//rebuild )
		//	rope.Rebuild = true;

		rope.type = (MegaRopeType)EditorGUILayout.EnumPopup("Type", rope.type);

		//rope.Width = EditorGUILayout.FloatField("Width", mod.Width);

		switch ( rope.type )
		{
			case MegaRopeType.Rope:
				RopeMesherGUI(rope.strandedMesher);
				break;

			case MegaRopeType.Chain:
				ChainMesherGUI(rope.chainMesher);
				break;

			case MegaRopeType.Hose:
				HoseMesherGUI(rope.hoseMesher);
				break;

			case MegaRopeType.Object:
				ObjectMesherGUI(rope.objectMesher);
				break;
		}

		// Physics
		rope.solverType = (MegaRopeSolverType)EditorGUILayout.EnumPopup("Solver", rope.solverType);
		rope.spring = EditorGUILayout.FloatField("Spring", rope.spring);
		rope.damp = EditorGUILayout.FloatField("Damp", rope.damp);
		rope.timeStep = EditorGUILayout.FloatField("Time Step", rope.timeStep);
		rope.friction = EditorGUILayout.FloatField("Friction", rope.friction);
		rope.Mass = EditorGUILayout.FloatField("Mass", rope.Mass);
		rope.Density = EditorGUILayout.FloatField("Density", rope.Density);
		rope.DampingRatio = EditorGUILayout.FloatField("Damping Ratio", rope.DampingRatio);
		rope.gravity = EditorGUILayout.Vector3Field("Gravity", rope.gravity);
		rope.airdrag = EditorGUILayout.FloatField("Air Drag", rope.airdrag);
		rope.stiffsprings = EditorGUILayout.BeginToggleGroup("Stiff Springs", rope.stiffsprings);
		rope.stiffspring = EditorGUILayout.FloatField("Stiff Spring", rope.stiffspring);
		rope.stiffdamp = EditorGUILayout.FloatField("Stiff Damp", rope.stiffdamp);
		rope.stiffnessCrv = EditorGUILayout.CurveField("Stiffness Crv", rope.stiffnessCrv);
		EditorGUILayout.EndToggleGroup();

		rope.floorfriction = EditorGUILayout.FloatField("Floor Friction", rope.floorfriction);
		rope.bounce = EditorGUILayout.FloatField("Bounce", rope.bounce);
		rope.points = EditorGUILayout.IntField("Points", rope.points);
		rope.iters = EditorGUILayout.IntField("Iters", rope.iters);

		rope.rbodyforce = EditorGUILayout.FloatField("RBody Force", rope.rbodyforce);

		if ( GUI.changed )	//rebuild )
		{
			if ( !Application.isPlaying )
				rope.Rebuild = true;
			EditorUtility.SetDirty(rope);
			//	rope.Rebuild();
		}
	}

	void RopeMesherGUI(MegaRopeStrandedMesher rm)
	{
		rm.show = EditorGUILayout.Foldout(rm.show, "Mesher Options");

		if ( rm.show )
		{
			rm.sides = EditorGUILayout.IntField("Sides", rm.sides);
			rm.strands = EditorGUILayout.IntField("Strands", rm.strands);
			rm.strandRadius = EditorGUILayout.FloatField("Strand Radius", rm.strandRadius);
			rm.offset = EditorGUILayout.FloatField("Offset", rm.offset);
			//rm.radius = EditorGUILayout.FloatField("Radius", rm.radius);
			//rm.segments = EditorGUILayout.IntField("Segments", rm.segments);
			rm.SegsPerUnit = EditorGUILayout.FloatField("Segs Per Unit", rm.SegsPerUnit);
			rm.TwistPerUnit = EditorGUILayout.FloatField("Twists Per Unit", rm.TwistPerUnit);
			rm.uvtwist = EditorGUILayout.FloatField("UV Twist", rm.uvtwist);
			rm.uvtilex = EditorGUILayout.FloatField("UV Tile X", rm.uvtilex);
			rm.uvtiley = EditorGUILayout.FloatField("UV Tile Y", rm.uvtiley);
			rm.Twist = EditorGUILayout.FloatField("Twist", rm.Twist);
			rm.cap = EditorGUILayout.Toggle("Cap", rm.cap);
		}
	}

	void ChainMesherGUI(MegaRopeChainMesher cm)
	{
		cm.show = EditorGUILayout.Foldout(cm.show, "Mesher Options");

		if ( cm.show )
		{
			cm.linkOff = EditorGUILayout.Vector3Field("Link Off", cm.linkOff);
			cm.linkOff1 = EditorGUILayout.Vector3Field("Link Off1", cm.linkOff1);
			cm.LinkRot = EditorGUILayout.Vector3Field("Link Rot", cm.LinkRot);
			cm.LinkRot1 = EditorGUILayout.Vector3Field("Link Rot1", cm.LinkRot1);
			cm.LinkSize = EditorGUILayout.FloatField("Link Size", cm.LinkSize);
			cm.linkScale = EditorGUILayout.Vector3Field("Link Scale", cm.linkScale);
			cm.linkPivot = EditorGUILayout.Vector3Field("Link Pivot", cm.linkPivot);
			//cm.LinkObjects = EditorGUILayout.Toggle("Link Objects", cm.LinkObjects);
			cm.RandomOrder = EditorGUILayout.BeginToggleGroup("Random Order", cm.RandomOrder);
			cm.seed = EditorGUILayout.IntField("Rnd Seed", cm.seed);
			EditorGUILayout.EndToggleGroup();

			for ( int i = 0; i < cm.LinkObj1.Count; i++ )
				cm.LinkObj1[i] = (GameObject)EditorGUILayout.ObjectField("Object", cm.LinkObj1[i], typeof(GameObject), true);

			EditorGUILayout.BeginHorizontal();
			Color col = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("Add Link Obj") )
			{
				cm.rebuild = true;
				GameObject no = null;
				if ( cm.LinkObj1.Count > 0 )
					no = cm.LinkObj1[cm.LinkObj1.Count - 1];
				cm.LinkObj1.Add(no);
				EditorUtility.SetDirty(target);
			}

			GUI.backgroundColor = Color.red;
			if ( GUILayout.Button("Delete Link Obj") )
			{
				cm.rebuild = true;
				cm.LinkObj1.RemoveAt(cm.LinkObj1.Count - 1);
				EditorUtility.SetDirty(target);
			}

			GUI.backgroundColor = col;

			EditorGUILayout.EndHorizontal();

			if ( GUI.changed )
			{
				cm.rebuild = true;
			}
		}
	}

	void HoseMesherGUI(MegaRopeHoseMesher hm)
	{

	}

	void ObjectMesherGUI(MegaRopeObjectMesher om)
	{
		bool changed = GUI.changed;
		GUI.changed = false;
		om.numframes = EditorGUILayout.IntField("Num Frames", om.numframes);
		GameObject src = (GameObject)EditorGUILayout.ObjectField("Source", om.source, typeof(GameObject), true);
		if ( src != om.source )
		{
			om.source = src;
			om.Rebuild((MegaRope)target);
		}

		om.meshaxis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", om.meshaxis);
		om.stretchtofit = EditorGUILayout.Toggle("Stretch", om.stretchtofit);

		om.scale = EditorGUILayout.Vector3Field("Scale", om.scale);
		if ( GUI.changed )
		{
			//if ( !Application.isPlaying )
			//	om.Rebuild((MegaRope)target);
			EditorUtility.SetDirty(target);
		}

		GUI.changed = changed;
	}

}