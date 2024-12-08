using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class TransformationUtil
{
    public static Vector3 QuaternionToScaledAngleAxis(Quaternion q)
    {
        if (q.w > 1f)
            q = NormalizeQuaternion(q);

        float angle = 2.0f * Mathf.Acos(q.w);
        float sinHalfAngle = Mathf.Sqrt(1f - q.w * q.w);

        if (sinHalfAngle < 0.001f)
        {
            return new Vector3(q.x, q.y, q.z) * angle;
        }
        else
        {
            Vector3 axis = new Vector3(q.x, q.y, q.z) / sinHalfAngle;
            return axis * angle;
        }
    }

    public static Quaternion ScaledAngleAxisToQuaternion(Vector3 scaledAngleAxis)
    {
        float angle = scaledAngleAxis.magnitude;
        if (angle < 0.001f)
        {
            // No rotation
            return Quaternion.identity;
        }
        else
        {
            Vector3 axis = scaledAngleAxis / angle;
            float halfAngle = angle / 2f;
            float sinHalfAngle = Mathf.Sin(halfAngle);

            Quaternion q = new Quaternion();
            q.x = axis.x * sinHalfAngle;
            q.y = axis.y * sinHalfAngle;
            q.z = axis.z * sinHalfAngle;
            q.w = Mathf.Cos(halfAngle);

            return q;
        }
    }

    public static Quaternion NormalizeQuaternion(Quaternion q)
    {
        float magnitude = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        return new Quaternion(q.x / magnitude, q.y / magnitude, q.z / magnitude, q.w / magnitude);
    }

    public static Vector3 GetAngularVelocity(Quaternion prevRot, Quaternion currRot, float deltaTime)
    {
        Quaternion deltaRotation = currRot * Quaternion.Inverse(prevRot);
        // Convert the quaternion to an angular velocity vector (in radians per second)
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        if (angleInDegrees > 180f)
            angleInDegrees -= 360f;
        Vector3 angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / deltaTime;
        return angularVelocity;
    }

}
