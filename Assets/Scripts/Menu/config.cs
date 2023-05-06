using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class config : MonoBehaviour
{
    public static bool game = false;

    public static int projectilesAmount = 25000;
    public static int types = 12;
    public static string spawn = "Uniform";
    public static float maxRadi = 0.4f;

    public static bool bound = true;
    public static float friction = 0.25f;
    public static float force = 1;
    public static float pressure = 1;
    public static bool velocityBrightness = false;
}
