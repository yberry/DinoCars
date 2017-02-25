using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif 
using UnityEngine;
namespace CND.Car
{

	public partial class Wheel : MonoBehaviour, IOverridableGravity
	{

		[System.Obsolete]
		void CheckForContact() //aka the "I have no idea what I'm doing" section
		{
			RaycastHit hit;
			ContactInfo contact = new ContactInfo();

			float wheelCircumference = settings.wheelRadius * fullCircle;

			Vector3 gravNorm = LocalGravity.normalized;

			// var src = transform.rotation * transform.position;
			var nextLength = m_contactInfo.springLength;
			float minCompressedLength = CompressedLength(settings.baseSpringLength, settings.maxCompression);
			float compressionMargin = settings.baseSpringLength - minCompressedLength;
			float upSign = (Mathf.Sign(transform.up.y) + float.Epsilon);

			Vector3 moveDelta = (transform.position - lastPos);
			Vector3 moveDir = moveDelta.normalized;

			contact.rootVelocity = moveDelta.magnitude > 0 ? moveDelta / Time.fixedDeltaTime : Vector3.zero;
			contact.rootVelocity = Vector3.Lerp(m_contactInfo.rootVelocity, contact.rootVelocity, 0.9f);

			Quaternion lookRot = (moveDir != Vector3.zero) && (moveDir != transform.forward) ?
				Quaternion.LookRotation(moveDir, transform.up) : transform.rotation;

			contact.relativeRotation = steerRot;
			contact.worldRotation = transform.rotation * Quaternion.Euler(0, steerAngleDeg, 0);
			//	contact.relativeRotation = Quaternion.Euler(0, steerAngleDeg, 0);

			var projMoveDir = Vector3.ProjectOnPlane(moveDir, transform.up);
			var dotForward = contact.forwardDot = Vector3.Dot(
				Vector3.ProjectOnPlane(transform.forward, transform.up),
				projMoveDir);
			var dotSideways = contact.sidewaysDot = Vector3.Dot(
				Vector3.ProjectOnPlane(-transform.right, transform.up),
				projMoveDir);

			//   dotForward = Quaternion.FromToRotation(transform.forward, moveDir).y;

			var asinForward = MathEx.DotToLinear(dotForward); //asin(dot)/(pi/2)
			if (Mathf.Abs(asinForward) < 0.0001) asinForward = 0;
			var asinSide = MathEx.DotToLinear(dotSideways);
			if (Mathf.Abs(asinSide) < 0.0001) asinSide = 0;

			contact.angularVelocity = (contact.angularVelocity + moveDelta.magnitude * wheelCircumference) % wheelCircumference;
			angularVelAngle += contact.angularVelocity * Mathf.Sign(asinForward);

			/*contact.forwardDirection = Mathf.Sign(Vector3.Dot(-gravNorm, transform.up) + float.Epsilon) >= 0 ?
				steerRot * transform.forward : Quaternion.Inverse(steerRot) * transform.forward;*/
			contact.forwardDirection = (transform.rotation * steerRot) * Vector3.forward;

			contact.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation ? asinForward : 1;
			contact.sidewaysRatio = moveDir != Vector3.zero ? asinSide : 1f - contact.forwardRatio; //leftOrRightness 
																									//contact.sideDirection = ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot*Vector3.left*Mathf.Sign(contact.sidewaysRatio)).normalized;
			contact.sideSlipDirection = (transform.rotation * steerRot) * (Vector3.left * Mathf.Sign(contact.sidewaysRatio));

			contact.forwardFriction = settings.maxForwardFriction * Mathf.Abs(contact.forwardRatio);
			contact.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(contact.sidewaysRatio);

			contact.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
			contact.springCompression = m_contactInfo.springCompression;
			contact.springLength = settings.baseSpringLength;

			var sqrtMoveMag = Mathf.Sqrt(moveDelta.magnitude);
			var velAtPoint = contact.rootVelocity;
			var velAtRoot = (transform.position - lastPos) / Time.fixedDeltaTime;
			//var sqrVel = vel * vel.magnitude;  

			//var sqrGrav = gravity * gravity.magnitude;
			dotVelGrav = Vector3.Dot(moveDir, gravNorm);
			dotVelY = Vector3.Dot(transform.up, moveDir);
			dotDownGrav = Vector3.Dot(-transform.up, gravNorm);

			//dotGrav = (Mathf.Asin(dotGrav) / halfPI);

			if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength * hitDetectionTolerance/* * settings.maxExpansion */+ settings.wheelRadius))
			{
				if (false && Mathf.Abs(contact.sidewaysRatio) > 0.1f)
				{
					Debug.ClearDeveloperConsole();
					Debug.Log("Sideways: " + contact.sidewaysRatio + " - " + contact.sideSlipDirection + " - grav: " + LocalGravity);
				}

				var dotHitGrav = Vector3.Dot(-hit.normal, gravNorm);
				float springLength = Mathf.Max(minCompressedLength, Mathf.Min(settings.baseSpringLength, hit.distance - settings.wheelRadius));
				float currentCompressionLength = settings.baseSpringLength - springLength;

				contact.springLength = springLength;
				contact.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
				contact.wasAlreadyOnFloor = m_contactInfo.isOnFloor;
				contact.isOnFloor = true;
				contact.hit = hit;


				var colVel = contact.otherColliderVelocity = GetColliderVelocity(hit, contact.wasAlreadyOnFloor);
				Vector3 totalVel = velAtPoint + colVel;

				Vector3 horizontalVel = contact.horizontalRootVelocity = Vector3.ProjectOnPlane(totalVel, transform.up);
				Vector3 verticalVel = contact.verticalRootVelocity = (velAtPoint - horizontalVel);

				//var damping = dotVelY * settings.damping;
				const float shockCancelPct = 100;
				//Vector3 hitToHinge = transform.position - wheelCenter;
				Vector3 shockCancel = Vector3.Lerp(-(verticalVel + horizontalVel * 0.25f), -verticalVel, Mathf.Sign(dotVelY) - MathEx.DotToLinear(dotVelY));// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
																																							//shockCancel *= (1f - Mathf.Clamp01(MathEx.DotToLinear(-dotVelGrav))) ;

				// var reflect =  Vector3.Reflect(vel , hit.normal) * shockCancelPct * Time.fixedDeltaTime * Time.fixedDeltaTime;
				Vector3 stickToFloor = shockCancel;
				stickToFloor += -LocalGravity * ((MathEx.DotToLinear(dotDownGrav) + 1f) * 0.5f); /*  * (1f-Mathf.Abs(dotVelGrav) * (1f-Time.fixedDeltaTime*20f)*/
																								 //stickToFloor += -horizontalVel  * contactInfo.springCompression;
				Vector3 pushForce;
				float springResistance = Mathf.Lerp(
					 contact.springCompression * contact.springCompression * contact.springCompression,
					Mathf.Clamp01(Mathf.Sin(halfPI * contact.springCompression)), settings.stiffnessAdjust) * 100f * Time.fixedDeltaTime;


				if (legacySuspensions)
				{
					float springExpand = 1f + verticalVel.magnitude * Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce * Mathf.Sign(-dotVelY);
					springExpand = Mathf.Clamp(springExpand, contact.springCompression, (1f+ contact.springCompression)+0* 100f * settings.springForce + 0 * float.PositiveInfinity);

					float springDamp = 1f - ((verticalVel.magnitude) * settings.compressionDamping * Mathf.Sign(dotVelY));
					springDamp = Mathf.Clamp(springDamp, -1f * 0, 1f);

					pushForce = Vector3.Lerp(
						stickToFloor * springResistance * springDamp,
						stickToFloor * springResistance * springExpand,
						 contact.springCompression);

					//pushForce= Vector3.ClampMagnitude(pushForce, (vel.magnitude/Time.fixedDeltaTime)/shockAbsorb);

				}
				else
				{
					float springExpand = (contactInfo.springCompression) * settings.springForce * 0.95f;
					float springDamp = verticalVel.magnitude * (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.compressionDamping;
					pushForce = (transform.up * springExpand + transform.up * springDamp) * Time.fixedDeltaTime * Time.fixedDeltaTime;
					//	pushForce = Vector3.Lerp(m_contactInfo.upForce, stickToFloor, 0.5f);
					/*
					float springExpand =( contactInfo.springCompression) *Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce ;
					float springDamp = (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;
					pushForce = Vector3.Lerp(m_contactInfo.upForce, transform.up * (springExpand+ springDamp),1f);*/
				}

				contact.appliedSpringForce = pushForce;

			}
			else
			{

				//curContact.upForce *= 0;
				if (prevContactInfo.isOnFloor)
				{
					contact = prevContactInfo;
					contact.isOnFloor = false;
				}
				else
				{
					if (Application.isPlaying)
					{
						contact.hit = default(RaycastHit);
						contact.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength * Mathf.Lerp(1f, settings.maxDroop, dotDownGrav), 50f * Time.fixedDeltaTime);
						contact.springCompression = (settings.baseSpringLength - contact.springLength) / compressionMargin;
					}

				}

			}

