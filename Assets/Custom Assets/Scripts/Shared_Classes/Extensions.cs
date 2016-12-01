using UnityEngine;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//namespace Custom.Extensions
//{


public static class EnumExtensions
    {
        public static bool ContainsFlag(this Enum source, Enum flag)
        {
            var sourceValue = ToUInt64(source);
            var flagValue = ToUInt64(flag);

            return (sourceValue & flagValue) == flagValue;
        }

        public static bool ContainsAnyFlag(this Enum source, Enum flags)
        {
            /*var sourceValue = ToUInt64(source);

            foreach (var flag in flags)
            {
                var flagValue = ToUInt64(flag);

                if ((sourceValue & flagValue) == flagValue)
                {
                    return true;
                }
            }

            return false;*/

		return
			source != null && ((Convert.ToInt32(source) & Convert.ToInt32(flags)) != 0);
	}

        // found in the Enum class as an internal method
        private static ulong ToUInt64(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException("Unknown enum type.");
        }

        public static bool IsNull(this object input)
        {
            return (input == null);
        }

        public static bool IsNotNull(this object input)
        {
            return (input != null);
        }
    }


//}

public static class RotateAroundPivotExtensions
{
    //Returns the rotated Vector3 using a Quaterion
    public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
    {
        return Angle * (Point - Pivot) + Pivot;
    }
    //Returns the rotated Vector3 using Euler
    public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler)
    {
        return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
    }
    //Rotates the Transform's position using a Quaterion
    public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle)
    {
        Me.position = Me.position.RotateAroundPivot(Pivot, Angle);
    }
    //Rotates the Transform's position using Euler
    public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler)
    {
        Me.position = Me.position.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
    }
}

public static class Vector3Extensions{

	public static List<MonoBehaviour> SortByDistance(this List<MonoBehaviour> unsorted, Vector3 origin){

			unsorted.Sort((p1, p2)
				=> (int)Mathf.Sign(p1.transform.position.DistanceTo(origin) - p2.transform.position.DistanceTo(origin) ));
			return unsorted;


	}

	public static float DistanceTo(this Vector3 orig, Vector3 dest)
	{
		return Vector3.Distance(dest, orig);
	}

	public static float SqrDistanceTo(this Vector3 orig, Vector3 dest)
	{
		return Vector3.SqrMagnitude(dest - orig);
	}

	public static Vector3 Randomize(this Vector3 src)
	{
		return RandomizeRange(src, -Vector3.one, Vector3.one);
//		return new Vector3(modifier.x*UnityEngine.Random.Range(-1f, 1f), modifier.y* UnityEngine.Random.Range(-1f, 1f), modifier.z*UnityEngine.Random.Range(-1f, 1f));
    }

	public static Vector3 RandomizeRange(this Vector3 src, Vector3 min, Vector3 max)
	{
		return new Vector3(src.x * UnityEngine.Random.Range(min.x, max.x), src.y * UnityEngine.Random.Range(min.y, max.y), src.z * UnityEngine.Random.Range(min.z,max.z));
	}
}


public static class ObjectCopier
{
	/// <summary>
	/// Perform a deep Copy of the object.
	/// </summary>
	/// <typeparam name="T">The type of object being copied.</typeparam>
	/// <param name="source">The object instance to copy.</param>
	/// <returns>The copied object.</returns>
	public static T Clone<T>(this T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}

		// Don't serialize a null object, simply return the default for that object
		if (System.Object.ReferenceEquals(source, null))
		{
			return default(T);
		}

		IFormatter formatter = new BinaryFormatter();
		Stream stream = new MemoryStream();
		using (stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}

