using UnityEngine;
using Harborview.GameTools;

public class GM : MonoBehaviour, IGameState
{
    bool isPaused { get; set; }

    bool IGameState.IsPaused => isPaused;

    void IGameState.PauseGame()
    {
        if(isPaused)
        { 
            Time.timeScale = 0;
        }
        else {
            Time.timeScale = 1;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
