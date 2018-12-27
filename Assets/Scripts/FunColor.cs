using UnityEngine;
using UnityEngine.UI;

public class FunColor : MonoBehaviour 
{
	Color curColor;
	public Color targetColor;
	Text txt;
	
	void Awake()
	{
		txt = GetComponent<Text>();
		curColor = txt.color;
	}
	
	void Update()
	{
		txt.color = Color.Lerp(curColor, targetColor, Mathf.PingPong(Time.time, 1));
	}
	
}
