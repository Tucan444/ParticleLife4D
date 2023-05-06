using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class camera3D : MonoBehaviour
{
    [Range(10, 80)]public float fov = 50;
    [HideInInspector] public Vector3 position = new Vector3(-20, 50, 50);
    [HideInInspector] public Vector3 rotation = new Vector3();
    [HideInInspector] public float fovx = 0;
    bool useMovement = true;

    Vector3 dir = new Vector3();
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
            Vector2 rotated = Quaternion.EulerAngles(new Vector3(0, 0, rotation.z)) * new Vector2(dir.x, dir.y);
            Vector3 fdir = new Vector3(rotated.x, rotated.y, dir.z);
            position += fdir * Time.deltaTime * 60;
        }
    }

    public void ToggleMovement() {
        useMovement = !useMovement;
    }

    public void OnMovement(InputAction.CallbackContext context) {
        dir = context.ReadValue<Vector3>();
        dir = new Vector3(dir.z, dir.x, dir.y);
    }

    public void OnMouse(InputAction.CallbackContext context) {
        Vector2 mdelta = context.ReadValue<Vector2>();
        if (useMovement) {
            rotation.z += mdelta.x * 0.001f;
            AddGamma(mdelta.y * 0.001f);
        }
    }

    private void AddGamma(float value) {
        rotation.y -= value;
        rotation.y = Mathf.Max(-Mathf.PI * 0.5f, Mathf.Min(Mathf.PI * 0.5f, rotation.y));
    }

    public Quaternion GetRotation() {
        Quaternion one = Quaternion.EulerAngles(new Vector3(0, 0, -rotation.z));
        Quaternion two = Quaternion.EulerAngles(new Vector3(0, -rotation.y, 0));
        return two * one;
    }
}
