using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Update()
    {
		ReflectionProbe probe = GetComponent<ReflectionProbe>();
		Debug.Log(probe.textureHDRDecodeValues);
    }
}