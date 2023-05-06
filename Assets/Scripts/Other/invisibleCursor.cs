using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invisibleCursor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    void Update() {
        Cursor.visible = false;
    }

    void OnDisable() {
        Cursor.visible = true;
    }
}
