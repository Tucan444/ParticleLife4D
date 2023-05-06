using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ingame : MonoBehaviour
{
    public TextMeshProUGUI friction;
    public Slider fs;

    public TextMeshProUGUI force;
    public Slider fos;

    public TextMeshProUGUI pressure;
    public Slider ps;

    // Update is called once per frame
    void Update()
    {
        friction.text = "friction: " + Mathf.Round(fs.value * 100) / 100f;
        force.text = "force: " + Mathf.Round(fos.value * 100) / 100f;
        pressure.text = "pressure: " + Mathf.Round(ps.value * 100) / 100f;
    }
}
