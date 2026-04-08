using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationUtil
{
    public static Quaternion GetQuaternionRotationChildRelativeParentApplicable(Quaternion targetRotation, Quaternion parentRotation, Quaternion childRotation)
    {
        var relativeDirectionPointRotationToParent = Quaternion.Inverse(parentRotation) * childRotation; //child's rotation relative to parent
        var newRotation = targetRotation * Quaternion.Inverse(relativeDirectionPointRotationToParent);   //apply target rotation to the child
        return newRotation;
    }
    public static Vector3 GetAngularVelocity(Quaternion deltaRotation, Vector3 deltaPosition, float deltaTime)
    {
        // ����Ԫ��ת��Ϊ��ת�����ת�Ƕ�
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis); 

        // ������ת�ٶȣ���ת���ÿ����ת�Ļ�����
        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / deltaTime);

        // ����λ�ñ仯������ת�ٶ�
        Vector3 correctedAngularVelocity = angularVelocity - Vector3.Project(angularVelocity, deltaPosition);

        return correctedAngularVelocity;
    }
}
