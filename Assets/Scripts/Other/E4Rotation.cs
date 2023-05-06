using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E4Rotation
{
    public Matrix4x4 rotation = new Matrix4x4();
    static Dictionary<string, int> axis2index = new Dictionary<string, int>() {
        {"x", 0},
        {"y", 1},
        {"z", 2},
        {"w", 3}
    };

    public E4Rotation(Matrix4x4 rotation_) {
        rotation = rotation_;
    }

    // i need some privacy
    // converts cartesian to spherical
    private static Vector3 Cartesian2Spherical(Vector4 v) {
        Vector3 position = new Vector3();

        position[2] = Mathf.Asin(v[3]);
        float cosOfW = Mathf.Cos(position[2]);

        if (Mathf.Abs(cosOfW) < 0.00001f) {return position;}
        v /= cosOfW;

        position[1] = Mathf.Asin(v[1]);
        float cosOfz = Mathf.Cos(position[1]);
        if (Mathf.Abs(cosOfz) < 0.00001f) {return position;}
        position[0] = Mathf.Atan2(v[2] / cosOfz, v[0] / cosOfz);

        return position;
    }

    private static E4Rotation flattenation(Vector4 v) {
        float gamma = 0;
        if (v.x != 1 && v.y != 1) {
            gamma = Mathf.Atan2(v.w, v.z);
        }

        E4Rotation wFlat = E4Rotation.Plane(gamma, "wz");

        Vector4 u = wFlat.RotateVector(v);

        float beta = 0;
        if (u.x != 1) {
            beta = Mathf.Atan2(u.y, u.z);
        }

        E4Rotation yFlat = E4Rotation.Plane(beta, "yz");

        wFlat.ApplyRotationOnSelf(yFlat);

        return wFlat;
    }

    // constructors
    public static E4Rotation Identity() {
        return new E4Rotation(Matrix4x4.identity);
    }

    public static E4Rotation Angles(float[] angles) { // yz xz xy xw yw zw
        Matrix4x4 alpha = Matrix4x4.identity;
        Matrix4x4 beta = Matrix4x4.identity;
        Matrix4x4 gamma = Matrix4x4.identity;
        Matrix4x4 theta = Matrix4x4.identity;
        Matrix4x4 lambda = Matrix4x4.identity;
        Matrix4x4 miu = Matrix4x4.identity;

        float[] calculations = new float[12];

        for (int i = 0; i < angles.Length; i++) {
            calculations[i * 2] = Mathf.Cos(angles[i]);
            calculations[(i * 2) + 1] = Mathf.Sin(angles[i]);
        }

        // basic 3d angles
        alpha.SetColumn(1, new Vector4(0, calculations[0], -calculations[1], 0));
        alpha.SetColumn(2, new Vector4(0, calculations[1], calculations[0], 0));

        beta.SetColumn(0, new Vector4(calculations[2], 0, -calculations[3], 0));
        beta.SetColumn(2, new Vector4(calculations[3], 0, calculations[2], 0));

        gamma.SetColumn(0, new Vector4(calculations[4], -calculations[5], 0, 0));
        gamma.SetColumn(1, new Vector4(calculations[5], calculations[4], 0, 0));

        // other angles needed for 4d
        theta.SetColumn(0, new Vector4(calculations[6], 0, 0, calculations[7]));
        theta.SetColumn(3, new Vector4(-calculations[7], 0, 0, calculations[6]));

        lambda.SetColumn(1, new Vector4(0, calculations[8], 0, calculations[9]));
        lambda.SetColumn(3, new Vector4(0, -calculations[9], 0, calculations[8]));

        miu.SetColumn(2, new Vector4(0, 0, calculations[10], calculations[11]));
        miu.SetColumn(3, new Vector4(0, 0, -calculations[11], calculations[10]));

        // calculating the final matrix
        Matrix4x4 orientation = alpha * beta * gamma * theta * lambda * miu;

        return new E4Rotation(orientation);
    }

    public static E4Rotation Plane(float angle, string axis) {
        Matrix4x4 rot = Matrix4x4.identity;

        int one = axis2index[axis[0].ToString()];
        int two = axis2index[axis[1].ToString()];

        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        rot[one, one] = cosAngle;
        rot[two, two] = cosAngle;
        rot[one, two] = -sinAngle;
        rot[two, one] = sinAngle;

        return new E4Rotation(rot);
    }

    public static E4Rotation Point(Vector4 vec) {
        Vector3 v = Cartesian2Spherical(vec);

        E4Rotation[] three = new E4Rotation[] {
            E4Rotation.Plane(v.x, "xz"),
            E4Rotation.Plane(v.y, "xy"),
            E4Rotation.Plane(v.z, "xw")
        };

        E4Rotation final = three[2];
        final.ApplyRotationOnSelf(three[1]);
        final.ApplyRotationOnSelf(three[0]);

        return final;
    }

    public static E4Rotation AnglePlane(float angle, Vector4 a, Vector4 b) {
        E4Rotation toA = E4Rotation.Point(a);
        E4Rotation AtoOrigin = toA.Copy();
        AtoOrigin.ReverseSelf();

        Vector4 v = AtoOrigin.RotateVector(b);

        E4Rotation flat = E4Rotation.flattenation(v);
        E4Rotation unflat = flat.Copy();
        unflat.ReverseSelf();

        // final wrapping
        E4Rotation final = AtoOrigin;
        final.ApplyRotationOnSelf(flat);

        final.ApplyRotationOnSelf(E4Rotation.Plane(angle, "xz"));

        final.ApplyRotationOnSelf(unflat);
        final.ApplyRotationOnSelf(toA);

        return final;
    }
    
    // basic use functions
    public Vector4 RotateVector(Vector4 v) {
        Matrix4x4 r = rotation;
        
        Vector4 v2 = new Vector4(
            (v[0] * r[0, 0]) + (v[1] * r[0, 1]) + (v[2] * r[0, 2]) + (v[3] * r[0, 3]),
            (v[0] * r[1, 0]) + (v[1] * r[1, 1]) + (v[2] * r[1, 2]) + (v[3] * r[1, 3]),
            (v[0] * r[2, 0]) + (v[1] * r[2, 1]) + (v[2] * r[2, 2]) + (v[3] * r[2, 3]),
            (v[0] * r[3, 0]) + (v[1] * r[3, 1]) + (v[2] * r[3, 2]) + (v[3] * r[3, 3])
        );

        return v2;
    }

    public void ApplyRotationOnSelf(E4Rotation r) {
        for (int i = 0; i < 4; i++) {
            Vector4 v = new Vector4(rotation[0, i], rotation[1, i], rotation[2, i], rotation[3, i]);
            v = r.RotateVector(v);

            rotation[0, i] = v.x;
            rotation[1, i] = v.y;
            rotation[2, i] = v.z;
            rotation[3, i] = v.w;
        }
    }

    public void ContinueRotationWithSelf(E4Rotation rot) {
        E4Rotation r = new E4Rotation(rot.rotation);
        r.ApplyRotationOnSelf(this);
        rotation = r.rotation;
    }

    public void ReverseSelf() {
        rotation = rotation.transpose;
    }

    public E4Rotation Copy() {
        return new E4Rotation(rotation);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
