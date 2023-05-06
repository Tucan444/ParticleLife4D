using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class menu : MonoBehaviour
{
    public Slider projectiles;
    public Slider types;
    public TMP_Dropdown spawn;
    public Slider maxRadi;

    public Toggle bound;
    public Slider friction;
    public Slider force;
    public Slider pressure;
    public Toggle velocityBrightness;

    Dictionary<string, int> spawnIds = new Dictionary<string, int>() {
        {"Uniform", 0},
        {"Dense", 1},
        {"Clusters", 2},
        {"Sphere", 3},
        {"Pillar", 4},
        {"Singularity", 5}
    };

    public void SaveValues() {
        config.projectilesAmount = (int)projectiles.value;
        config.types = (int)types.value;
        config.spawn = spawn.captionText.text;
        config.maxRadi = maxRadi.value;

        config.bound = bound.isOn;
        config.friction = friction.value;
        config.force = force.value;
        config.pressure = pressure.value;
        config.velocityBrightness = velocityBrightness.isOn;
    }

    void LoadData() {
        projectiles.value = config.projectilesAmount;
        types.value = config.types;
        spawn.value = spawnIds[config.spawn];
        maxRadi.value = config.maxRadi;

        bound.isOn = config.bound;
        friction.value = config.friction;
        force.value = config.force;
        pressure.value = config.pressure;
        velocityBrightness.isOn = config.velocityBrightness;
    }

    void Start() {
        LoadData();
        Cursor.visible = true;
    }

    public void Quit() {
        Application.Quit();
    }

    public void play2D() {
        config.game = true;
        SaveValues();
        SceneManager.LoadScene(1);
    }

    public void play3D() {
        config.game = true;
        SaveValues();
        SceneManager.LoadScene(2);
    }

    public void play4D() {
        config.game = true;
        SaveValues();
        SceneManager.LoadScene(3);
    }
}
