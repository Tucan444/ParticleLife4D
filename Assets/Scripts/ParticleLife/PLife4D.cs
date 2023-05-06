using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PLife4D : MonoBehaviour
{
    // playfield range is 100 by 100
    [Range(1, 100000)] public int particlesAmount = 100;
    [Range(1, 50)] public int types = 6;

    public enum Spawn {
        uniform, dense, clusters, hypersphere, pillar, singularity
    };

    public Spawn spawn = Spawn.uniform;

    public bool bound = true;

    [Range(0.1f, 60)] public float maxRadi = 10;
    [Range(0, 1)] public float friction = 0.2f;
    [Range(0, 5)] public float force = 1;
    [Range(0, 5)] public float pressure = 1;

    [Range(0.1f, 4)] public float particleSize = 1;
    public bool velocityBrightness = false;
    
    public camera3D cam;
    public camera4D hypercam;
    public ComputeShader baseShader;
    public ComputeShader draw4D;
    public ComputeShader black;
    private RenderTexture renderTexture;

    [HideInInspector] public Relation[] relations;
    [HideInInspector] public Particle4D[] particles;

    Vector4 center = new Vector4(50, 50, 50, 50);
    // Start is called before the first frame update
    void Start()
    {
        LoadData();
        GetRelations();
        GetParticles();

        // initializing texture
        renderTexture = new RenderTexture(1280, 720, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
    }
    
    void LoadData() {
        if (!config.game) {return;}

        particlesAmount = config.projectilesAmount;
        types = config.types;
        switch (config.spawn) {
            case "Uniform":
                spawn = Spawn.uniform;
                break;
            case "Dense":
                spawn = Spawn.dense;
                break;
            case "Clusters":
                spawn = Spawn.clusters;
                break;
            case "Sphere":
                spawn = Spawn.hypersphere;
                break;
            case "Pillar":
                spawn = Spawn.pillar;
                break;
            case "Singularity":
                spawn = Spawn.singularity;
                break;
        }
        maxRadi = 0.1f + 59.9f * config.maxRadi;

        bound = config.bound;
        friction = config.friction;
        force = config.force;
        pressure = config.pressure;
        velocityBrightness = config.velocityBrightness;

        particleSize = 0.8f;
    }

    void SaveData() {
        config.bound = bound;
        config.friction = friction;
        config.force = force;
        config.pressure = pressure;
        config.velocityBrightness = velocityBrightness;
    }

    void Update() {
    }

    public Vector4 RandomSpherePoint() {
        Vector4 p = new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        while (p.sqrMagnitude > 1) {
            p = new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
        return p.normalized;
    }

    public void GetParticles() {
        Vector4[] centers = new Vector4[types];
        for (int i = 0; i < types; i++) {
            centers[i] = new Vector4(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));
        }

        Vector4 spherePoint;
        particles = new Particle4D[particlesAmount];
        for (int i = 0; i < particlesAmount; i++) {
            Particle4D p = new Particle4D();
            p.color = Random.Range(0, types);
            switch (spawn) {
                case Spawn.uniform:
                    p.position = new Vector4(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));
                    p.velocity = RandomSpherePoint();
                    break;

                case Spawn.dense:
                    spherePoint = RandomSpherePoint();
                    p.position = center + Random.Range(0f, 3f) * spherePoint;
                    p.velocity = -spherePoint;
                    break;

                case Spawn.clusters:
                    spherePoint = RandomSpherePoint();
                    p.position = centers[p.color] + Random.Range(0f, 10f) * spherePoint;
                    p.velocity = -spherePoint;
                    break;

                case Spawn.hypersphere:
                    spherePoint = RandomSpherePoint();
                    p.position = center + 30 * spherePoint;
                    p.velocity = Mathf.Pow(-1, Random.Range(0, 1)) * spherePoint;
                    break;

                case Spawn.pillar:
                    p.position = new Vector4(Random.Range(40f, 60f), Random.Range(40f, 60f), Random.Range(40f, 60f), Random.Range(0f, 100f));
                    p.velocity = RandomSpherePoint();
                    break;

                case Spawn.singularity:
                    spherePoint = RandomSpherePoint();
                    p.position = center + Random.Range(0f, 0.15f) * spherePoint;
                    p.velocity = -spherePoint;
                    break;
            }
            particles[i] = p;
        }
    }

    public void GetRelations() {
        relations = new Relation[types * types];

        for (int x = 0; x < types; x++) {
            for (int y = 0; y < types; y++) {
                Relation r = new Relation();
                r.radius = Random.Range(0.05f, maxRadi);

                float apoint = r.radius * Random.Range(0.55f, 0.7f);
                r.attraction = new Vector3(Random.Range(-1f, 1f), apoint, 1f / (r.radius - apoint));
                r.repulsionSlope = 1.5f / (apoint - (r.radius - apoint));
                relations[Pos2Index(x, y)] = r;
            }
        }
    }

    public int Pos2Index(int x, int y) {
        return x + (y*types);
    }

    public void Quit() {
        SaveData();
        SceneManager.LoadScene(0);
    }

    private void BaseShader() {
        // spheres
        ComputeBuffer relations_buffer = new ComputeBuffer(relations.Length, sizeof(float)*5); // relations
        relations_buffer.SetData(relations);
        baseShader.SetBuffer(0, "relations", relations_buffer);
        baseShader.SetInt("typesN", types);

        //nets
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*9);
        particles_buffer.SetData(particles);
        baseShader.SetBuffer(0, "particles", particles_buffer);
        baseShader.SetInt("particlesN", particles.Length);

        baseShader.SetFloat("friction", friction);
        baseShader.SetFloat("force", force);
        baseShader.SetFloat("pressure", pressure);

        baseShader.SetBool("bound", bound);

        // dispatching
        baseShader.Dispatch(0, (int)Mathf.Ceil((float)particles.Length / 128f), 1, 1);

        // disposing of buffers
        relations_buffer.Dispose();
        particles_buffer.GetData(particles);
        particles_buffer.Dispose();
    }

    private void DrawShader() {
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*9);  // nets
        particles_buffer.SetData(particles);
        draw4D.SetBuffer(0, "particles", particles_buffer);
        draw4D.SetInt("particlesN", particles.Length);

        draw4D.SetTexture(0, "Result", renderTexture);

        draw4D.SetFloat("particleSize", particleSize);
        draw4D.SetBool("velocityBrightness", velocityBrightness);

        // 4d cam
        draw4D.SetVector("hpos", hypercam.GetPosition());
        draw4D.SetMatrix("hrot", hypercam.GetRotation());
        draw4D.SetFloat("hfovx", hypercam.fovx);

        // 3d cam
        draw4D.SetVector("position", cam.position - new Vector3(50, 50, 50));
        draw4D.SetMatrix("orientation", Matrix4x4.TRS(new Vector3(), cam.GetRotation(), new Vector3(1, 1, 1)));
        draw4D.SetFloat("fovx", cam.fovx);

        // dispatching
        draw4D.Dispatch(0, (int)Mathf.Ceil((float)particles.Length / 128f), 1, 1);

        // disposing of buffers
        particles_buffer.Dispose();
    }

    public void Black() {
        black.SetTexture(0, "Result", renderTexture);
        black.Dispatch(0, 80, 80, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        BaseShader();
        DrawShader();

        Graphics.Blit(renderTexture, dest);

        Black();
    }
}
