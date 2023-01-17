using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SaveSystem saveSystem;
    public UnityEvent<bool> OnResultData; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
        
        
        OnResultData?.Invoke(File.Exists(Application.dataPath + "/SaveData/SaveFile.txt"));
        
    }

    public void ClickStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1); // 다음 씬으로 이동
    }

    public void ClickLoad()
    {
        StartCoroutine(LoadRoutine()) ;
    }

    IEnumerator LoadRoutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Main");
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        while (!operation.isDone)
        {
            yield return null;
        }


        saveSystem = FindObjectOfType<SaveSystem>();
        saveSystem.Load();
    }
}