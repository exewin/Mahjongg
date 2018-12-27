using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingTxt : MonoBehaviour 
{

	Text comp;
	Color tcol;
	float alpha=0;
	public float fadingSpeed=0.8f;
	
	void Awake()
	{
		comp = GetComponent<Text>();
	}

	void OnEnable () 
	{
		alpha=0;
		tcol = comp.color;
		comp.color=new Color(tcol.r,tcol.g,tcol.b,alpha);	
		
	}
	
	void Update()
	{
		if(alpha<1)
		{
			alpha=alpha + Time.deltaTime*fadingSpeed;
			comp.color=new Color(tcol.r,tcol.g,tcol.b,alpha);	
		}
	}
}