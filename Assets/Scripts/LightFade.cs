using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFade : MonoBehaviour 
{
	[SerializeField]
	private float duration;
	private Light myLight;

    private void Start()
    {
        myLight = GetComponent<Light>();
    }

	private void Update()
	{
		myLight.intensity -=(Time.deltaTime*(1/duration));
	}

}
