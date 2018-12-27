using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuFunctions : MonoBehaviour 
{
	
	
	public void changeScene(string title)
	{
		SceneManager.LoadScene(title, LoadSceneMode.Single);
	}

}
