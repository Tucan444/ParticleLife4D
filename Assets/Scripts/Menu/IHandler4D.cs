using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IHandler4D : MonoBehaviour
{
    public GameObject c;
    public Toggle b;
    public Slider f;
    public Slider fo;
    public Slider p;
    public Toggle vb;

    public TextMeshProUGUI psize;
    public Slider ps;
    public PLife4D plife;

    void Start() {
        b.isOn = config.bound;
        f.value = config.friction;
        fo.value = config.force;
        p.value = config.pressure;
        vb.isOn = config.velocityBrightness;
    }

    public void Hide() {
        Cursor.visible = !Cursor.visible;
        c.SetActive(Cursor.visible);
    }

    // Update is called once per frame
    void Update()
    {
        psize.text = "psize: " + Mathf.Round(ps.value * 100) / 100f;
    }

    public void PsizeChange(float value) {
        plife.particleSize = value;
    }

    public void FrictionChange(float value) {
        plife.friction = value;
    }

    public void ForceChange(float value) {
        plife.force = value;
    }

    public void PressureChange(float value) {
        plife.pressure = value;
    }

    public void BoundChange(bool value) {
        plife.bound = value;
    }

    public void VBrightnessChange(bool value) {
        plife.velocityBrightness = value;
    }
}

