using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Cache : MonoBehaviour 
{
	[SerializeField]private bool freezeTimer;
	
	private bool timeFrozen;
	
	
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
	
	private const bool multiMatchBonus=true;
	private int lastTypeMatch = 99;
	
	
	private Block block1;
	private Block block2;
	
	[SerializeField]private Image[] musicSoundObjs;
	[SerializeField]private Sprite[] musicSoundSprites;
	[SerializeField]private AudioSource musicPlayer;
	[SerializeField]private AudioSource soundPlayer;
	
	private bool musicMuted;
	private bool soundMuted;
	
	[SerializeField]private GameObject rotator;
	private bool rotating;
	
	[SerializeField]private GameObject[] effect;
	[SerializeField]private GameObject boomEffect;
	
	[SerializeField]private GameObject bonusText;
	
	[SerializeField]private GameObject shuffleScreen;
	[SerializeField]private GameObject shuffleBlock;
	[SerializeField]private GameObject gameOverScreen;
	[SerializeField]private GameObject nextLevelScreen;
	[SerializeField]private Text nextLevelScore;
	[SerializeField]private GameObject victoryScreen;
	[SerializeField]private Text lastLevelScore;
	[SerializeField]private Text lastLevelScoreTime;
	[SerializeField]private GameObject pauseScreen;
	[SerializeField]private GameObject lastScreen;
	[SerializeField]private Text lastScreenScore;
	
	[SerializeField]private GameObject highlightSelected;
	[SerializeField]private GameObject highlightViewed;
	
	[SerializeField]private Image UI_padlock;
	
	[SerializeField]private RawImage UI_viewed;
	[SerializeField]private RawImage UI_selected;
	
	
	[SerializeField]private Text scoreText;
	
	
	private SafeInt score;
	private int allPairs;
	
	[SerializeField]private AudioClip[] sound;
	[SerializeField]private AudioClip soundOk;
	[SerializeField]private AudioClip soundError;
	[SerializeField]private AudioClip soundClip;
	[SerializeField]private AudioClip soundShuffle;
	[SerializeField]private AudioClip soundLevelPassed;
	
	private AudioSource audioSource;
	
	[SerializeField]private Texture2D emptyTexture;
	
	[SerializeField]private Texture2D[] textures;
	
	[SerializeField]private GameObject[] level;
	private byte curLevel;
	[SerializeField]private byte[] levelPairs;
	
	[SerializeField]private Text bonusScoreTimer;
	private float bonusTimer=0;
	private SafeInt bonusMultiplier;
	
	
	private GameObject[] blocks;
	private List<int> clickableTypes = new List<int>();
	private List<Block> blockScript = new List<Block>();
	
	private SafeFloat time;
	[SerializeField]private Text timeText;
	private bool allowTimer;
	private bool allowBonusTimer;
	
	

	public bool paused;
	
	private bool coroutined;
	
	
	[SerializeField]private bool kongBuild;
	[SerializeField]private KongregateAPIBehaviour kongComp;
	
	
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
		if(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.W))
		{
			rotator.transform.Translate(0,Time.deltaTime*5,0);
			checkRotatorBoundaries();
		}
		if(Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S))
		{
			rotator.transform.Translate(0,-Time.deltaTime*5,0);
			checkRotatorBoundaries();
		}
		
		if (Input.GetAxis("Mouse ScrollWheel") < 0f && Camera.main.orthographicSize < 10 ) // forward
		{
			Camera.main.orthographicSize++;
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0f &&  Camera.main.orthographicSize > 4  ) // backwards
		{
			Camera.main.orthographicSize--;
		}
		
		if (Input.GetMouseButton(1))
		{
			float h = Input.GetAxis("Mouse X");
			if(h>3)
				rotMe(true);
			else if(h<-3)
				rotMe(false);
		}
		
		if(Input.GetMouseButton(2))
		{
			float j = Input.GetAxis("Mouse Y");
			rotator.transform.Translate(0,-j*Time.deltaTime*5,0);
			checkRotatorBoundaries();
		}
		
		updateBonusTimerUI();
		
		if(time.GetValue()>0&&allowTimer)
		{
			if(!timeFrozen)
			{
				time=time-Time.deltaTime*1;
				int normalTime = (int)time.GetValue();
				timeText.text = "Time: "+(normalTime/60)+":"+ (normalTime%60<10?"0" : "")+normalTime%60;
			}
		}
		else if(time.GetValue()<=0)
		{
			StartCoroutine(gameOver());
		}
		
	}
	
	private void checkRotatorBoundaries()
	{
		if(rotator.transform.position.y>10)
			rotator.transform.position = new Vector3(0,10,0);
		else if(rotator.transform.position.y<-10)
			rotator.transform.position = new Vector3(0,-10,0);
	}
	
	void newLevel()
	{
		if(freezeTimer)
		{
			timeFrozen=true;
			UI_padlock.gameObject.SetActive(true);
		}
		
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
				Instantiate(effect[1],block1.gameObject.transform.position,transform.rotation);
				Instantiate(effect[1],block2.gameObject.transform.position,transform.rotation);
			}
			else if(bonusint>=11&&bonusint<=18)
			{
				btext.GetComponent<TextPopUp>().assign("Great",Color.blue,25);
				updateScore(10+bonusMultiplier.GetValue()*3, btextScore);
				Instantiate(effect[2],block1.gameObject.transform.position,transform.rotation);
				Instantiate(effect[2],block2.gameObject.transform.position,transform.rotation);
			}
			else if(bonusint>=19&&bonusint<=28)
			{
				btext.GetComponent<TextPopUp>().assign("Splendid",Color.green,30);
				updateScore(10+bonusMultiplier.GetValue()*4, btextScore);
				Instantiate(effect[3],block1.gameObject.transform.position,transform.rotation);
				Instantiate(effect[3],block2.gameObject.transform.position,transform.rotation);
			}
			else if(bonusint+1==allPairs)
			{
				audioSource.PlayOneShot(sound[1],1);
				Instantiate(boomEffect,block2.gameObject.transform.position,transform.rotation);
				
				btext.GetComponent<TextPopUp>().assign("FLAWLESS",Color.black,50);
				updateScore(100+bonusMultiplier.GetValue()*6, btextScore);
			}
			else
			{
				Instantiate(effect[4],block1.gameObject.transform.position,transform.rotation);
				Instantiate(effect[4],block2.gameObject.transform.position,transform.rotation);
				btext.GetComponent<TextPopUp>().assign("Excellent",Color.magenta,35);
				updateScore(10+bonusMultiplier.GetValue()*5, btextScore);
			}
		}
		else
		{
			updateScore(10+bonusMultiplier.GetValue(), btextScore);
			Instantiate(effect[0],block1.gameObject.transform.position,transform.rotation);
			Instantiate(effect[0],block2.gameObject.transform.position,transform.rotation);
		}
		
		
		bonusTimer=3f;
		bonusMultiplier+=new SafeInt(1);

		audioSource.PlayOneShot(sound[0],1);
		
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
		
		if(freezeTimer&&timeFrozen)
		{
			timeFrozen=false;
			UI_padlock.gameObject.SetActive(false);
		}
		
		//any blocks left?
		if(anyBlocks())
		{
			//check possible moves
			checkMoves();
		}
	}
	
	void updateBonusTimerUI()
	{
		if(bonusTimer>0)
		{
			bonusTimer-=Time.deltaTime*1;
			bonusScoreTimer.text = bonusTimer.ToString("F2");
			if(bonusTimer<=0)
			{
				bonusTimer=0;
				bonusMultiplier=new SafeInt(0);
			}
			if(bonusTimer<=0) // disallow -0.02 etc.
			{
				bonusScoreTimer.text="";
			}
		}
		else //safety
		{
			bonusScoreTimer.text="";
		}
	}
	
	void updateScore(int addScore, GameObject bts)
	{
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
		//if(EasyModeStatic.easyMode==2) EASY MODE ALL TIME
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
				if(multiMatchBonus)
				{
					if(lastTypeMatch == block1.type)
					{
						Debug.Log("multimatch");
					}
				}
				lastTypeMatch = block1.type;
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
			for(float t = 0f; t <= 1; t += Time.deltaTime/.3f) 
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







