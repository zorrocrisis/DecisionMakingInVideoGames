using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    public void InitiateGOB()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.GOB;
        InitiateNextScene();
    }

    public void InitiateGOAP()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.GOAP;
        InitiateNextScene();
    }

    public void InitiateMCTS()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.MCTS;
        InitiateNextScene();
    }

    private void InitiateNextScene()
    {
        SceneManager.LoadScene("Dungeon");
    }

    public void Exit()
    {
        // Exit the application
        Application.Quit();

        // (This will not work in the Unity editor, but will work in a build)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
