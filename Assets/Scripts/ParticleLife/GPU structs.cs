using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Relation {
    public float radius;
    public Vector3 attraction;
    public float repulsionSlope;
}

public struct Particle2D {
    public int color;
    public Vector2 position;
    public Vector2 velocity;
}

public struct Particle3D {
    public int color;
    public Vector3 position;
    public Vector3 velocity;
}

public struct Particle4D {
    public int color;
    public Vector4 position;
    public Vector4 velocity;
}
