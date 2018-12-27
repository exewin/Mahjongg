using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Cache : MonoBehaviour 
{
	
	public struct SafeInt 
	{
		private int offset;
		private int value;

		public SafeInt (int value = 0) {
			offset = Random.Range(-1000, +1000);
			this.value = value + offset;
		}
		
		public int GetValue ()
		{
			return value - offset;
		}

		public void Dispose ()
		{
			offset = 0;
			value = 0;
		}

		public override string ToString()
		{
			return GetValue().ToString();
		}

		public static SafeInt operator +(SafeInt f1, SafeInt f2) {
			return new SafeInt(f1.GetValue() + f2.GetValue());
		}
	}
	
	public struct SafeFloat 
	{
    private float offset;
    private float value;

	public SafeFloat (float value = 0) {
        offset = Random.Range(-1000, +1000);
        this.value = value + offset;
	}
	
	public float GetValue ()
    {
        return value - offset;
    }

    public void Dispose ()
    {
        offset = 0;
        value = 0;
    }

    public override string ToString()
    {
        return GetValue().ToString();
    }

    public static SafeFloat operator -(SafeFloat f1, float f2) {
        return new SafeFloat(f1.GetValue() - f2);
    }
	

	}
	
	
	Block block1;
	Block block2;
	
	public Image[] musicSoundObjs;
	public Sprite[] musicSoundSprites;
	public AudioSource musicPlayer;
	public AudioSource soundPlayer;
	
	bool musicMuted;
	bool soundMuted;
	
	public GameObject rotator;
	bool rotating;
	
	public GameObject effect;
	public GameObject boomEffect;
	
	public GameObject bonusText;
	
	public GameObject shuffleScreen;
	public GameObject shuffleBlock;
	public GameObject gameOverScreen;
	public GameObject nextLevelScreen;
	public Text nextLevelScore;
	public GameObject victoryScreen;
	public Text lastLevelScore;
	public Text lastLevelScoreTime;
	public GameObject pauseScreen;
	public GameObject lastScreen;
	public Text lastScreenScore;
	
	public GameObject highlightSelected;
	public GameObject highlightViewed;
	
	public RawImage UI_viewed;
	public RawImage UI_selected;
	
	public Text scoreText;
	SafeInt score;
	int allPairs;
	
	public AudioClip[] sound;
	public AudioClip soundOk;
	public AudioClip soundError;
	public AudioClip soundClip;
	public AudioClip soundShuffle;
	public AudioClip soundLevelPassed;
	
	AudioSource audioSource;
	
	public Texture2D emptyTexture;
	
	public Texture2D[] textures;
	
	
	public GameObject[] level;
	byte curLevel;
	public byte[] levelPairs;
	
	float bonusTimer=0;
	SafeInt bonusMultiplier;
	
	
	GameObject[] blocks;
	List<int> clickableTypes = new List<int>();
	List<Block> blockScript = new List<Block>();
	
	SafeFloat time;
	public Text timeText;
	protected bool allowTimer;
	protected bool allowBonusTimer;
	
	
	
	public bool paused;
	bool coroutined;
	
	public bool kongBuild;
	public KongregateAPIBehaviour kongComp;
	
	
	void Awake() 
	{
        Application.targetFrameRate = 60;
    }
	
	void Start()
	{
		if(PlayerPrefs.HasKey("muteM"))
			if(PlayerPrefs.GetInt("muteM")==1)
				clickMusic();
			
		if(PlayerPrefs.HasKey("muteS"))
			if(PlayerPrefs.GetInt("muteS")==1)
				clickSound();
		
		
		bonusMultiplier=new SafeInt(0);
		time=new SafeFloat(540);
		highlightSelected.transform.position=new Vector3(0,0,-100);
		highlightViewed.transform.position=new Vector3(0,0,-100);
		audioSource = GetComponent<AudioSource>();
	
		newLevel();
	}
	
	void Update()
	{
		
		if(Input.GetKeyDown(KeyCode.Escape) && !anyScreenActive())
		{
			clickPause();
		}
		
		if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))
		{
			rotMe(true);
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))
		{
			rotMe(false);
		}
		
		if(bonusTimer>0)
		{
			bonusTimer-=Time.deltaTime*1;
			if(bonusTimer<=0)
			{
				bonusTimer=0;
				bonusMultiplier=new SafeInt(0);
			}
		}
		
		if(time.GetValue()>0&&allowTimer)
		{
			time=time-Time.deltaTime*1;
			int normalTime = (int)time.GetValue();
			timeText.text = "Time: "+(normalTime/60)+":"+ (normalTime%60<10?"0" : "")+normalTime%60;
		}
		else if(time.GetValue()<=0)
		{
			StartCoroutine(gameOver());
		}
		
	}
	
	void newLevel()
	{
		clearRot();
		level[curLevel].SetActive(true);
		
		blocks = GameObject.FindGameObjectsWithTag("block");
		allPairs=blocks.Length/2;
		//protection check
		if(blocks.Length%2==1)
		{
			Debug.Log("Number of blocks is not odd.");
			return;
		}
		
		for(int i=0; i < blocks.Length; i++)
		{
			blockScript.Add(blocks[i].GetComponent<Block>());
		}
		randomize(blockScript);
		
		int r=0;
		for(int i=0; i < blocks.Length; i++)
		{
			if(i%(levelPairs[curLevel]*2)==0&&i!=0)
				r++;
			
			blockScript[i].assign(r,textures[r]);
		}
		
		nextLevelScreenActive(false);
		
		//wait secs for blocks to position
		
		StartCoroutine(waiter());
	}

	void matchBlocks()
	{
		int bonusint=bonusMultiplier.GetValue();
		
		GameObject btextScore = Instantiate(bonusText,Input.mousePosition+new Vector3(0,30,0),Quaternion.identity);
		
	
		if(bonusint>=6)
		{
			GameObject btext = Instantiate(bonusText,Input.mousePosition,Quaternion.identity);
			if(bonusint>=4&&bonusint<=10)
			{
				btext.GetComponent<TextPopUp>().assign("Good",Color.yellow,20);
				updateScore(10+bonusMultiplier.GetValue()*2, btextScore);
			}
			else if(bonusint>=11&&bonusint<=18)
			{
				btext.GetComponent<TextPopUp>().assign("Great",Color.blue,25);
				updateScore(10+bonusMultiplier.GetValue()*3, btextScore);
			}
			else if(bonusint>=19&&bonusint<=28)
			{
				btext.GetComponent<TextPopUp>().assign("Splendid",Color.green,30);
				updateScore(10+bonusMultiplier.GetValue()*4, btextScore);
			}
			else if(bonusint+1==allPairs)
			{
				audioSource.PlayOneShot(sound[1],1);
				Instantiate(boomEffect,block2.gameObject.transform.position,transform.rotation);
				
				btext.GetComponent<TextPopUp>().assign("FLAWLESS",Color.white,50);
				updateScore(100+bonusMultiplier.GetValue()*6, btextScore);
			}
			else
			{
				btext.GetComponent<TextPopUp>().assign("Excellent",Color.magenta,35);
				updateScore(10+bonusMultiplier.GetValue()*5, btextScore);
			}
		}
		else
		{
			updateScore(10+bonusMultiplier.GetValue(), btextScore);
		}
		
		
		bonusTimer=3f;
		bonusMultiplier+=new SafeInt(1);

		audioSource.PlayOneShot(sound[0],1);
		
		Instantiate(effect,block1.gameObject.transform.position,transform.rotation);
		Instantiate(effect,block2.gameObject.transform.position,transform.rotation);
		
		blockScript.Remove(block1);
		blockScript.Remove(block2);
		
		/*
		NOTE:
			since Destroy(gameObject) driven me crazy, I decided
			just to throw blocks out of screen and change their tag.
			
			//Destroy(block1.gameObject)
			//Destroy(block2.gameObject)
			
			Here lies two hours of sorrow and madness	
		*/
		
		block1.gameObject.transform.position=new Vector3(999,999,999);
		block2.gameObject.transform.position=new Vector3(999,999,999);
		block1.gameObject.tag="defeated";
		block2.gameObject.tag="defeated";
		
		
		UI_selected.texture = emptyTexture;
		UI_viewed.texture = emptyTexture;
		
		highlightSelected.transform.position=new Vector3(0,0,-100);
		highlightViewed.transform.position=new Vector3(0,0,-100);
		
		block1=null;
		block2=null;
		
		//any blocks left?
		if(anyBlocks())
		{
			//check possible moves
			checkMoves();
		}
	}
	
	void updateScore(int addScore, GameObject bts)
	{
		addScore/=EasyModeStatic.easyMode;
		score=score + new SafeInt(addScore);
		scoreText.text = "Score: "+score;
		
		bts.GetComponent<TextPopUp>().assign("+"+addScore,Color.white,18);
		
	}
	
	void updateScore(int addScore, Text txt)
	{
		score=score + new SafeInt(addScore);
		scoreText.text = "Score: "+score;
		
		txt.text = "+"+addScore;
		
	}
	
	List<Block> randomize(List<Block> array) 
	{
		for (int i = array.Count - 1; i > 0; i--) 
		{
			int j = Random.Range(0,(i + 1));
			Block temp = array[i];
			array[i] = array[j];
			array[j] = temp;
		}
		return array;
	}
	
	void clearRot()
	{
		rotator.transform.rotation = Quaternion.identity;
	}
	
	bool anyBlocks()
	{
		blocks = GameObject.FindGameObjectsWithTag("block");
		
		//htp
		if(EasyModeStatic.easyMode==2)
			for(int i = 0; i<blockScript.Count; i++)
			{
				blockScript[i].checkStateUltra();
			}
		//
		
		if(blocks.Length==0)
		{
			curLevel++;
			audioSource.PlayOneShot(soundLevelPassed,1);
			if(curLevel>=level.Length)
			{
				updateScore(curLevel*500,lastLevelScore);
				StartCoroutine(lastLevelPassed());
			}
			else
			{
				updateScore(curLevel*500,nextLevelScore);
				StartCoroutine(changeLevel());
			}
			
			return false;

		}
		return true;
	}
	
	void checkMoves()
	{
		clickableTypes = new List<int>();
		for(int i=0; i < blockScript.Count; i++)
		{
			if(blockScript[i].checkState())
			{
				for(int o=0; o < clickableTypes.Count; o++)
				{
					if(blockScript[i].type==clickableTypes[o])
					{
						shuffleScreenActive(false);
						coroutined=false;
						
						return;
					}
				}
				clickableTypes.Add(blockScript[i].type);
			}
		}
		StartCoroutine(shuffleBlocks());
	}
	
	#region publicVoids
	
	public void receiveType(Block block)
	{
		highlightSelected.transform.position = block.transform.position;
		UI_selected.texture = block.rend.material.mainTexture;
		if(block1==null)
		{
			block1=block;
		}
		else
		{
			if(block1==block)
			{
				return;
			}
			block2=block;
			if(block1.type==block2.type)
			{
				matchBlocks();
			}
			else
			{
				block1=block;
				block2=null;
			}
		}
	}
	
	public void soundMe(bool which)
	{
		audioSource.pitch = 1;
		if(which)
			audioSource.PlayOneShot(soundOk,0.5f);
		else
			audioSource.PlayOneShot(soundError,0.5f);
	}
	
	public void goToMenu()
	{
		lastScreenActive(true);
		lastScreenScore.text = ""+score.GetValue();
		if(kongBuild)
		{
			kongComp.SendScore(score.GetValue());
		}
	}
	
	public void realGoToMenu()
	{
		SceneManager.LoadScene("Menu", LoadSceneMode.Single);
	}
	
	public void viewBlock(Block block)
	{
		UI_viewed.texture = block.rend.material.mainTexture;
		highlightViewed.transform.position = block.gameObject.transform.position;
	}
	
	public void clearView()
	{
		highlightViewed.transform.position=new Vector3(0,0,-100);
		UI_viewed.texture = emptyTexture;
	}

	public void rotMe(bool left)
	{
		if(left)
		{
			StartCoroutine(rotateMe(Vector3.up * (-90)));
		}
		else
		{
			StartCoroutine(rotateMe(Vector3.up * 90));
		}
	}
	
	#endregion
	
	#region IEnumerators
	
	IEnumerator waiter()
	{
		allowTimer=false;
		yield return new WaitForSeconds(3);
		checkMoves();
		allowTimer=true;
	}

	IEnumerator shuffleBlocks()
	{
		shuffleScreenActive(true);	
	
		if(!coroutined)
		{
			audioSource.PlayOneShot(soundShuffle,1);
			yield return new WaitForSeconds(2);
			coroutined=true;
		}
		
		int[] shufflada = new int[blockScript.Count];
		for (int q = 0; q < shufflada.Length; q++)
		{
			shufflada[q]=blockScript[q].type;
		}	
		
		
		for (int i = shufflada.Length - 1; i > 0; i--) 
		{
			int j = Random.Range(0,(i + 1));
			int temp = shufflada[i];
			shufflada[i] = shufflada[j];
			shufflada[j] = temp;
		}
		
		for (int w = 0; w < shufflada.Length; w++)
		{
			blockScript[w].assign(shufflada[w],textures[shufflada[w]]);
		}
			
		checkMoves();
		
	}
	
	IEnumerator changeLevel()
	{
		//sound
		nextLevelScreenActive(true);
		yield return new WaitForSeconds(3);
		newLevel();
	}
	
	IEnumerator lastLevelPassed()
	{
		allowTimer=false;
		victoryScreenActive(true);
		yield return new WaitForSeconds(3);
		
		int timeToInt = (int)time.GetValue();
		int i=0;
		while(i!=timeToInt)
		{
			lastLevelScoreTime.text = "+"+(i*10);
			yield return new WaitForSeconds(0.02f);
			if(i%3==0) //just to reduce noise
				audioSource.PlayOneShot(soundClip,1);
			i++;
		}
		
		updateScore(i*10,lastLevelScoreTime);
		
	yield return new WaitForSeconds(1);
	goToMenu();
	
	}
	
	IEnumerator gameOver()
	{
		//sound
		gameOverScreenActive(true);
		yield return new WaitForSeconds(3);
		goToMenu();
		
	}
	
	IEnumerator rotateMe(Vector3 byAngles) 
	{
		if(!rotating)
		{
			rotating=true;
			var fromAngle = rotator.transform.rotation;
			var toAngle = Quaternion.Euler(rotator.transform.eulerAngles + byAngles);
			for(float t = 0f; t <= 1; t += Time.deltaTime/.5f) 
			{
				rotator.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
				yield return null;
			}
			rotator.transform.rotation = toAngle;
			rotating=false;
		}
	}
	
	#endregion
	
	#region screens
	
	bool anyScreenActive()
	{
		if(allowTimer || pauseScreen.active)
			return false;
		else
			return true;
	}
	
	void shuffleScreenActive(bool mode)
	{
		allowBonusTimer=!mode;
		allowTimer=!mode;
		shuffleScreen.SetActive(mode);
		shuffleBlock.SetActive(mode);
	}
	
	void gameOverScreenActive(bool mode)
	{
		allowTimer=!mode;
		gameOverScreen.SetActive(mode);
	}
	
	void nextLevelScreenActive(bool mode)
	{
		allowTimer=!mode;
		nextLevelScreen.SetActive(mode);
	}
	
	void victoryScreenActive(bool mode)
	{
		allowTimer=!mode;
		victoryScreen.SetActive(mode);
	}
	
	void pauseScreenActive(bool mode)
	{
		paused=mode;
		allowTimer=!mode;
		pauseScreen.SetActive(mode);
	}
	
	void lastScreenActive(bool mode)
	{
		allowTimer=!mode;
		gameOverScreenActive(!mode);
		victoryScreenActive(!mode);
		lastScreen.SetActive(mode);
	}
	
	#endregion
	
	#region musicSoundPauseUI
	public void clickPause()
	{
		if(pauseScreen.active)
			pauseScreenActive(false);
		else
			pauseScreenActive(true);
	}
	public void clickMusic()
	{
		if(musicMuted)
		{
			musicSoundObjs[0].sprite = musicSoundSprites[0];
			musicMuted=false;
			musicPlayer.Play();
			PlayerPrefs.SetInt("muteM",0);
		}
		else
		{
			musicSoundObjs[0].sprite = musicSoundSprites[1];
			musicMuted=true;
			musicPlayer.Pause();
			PlayerPrefs.SetInt("muteM",1);
		}	
		PlayerPrefs.Save();
	}
	
	public void clickSound()
	{
		if(soundMuted)
		{
			musicSoundObjs[1].sprite = musicSoundSprites[2];
			soundMuted=false;
			soundPlayer.mute=false;
			PlayerPrefs.SetInt("muteS",0);
		}
		else
		{
			musicSoundObjs[1].sprite = musicSoundSprites[3];
			soundMuted=true;
			soundPlayer.mute=true;
			PlayerPrefs.SetInt("muteS",1);
		}	
		PlayerPrefs.Save();
	}
	
	#endregion
	
}

//screens sounds







