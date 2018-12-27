using UnityEngine;
using UnityEngine.UI;

public class TextPopUp : MonoBehaviour 
{
	
	Text component;
	float alpha;
	Color color;

	
	void Update()
	{
		transform.Translate(0,100*Time.deltaTime,0);
		alpha=alpha - Time.deltaTime*0.8f;
		component.color=new Color(color.r,color.g,color.b,alpha);
		
		if(alpha<=0)
			Destroy(gameObject);
	}
	
	public void assign(string newText, Color col,int fontSize)
	{
		transform.SetParent(GameObject.FindWithTag("canvas").transform);
		component=GetComponent<Text>();
		Debug.Log("3");
		component.color=col;
		color=component.color;
		alpha=1;
		component.text=newText;
		component.fontSize = fontSize;
		transform.rotation = Quaternion.identity;
	}
	
}
