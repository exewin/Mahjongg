using UnityEngine;
using UnityEngine.UI;

public class EasyModeStatic : MonoBehaviour 
{
	public static int easyMode = 1;
	public Text x;
	
	public void clicker()
	{
		if(easyMode==1)
		{
			easyMode=2;
			x.text = "X";
		}
		else
		{
			easyMode=1;
			x.text = "";
		}
	}
	
}