			m_contactInfo = contact;
		}



		//--------------------------------------------------------------------------------------
		//EDITOR
		//--------------------------------------------------------------------------------------


#if UNITY_EDITOR
		[Header("Debug Gizmos")]
		public bool showDrift = true;
		public bool showForward = true;
		public bool showSpring = true;
		public bool showWheelDisc = true;
		public bool showCompressedWheelDisc = true;

		void OnDrawGizmos()
		{

			const float arcAngle = 30f;
			Quaternion curRot = steerRot;
			var src = transform.position;
			Vector3 center;
			var rotNorm = (transform.rotation * curRot);
			var absSteerRot = rotNorm * Vector3.right;
			var lookRotNormal = transform.localRotation* Quaternion.LookRotation(absSteerRot, transform.up);

			if (!Application.isPlaying)
			{

				CheckForContact();
				RecalculatePositions();
				m_contactInfo.angularVelocity = prevContactInfo.angularVelocity = angularVelAngle = 0;
				curRot = transform.rotation * Quaternion.LookRotation(transform.forward, transform.up);
				center = wheelCenter;
			}
			else
			{
				center = wheelGraphics.transform.position;
			}

			Vector3 lagOffset = wheelCenter - center;
			float absSide = Mathf.Abs(m_contactInfo.sidewaysRatio);
			float absForward = Mathf.Abs(m_contactInfo.forwardRatio);

			Color defHandleColor = Color.white;
			Color defGizmoColor = Color.white;
			
			if (!enabled)
			{
				Gizmos.color = defGizmoColor *= 0.5f;
				Handles.color = defHandleColor *= 0.5f;
			}

			Color contactColor = defGizmoColor * (m_contactInfo.isOnFloor ? Color.green : Color.red);

			Gizmos.color = defGizmoColor * contactColor;
			if (m_contactInfo.isOnFloor && m_contactInfo.hit.distance < settings.baseSpringLength - (settings.baseSpringLength * settings.maxCompression))
			{
				Gizmos.color = defGizmoColor * Color.yellow;
			}

			if (showWheelDisc)
			{
				Handles.color = Gizmos.color * 0.25f;
				Handles.DrawSolidDisc(center, lookRotNormal * Vector3.forward, settings.wheelRadius);

				Handles.color = Gizmos.color;
				Handles.CircleCap(0, center, lookRotNormal, settings.wheelRadius);
				Handles.color = Gizmos.color * 0.75f;


				Handles.DrawSolidArc(center, lookRotNormal * Vector3.forward,
				   rotNorm * (Quaternion.Euler(angularVelAngle * Mathf.Rad2Deg - arcAngle * 0.5f, 0, 0)) * Vector3.down, arcAngle, settings.wheelRadius * 0.9f);

			}


			Gizmos.color = Handles.color = defGizmoColor;
			Color dirMultipliers = new Color(1.2f, 0.8f, 1.2f, 0.85f);
			Gizmos.DrawWireSphere(center, 0.05f);
			if (showDrift && absSide > 0.01)
			{
				Gizmos.color = Handles.color = defGizmoColor * Handles.xAxisColor * dirMultipliers;

				Vector3 sidewaysEnd = m_contactInfo.sideSlipDirection * -absSide;
				if (absSide > 0)
				{
					Gizmos.DrawLine(center, center + sidewaysEnd);

					Quaternion arrowRot = m_contactInfo.sidewaysRatio >= 0 ?
						lookRotNormal : lookRotNormal * Quaternion.FromToRotation(Vector3.right, Vector3.left);

					Handles.ArrowCap(0, center, arrowRot, absSide * 1.33f);
				}

			}
			if (showForward && absForward > 0.01)
			{
				Gizmos.color = Handles.color = defGizmoColor * Handles.zAxisColor * dirMultipliers;
				Vector3 forwardEnd = m_contactInfo.forwardDirection * m_contactInfo.forwardRatio;
				if (absForward < 0.01f || !Application.isPlaying)
					forwardEnd = m_contactInfo.forwardDirection;
				forwardEnd *= settings.wheelRadius;
				Quaternion arrowRot = m_contactInfo.forwardDot >= 0 ?
					transform.rotation * steerRot : (transform.rotation * steerRot) * Quaternion.FromToRotation(Vector3.forward, Vector3.back);
				Gizmos.DrawLine(center, center + forwardEnd);
				Handles.ArrowCap(0, center + forwardEnd, arrowRot, absForward * 0.85f);
			}

			Gizmos.color = defGizmoColor;
			Handles.color = defHandleColor;

			Gizmos.DrawLine(center, targetContactPoint - lagOffset); //wheel radius

			if (m_contactInfo.isOnFloor)
			{
				Gizmos.color = Handles.color=defGizmoColor * Color.Lerp(Color.green, Color.red, contactInfo.springCompression);
			}
			else
			{
				Gizmos.color = Handles.color=Color.yellow;
			}

			if (showSpring)
			{
				Color oldCol = Gizmos.color, nextCol = Gizmos.color*1f;
				nextCol.a = 1f;

				Gizmos.color = nextCol;
				Gizmos.DrawLine(src, center);

				Handles.color = nextCol;
				Gizmos.color = nextCol * 1.5f;
				//Gizmos.DrawSphere(src, 0.075f);
				Handles.DrawAAPolyLine(null,10f,src, center);

				
				Handles.DrawSolidDisc(src, -Camera.current.gameObject.transform.forward, 0.075f);
				Gizmos.color = nextCol*1.1f;
				Gizmos.DrawWireSphere(src, 0.075f);

				Gizmos.color = oldCol;
				Handles.color = oldCol;
			}



			//--contact points

			Vector3 raycastHitPoint = Application.isPlaying ? contactInfo.finalContactPoint : targetContactPoint;
			Handles.color = contactColor;
			Handles.DrawSolidDisc(raycastHitPoint - lagOffset, -Camera.current.gameObject.transform.forward, 0.025f);
			//Gizmos.DrawSphere(raycastHitPoint - lagOffset, 0.05f);
			Gizmos.color = contactColor * 1.1f;
			Gizmos.DrawWireSphere(targetContactPoint - lagOffset, 0.025f);
			Gizmos.DrawWireSphere(raycastHitPoint - lagOffset, 0.025f);

			Gizmos.color /= 1.1f;

			if (!Application.isPlaying || showCompressedWheelDisc)
			{
				var compressedCenter = transform.position - transform.up * CompressedLength(settings.baseSpringLength, settings.maxCompression);
				Handles.color = Gizmos.color = Color.blue * 0.75f;
				Handles.DrawWireArc(compressedCenter, lookRotNormal * Vector3.forward,
					rotNorm * (Quaternion.Euler(-arcAngle * 0.5f, 0, 0)) * Vector3.down, 360f, settings.wheelRadius);
				Gizmos.DrawWireSphere(compressedCenter, 0.025f);

			}

			Handles.color = Color.white;
		}
#endif


		#region  Drawer

#if UNITY_EDITOR && DISABLED
    [CustomPropertyDrawer(typeof(Wheel.Settings))]
    public class WheelSettingsDrawer : PropertyDrawer
    {
        System.Reflection.FieldInfo[] fields;
        SerializedProperty[] members;

        float height = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (fields == null)
            {
                fields = fieldInfo.FieldType.GetFields();
                members = new SerializedProperty[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    var subMember = property.FindPropertyRelative(fields[i].Name);
                    if (subMember != null)
                    {
                        members[i] = subMember;
                        height += EditorGUI.GetPropertyHeight(subMember);
                    }

                }
            }
            return height+2;
        }

      
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.logger.logEnabled=true;

           
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            for (int i= 0;i<members.Length; i++)
            {
                var height = EditorGUI.GetPropertyHeight(members[i]);
                position.height = height;
                // string path = fieldInfo.Name + "." + fields[i].Name;
                EditorGUI.indentLevel = indent;
                if (members[i] != null)
                {                   
                    EditorGUI.PropertyField(position, members[i]);                    
                }
                
                position.y += height;
                
            }


            EditorGUI.EndProperty();
        }
    }
#endif
		#endregion Drawer
	}
}