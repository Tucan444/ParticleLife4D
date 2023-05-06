using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ParticleLife2D : MonoBehaviour
{
    // playfield range is 100 by 100
    [Range(1, 100000)] public int particlesAmount = 100;
    [Range(1, 50)] public int types = 6;

    public enum Spawn {
        uniform, dense, clusters, ring, pillar, singularity
    };

    public Spawn spawn = Spawn.uniform;

    public bool bound = true;

    [Range(0.1f, 20)] public float maxRadi = 2;
    [Range(0, 1)] public float friction = 0.2f;
    [Range(0, 5)] public float force = 1;
    [Range(0, 5)] public float pressure = 1;

    [Range(0.1f, 10)] public float particleSize = 5;
    public bool velocityBrightness = false;

    public ComputeShader baseShader;
    public ComputeShader draw2D;
    public ComputeShader black;
    private RenderTexture renderTexture;

    [HideInInspector] public Relation[] relations;
    [HideInInspector] public Particle2D[] particles;

    // controls
    Vector2 position = new Vector2();
    float scale = 1;
    Vector3 dir = new Vector3();
    bool useMovement = true;

    Vector2 center = new Vector2(50, 50);
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
                spawn = Spawn.ring;
                break;
            case "Pillar":
                spawn = Spawn.pillar;
                break;
            case "Singularity":
                spawn = Spawn.singularity;
                break;
        }
        maxRadi = 0.1f + 19.9f * config.maxRadi;

        bound = config.bound;
        friction = config.friction;
        force = config.force;
        pressure = config.pressure;
        velocityBrightness = config.velocityBrightness;

        particleSize = 5;
    }

    void SaveData() {
        config.bound = bound;
        config.friction = friction;
        config.force = force;
        config.pressure = pressure;
        config.velocityBrightness = velocityBrightness;
    }

    void Update() {
        if (useMovement) {
            position += new Vector2(dir.x, dir.y) * Time.deltaTime * (300f / scale);
            if (dir.z > 0) {
                scale *= 1 + (0.5f * Time.deltaTime);
            } else if (dir.z < 0) {
                scale *= 1 - (0.5f * Time.deltaTime);
            }
        }
    }

    public void GetParticles() {
        Vector2[] centers = new Vector2[types];
        for (int i = 0; i < types; i++) {
            centers[i] = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
        }

        float angle;
        particles = new Particle2D[particlesAmount];
        for (int i = 0; i < particlesAmount; i++) {
            Particle2D p = new Particle2D();
            p.color = Random.Range(0, types);
            switch (spawn) {
                case Spawn.uniform:
                    p.position = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    break;

                case Spawn.dense:
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.position = center + (Random.Range(0f, 5f) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                    p.velocity = -new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    break;

                case Spawn.clusters:
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.position = centers[p.color] + (Random.Range(0f, 15f) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                    p.velocity = -new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    break;

                case Spawn.ring:
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.position = center + (30 * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                    p.velocity = Mathf.Pow(-1, Random.Range(0, 1)) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    break;

                case Spawn.pillar:
                    p.position = new Vector2(Random.Range(40f, 60f), Random.Range(0f, 100f));
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    break;

                case Spawn.singularity:
                    angle = Random.Range(0f, Mathf.PI * 2);
                    p.position = center + (Random.Range(0f, 0.3f) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                    p.velocity = -new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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

    public void ToggleMovement() {
        useMovement = !useMovement;
    }

    public void OnMovement(InputAction.CallbackContext context) {
        dir = -context.ReadValue<Vector3>();
        dir.z *= -1;
    }

    private void BaseShader() {
        // spheres
        ComputeBuffer relations_buffer = new ComputeBuffer(relations.Length, sizeof(float)*5); // relations
        relations_buffer.SetData(relations);
        baseShader.SetBuffer(0, "relations", relations_buffer);
        baseShader.SetInt("typesN", types);

        // particles
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*5);  // nets

        particles_buffer.SetData(particles);
        baseShader.SetBuffer(0, "particles", particles_buffer);
        baseShader.SetInt("particlesN", particles.Length);
        
        // other
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
        // spheres

        //nets
        ComputeBuffer particles_buffer = new ComputeBuffer(particles.Length, sizeof(float)*5);  // nets
        particles_buffer.SetData(particles);
        draw2D.SetBuffer(0, "particles", particles_buffer);
        draw2D.SetInt("particlesN", particles.Length);

        draw2D.SetTexture(0, "Result", renderTexture);

        draw2D.SetInt("particleSize", Mathf.Max(1, (int)(particleSize * scale)));
        draw2D.SetFloat("sizeSquared", (0.5f * (float)particleSize * scale) * (0.5f * (float)particleSize * scale));
        draw2D.SetInt("halfSize", (int)((float)particleSize * 0.5f * scale));
        draw2D.SetBool("velocityBrightness", velocityBrightness);

        draw2D.SetVector("position", position);
        draw2D.SetFloat("scale", scale);

        // dispatching
        draw2D.Dispatch(0, (int)Mathf.Ceil((float)particles.Length / 128f), 1, 1);

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
