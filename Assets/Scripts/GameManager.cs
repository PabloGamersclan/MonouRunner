
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject obstacleParent;
    [SerializeField] private GameObject AudioMenuObj;
    [SerializeField] private GameObject GameOverText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject initialText;
    [SerializeField] private GameObject initialTextDemo;
    [SerializeField] private GameObject instrucciones;
    private static GameManager instance;


    public bool isGameOver;
    public bool isPause;
    public bool IsPlaying;
    public bool notStarted = true;

    private int score;
    private Animator GameOverAnimator;
    public Sound[] sfxSounds;
    public AudioSource sfxSource;
    [SerializeField] TextMeshProUGUI demoText;

    public static GameManager Intance { get { return instance; } }

    private void Awake()
    {
        
        if(Intance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        PauseGame();
    }

    void Update()
    {
        if((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && isGameOver)
        {
            //RestartGame();
        }
        /*if (Input.GetMouseButton(0) && notStarted){
            Time.timeScale = 1f;
            notSettarted = false;
            initialText.SetActive(false);
            isPause = false;
            ResumeGame();
            demoText.gameObject.SActive(false);
        }
        if (Input.GetMouseButton(1) && notStarted)
        {
            Linker.instance.isDemo = true;
            Time.timeScale = 1f;
            notStarted = false;
            initialText.SetActive(false);
            isPause = false;
            ResumeGame();

            demoText.gameObject.SetActive(true);

        }*/
    }

    public void SelectGameType(bool isDemo)
    {
        if (isDemo)
        {
            Debug.Log("me hablan");
        //  Linker.instance.isDemo = true;
            Time.timeScale = 1f;
            notStarted = false;
            initialText.SetActive(false);
            initialTextDemo.SetActive(false);
            instrucciones.SetActive(false);
            isPause = false;
            IsPlaying = true;
            ResumeGame();
            scoreText.transform.parent.gameObject.SetActive(true);
            demoText.gameObject.SetActive(true);
            Destroy(initialText.gameObject);
        }
        else
        {
            //Linker.instance.isDemo = false;
            Time.timeScale = 1f;
            notStarted = false;
            initialText.SetActive(false);
            initialTextDemo.SetActive(false);
            instrucciones.SetActive(false);
            isPause = false;
            scoreText.transform.parent.gameObject.SetActive(true);
            ResumeGame();
            demoText.gameObject.SetActive(false);
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator WaitforSeconds(float time, int score )
    {
        //Print the time of when the function is first called.
      

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(time);
        try{ Monou.MonouArcadeManager.inst.Success(score); } catch {}
        //After we have waited 5 seconds print the time again.
       
    }
    public void GameOver()
    {
        isGameOver = true;
        GameOverText.SetActive(true);
        GameOverAnimator = GameOverText.GetComponent<Animator>();
        GameOverAnimator.SetTrigger("GameOver");
        PlaySFX("Die");
        Debug.LogWarning("Postin results");
        IsPlaying = false;
        StartCoroutine(WaitforSeconds(2f, score));
       
        /*if (!Linker.instance.isDemo)
        {
            Debug.LogWarning("Postin results 2");

            StartCoroutine(PostResults());
        }*/
    }

    public void IncreaseScore()
    {
        try{ Monou.MonouArcadeManager.inst.Advance(1); } catch {}
        score++;
        scoreText.text = score.ToString();
    }


    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.clipName == name);

        if (s == null)
            Debug.Log("Sound not found");
        else
            sfxSource.PlayOneShot(s.clip);
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void PauseGame()
    {
        isPause = true;
        //AudioMenuObj.SetActive(true);
        obstacleParent.SetActive(false);
        player.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isPause = false;
        AudioMenuObj.SetActive(false);
        player.gameObject.SetActive(true);
        obstacleParent.SetActive(true);
    }


    public IEnumerator PostResults()
    {
        Debug.Log("posting results");

        string api = Linker.GetEnv() + ".monou.gg/api/tournament/match/decision/round-robin/";


        WWWForm form = new WWWForm();
        form.AddField("place", 99);
        form.AddField("team_id", Linker.m_myTeamId);
        form.AddField("round", Linker.round);
        form.AddField("tournament_id", Linker.m_tournamentIdNumber);
        form.AddField("kills", score);
        form.AddField("deaths", 0);
        form.AddField("assistence", 0);

        Debug.Log("rondaaaa " + Linker.round);

        UnityWebRequest www = UnityWebRequest.Post(api, form);

        Debug.Log("POSTING RESULTS API " + api);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("get Success");
            Debug.Log(www.downloadHandler.text);
            //Debug.Log(www.downloadHandler.text);
            //ReadJson(www.downloadHandler.text);
            /*username = _username.text;
            SceneManager.LoadScene("CustomizationScene");*/
        }

    }
}
