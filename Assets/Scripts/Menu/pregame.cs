using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class pregame : MonoBehaviour
{

    public TextMeshProUGUI particles;
    public Slider ps;

    public TextMeshProUGUI types;
    public Slider ts;

    public TextMeshProUGUI maxRadi;
    public Slider mrs;

    // Update is called once per frame
    void Update()
    {
        particles.text = "particles: " + ps.value;
        types.text = "types: " + ts.value;
        maxRadi.text = "maxRadi: " + Mathf.Round(mrs.value * 100) / 100f;
    }
}
