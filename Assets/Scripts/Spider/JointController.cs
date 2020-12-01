using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class JointController : MonoBehaviour
{
    [Header("Position")]
    public float sensitivity = 100f;
    public string bodyPartName;
    [HideInInspector] public ConfigurableJoint joint;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public float currentRotationX = 0f;
    [HideInInspector] public float currentRotationY = 0f;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        rigidBody = GetComponent<Rigidbody>();        
    }

    public void RotateJoint(float directionX, float directionY)
    {
        var x = currentRotationX + directionX * sensitivity * Time.deltaTime;
        var y = currentRotationY + directionY * sensitivity * Time.deltaTime;
        var xRot = Mathf.Clamp(x, joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit);
        var yRot = Mathf.Clamp(y, -joint.angularYLimit.limit, joint.angularYLimit.limit);
        joint.targetRotation = Quaternion.Euler(xRot, yRot, joint.targetRotation.z);
        currentRotationY = yRot;
        currentRotationX = xRot;
    }
}
