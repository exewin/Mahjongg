using UnityEngine;
using System.Collections;


public class Block : MonoBehaviour 
{
	
	public Cache cache;
	public int type;

	public Renderer rend;
	
	Vector3 startPos;
	Quaternion startRot;
	
	bool positioned;
	
	public Renderer blockRend; //htp
	
	
	void Start()
	{
		startPos = transform.position;
		startRot = transform.rotation;
		
		transform.position=new Vector3(Random.Range(-10f,10f),Random.Range(-10f,10f),Random.Range(-10f,10f));
		transform.eulerAngles=new Vector3(Random.Range(-10f,10f),Random.Range(-10f,10f),Random.Range(-10f,10f));
		
		StartCoroutine(Interpolate(3f,transform.position,transform.rotation));
		
	}
	
	
	IEnumerator Interpolate(float overTime, Vector3 objpos, Quaternion objrot)
	{
		Vector3 pos = objpos;
		Quaternion rot = objrot;
		float startTime = Time.time;
		while(Time.time < startTime + overTime)
		{
			transform.position = Vector3.Slerp(pos, startPos,(Time.time - startTime)/overTime);
			transform.rotation = Quaternion.Slerp(rot, startRot, (Time.time - startTime)/overTime);
			yield return null;
		}
		transform.position = startPos;
		transform.rotation = startRot;
		positioned=true;
		//if(EasyModeStatic.easyMode==2) EASY MODE ALL TIME
			checkStateUltra();
		
	}

	
	void OnMouseDown()
	{
		if(positioned && !cache.paused)
			if(checkState())
			{
				cache.soundMe(true);
				cache.receiveType(this);
			}
			else
			{
				cache.soundMe(false);
			}
	}
	
	
	void OnMouseEnter()
	{
		if(positioned && !cache.paused)
			cache.viewBlock(this);
	}
	
	void OnMouseExit()
	{
		if(positioned && !cache.paused)
			cache.clearView();
	}
	
	public void checkStateUltra()
	{
		if(checkState())
			blockRend.material.color = new Color(0.8F, 1F, 0.8F);
		else
			blockRend.material.color = new Color(1F, 0.8F, 0.8F);
	}
	
	public bool checkState()
	{
		if (Physics.Raycast(transform.position, Vector3.left, 1))
            if (Physics.Raycast(transform.position, -Vector3.left, 1))
				return false;
			
		if (Physics.Raycast(transform.position, Vector3.forward, 1))
            if (Physics.Raycast(transform.position, -Vector3.forward, 1))
				return false;
			
		return true;
	}
	
	public void assign(int typ, Texture2D texture)
	{
		type=typ;
		rend.material.mainTexture = texture;
	}

}
