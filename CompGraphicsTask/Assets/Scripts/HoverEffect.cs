using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverEffect : MonoBehaviour
{

    Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(originalPos.x, originalPos.y + Mathf.Sin(Time.time*2f)/4f, originalPos.z);
        transform.rotation = Quaternion.Euler(0, Time.time*20f, 0);

    }
}
