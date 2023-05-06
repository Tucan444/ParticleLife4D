using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class camera4D : MonoBehaviour
{
    [Range(10, 80)]public float fov = 50;
    [HideInInspector] public float distance = 70;
    [HideInInspector] public Vector3 rotation = new Vector3();
    [HideInInspector] public float fovx = 0;
    bool useMovement = true;

    Vector3 rotationDelta = new Vector3();
    float distanceDelta = 0;
    // Start is called before the first frame update
    void Start()
    {
        fovx = 1 / Mathf.Tan(Mathf.Deg2Rad * fov);
    }

    // Update is called once per frame
    void Update()
    {
        fovx = 1 / Mathf.Tan(Mathf.Deg2Rad * fov);
        if (useMovement) {
            distance += distanceDelta * Time.deltaTime * 20;
            distance = Mathf.Max(1, distance);

            rotation += rotationDelta * Time.deltaTime * 2;
            rotation.y = Mathf.Max(-Mathf.PI * 0.5f, Mathf.Min(Mathf.PI * 0.5f, rotation.y));
            rotation.z = Mathf.Max(-Mathf.PI * 0.5f, Mathf.Min(Mathf.PI * 0.5f, rotation.z));
        }
    }

    public void ToggleMovement() {
        useMovement = !useMovement;
    }

    public void OnDistanceChange(InputAction.CallbackContext context) {
        distanceDelta = context.ReadValue<float>();
    }

    public void OnRotation(InputAction.CallbackContext context) {
        rotationDelta = context.ReadValue<Vector3>();
    }

    private void AddGamma(float value) {
        rotation.y -= value;
        rotation.y = Mathf.Max(-Mathf.PI * 0.5f, Mathf.Min(Mathf.PI * 0.5f, rotation.y));
    }

    public Vector4 GetPosition() {
        E4Rotation rot = E4Rotation.Plane(rotation.z, "xw");
        rot.ApplyRotationOnSelf(E4Rotation.Plane(rotation.y, "xz"));
        rot.ApplyRotationOnSelf(E4Rotation.Plane(rotation.x, "xy"));
        Vector4 pos = rot.RotateVector(new Vector4(1, 0, 0, 0));
        return (distance * pos) + new Vector4(50, 50, 50, 50);
    }

    public Matrix4x4 GetRotation() {
        E4Rotation rot = E4Rotation.Plane(rotation.z, "xw");
        rot.ApplyRotationOnSelf(E4Rotation.Plane(rotation.y, "xz"));
        rot.ApplyRotationOnSelf(E4Rotation.Plane(rotation.x, "xy"));
        rot.ReverseSelf();
        return rot.rotation;
    }
}
