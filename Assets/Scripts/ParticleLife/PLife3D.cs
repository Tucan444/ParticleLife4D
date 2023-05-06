using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PLife3D : MonoBehaviour
{
    // playfield range is 100 by 100
    [Range(1, 100000)] public int particlesAmount = 100;
    [Range(1, 50)] public int types = 6;

    public enum Spawn {
        uniform, dense, clusters, sphere, pillar, singularity
    };

    public Spawn spawn = Spawn.uniform;

    public bool bound = true;

    [Range(0.1f, 40)] public float maxRadi = 6;
    [Range(0, 1)] public float friction = 0.2f;
    [Range(0, 5)] public float force = 1;
    [Range(0, 5)] public float pressure = 1;

    [Range(0.1f, 4)] public float particleSize = 1;
    public bool velocityBrightness = false;
    
    public camera3D cam;
    public ComputeShader baseShader;
    public ComputeShader draw3D;
    public ComputeShader black;
    private RenderTexture renderTexture;

    [HideInInspector] public Relation[] relations;
    [HideInInspector] public Particle3D[] particles;

    Vector3 center = new Vector3(50, 50, 50);
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
                spawn = Spawn.sphere;
                break;
            case "Pillar":
                spawn = Spawn.pillar;
                break;
            case "Singularity":
                spawn = Spawn.singularity;
                break;
        }
        maxRadi = 0.1f + 39.9f * config.maxRadi;

        bound = config.bound;
        friction = config.friction;
        force = config.force;
        pressure = config.pressure;
        velocityBrightness = config.velocityBrightness;

        particleSize = 1;
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

    public Vector3 RandomSpherePoint() {
        Vector3 p = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        while (p.sqrMagnitude > 1) {
            p = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
        return p.normalized;
    }

    public void GetParticles() {
        Vector3[] centers = new Vector3[types];
        for (int i = 0; i < types; i++) {
            centers[i] = new Vector3(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));
        }

        Vector3 spherePoint;
        particles = new Particle3D[particlesAmount];
        for (int i = 0; i < particlesAmount; i++) {
            Particle3D p = new Particle3D();
            p.color = Random.Range(0, types);
            switch (spawn) {
                case Spawn.uniform:
                    p.position = new Vector3(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));
                    p.velocity = RandomSpherePoint();
                    break;

                case Spawn.dense:
                    spherePoint = RandomSpherePoint();
                    p.position = center + Random.Range(0f, 4f) * spherePoint;
                    p.velocity = -spherePoint;
                    break;

                case Spawn.clusters:
                    spherePoint = RandomSpherePoint();
                    p.position = centers[p.color] + Random.Range(0f, 12f) * spherePoint;
                    p.velocity = -spherePoint;
                    break;

                case Spawn.sphere:
                    spherePoint = RandomSpherePoint();
                    p.position = center + 30 * spherePoint;
                    p.velocity = Mathf.Pow(-1, Random.Range(0, 1)) * spherePoint;
                    break;

                case Spawn.pillar:
                    p.position = new Vector3(Random.Range(40f, 60f), Random.Range(40f, 60f), Random.Range(0f, 100f));
                    p.velocity = RandomSpherePoint();
                    break;

                case Spawn.singularity:
                    spherePoint = RandomSpherePoint();
                    p.position = center + Random.Range(0f, 0.2f) * spherePoint;
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
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*7);  // nets
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
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*7);  // nets
        particles_buffer.SetData(particles);
        draw3D.SetBuffer(0, "particles", particles_buffer);
        draw3D.SetInt("particlesN", particles.Length);

        draw3D.SetTexture(0, "Result", renderTexture);

        draw3D.SetFloat("particleSize", particleSize);
        draw3D.SetBool("velocityBrightness", velocityBrightness);

        draw3D.SetVector("position", cam.position);
        draw3D.SetMatrix("orientation", Matrix4x4.TRS(new Vector3(), cam.GetRotation(), new Vector3(1, 1, 1)));
        draw3D.SetFloat("fovx", cam.fovx);

        // dispatching
        draw3D.Dispatch(0, (int)Mathf.Ceil((float)particles.Length / 128f), 1, 1);

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
