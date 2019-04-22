using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    public RectTransform LoadingCircleFill;
    // Start is called before the first frame update
    void Update()
    {
        var rotation = LoadingCircleFill.localEulerAngles;
        rotation.z += (360 * 1.39f) * Time.deltaTime;
        if(rotation.z > 180)
        {
            rotation.z -= 360;
        }

        LoadingCircleFill.localEulerAngles = rotation;
    }
}
