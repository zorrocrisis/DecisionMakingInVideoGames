using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{

    [Header("ChangingMenus")]
    public GameObject backPanel;
    public GameObject algorithmOptionsPanel;
    public GameObject MCTSOptionsPanel;
    public GameObject playoutsSliderPanel;
    public GameObject worldPanel;
    public GameObject enemyPanel;
    public GameObject abilitiesPanel;
    public GameObject abilitiesExplanation;


    // Auxliary variables
    private uint playoutNumber;
    private Text playoutText;
    private Slider playoutSlider;

    private void Awake()
    {
        backPanel.SetActive(false);
        MCTSOptionsPanel.SetActive(false);
        playoutsSliderPanel.SetActive(false);
        worldPanel.SetActive(false);
        enemyPanel.SetActive(false);
        abilitiesExplanation.SetActive(false);

        playoutText = playoutsSliderPanel.GetComponentInChildren<Text>();
        playoutSlider = playoutsSliderPanel.GetComponentInChildren<Slider>();
        UpdatePlayoutNumber();
    }

    public void InitiateGOB()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.GOB;
        InitiateWorldSettingsMenu();
    }

    public void InitiateGOAP()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.GOAP;
        InitiateWorldSettingsMenu();
    }

    public void InitiateMCTSMenu()
    {
        algorithmOptionsPanel.SetActive(false);
        backPanel.SetActive(true);
        MCTSOptionsPanel.SetActive(true);
        abilitiesPanel.SetActive(false);
    }

    public void BackFromMCTSMenu()
    {
        backPanel.SetActive(false);
        MCTSOptionsPanel.SetActive(false);
        playoutsSliderPanel.SetActive(false);
        worldPanel.SetActive(false);
        algorithmOptionsPanel.SetActive(true);
        enemyPanel.SetActive(false);
        abilitiesPanel.SetActive(true);
        abilitiesExplanation.SetActive(false);
    }

    public void InitiateVanillaMCTS()
    {
        DecisionMakingSceneParameters.numberOfPlayoutsParameter = 1;
        DecisionMakingSceneParameters.biasedPlayoutParameter = false;
        DecisionMakingSceneParameters.limitedPlayoutParameter = false;
        InitiateMCTS();
    }

    public void InitiateMultiplePlayoutsMCTSMenu()
    {
        playoutsSliderPanel.SetActive(true);
        MCTSOptionsPanel.SetActive(false);
    }

    public void InitiateMultiplePlayoutsMCTS()
    {
        DecisionMakingSceneParameters.numberOfPlayoutsParameter = playoutNumber;
        DecisionMakingSceneParameters.biasedPlayoutParameter = false;
        DecisionMakingSceneParameters.limitedPlayoutParameter = false;
        InitiateMCTS();
    }

    public void InitiateBiasedPlayoutsMCTS()
    {
        DecisionMakingSceneParameters.numberOfPlayoutsParameter = 1;
        DecisionMakingSceneParameters.biasedPlayoutParameter = true;
        DecisionMakingSceneParameters.limitedPlayoutParameter = false;
        InitiateMCTS();
    }

    public void InitiateLimitedPlayoutsMCTS()
    {
        DecisionMakingSceneParameters.numberOfPlayoutsParameter = 1;
        DecisionMakingSceneParameters.biasedPlayoutParameter = false;
        DecisionMakingSceneParameters.limitedPlayoutParameter = true;
        InitiateMCTS();
    }

    private void InitiateMCTS()
    {
        DecisionMakingSceneParameters.algorithmToUse = DecisionAlgorithmActive.MCTS;
        InitiateWorldSettingsMenu();
    }

    public void DeterministicWorld()
    {
        worldPanel.SetActive(false);
        DecisionMakingSceneParameters.stochasticWorld = false;
        InitiateEnemySettingsMenu();
    }

    public void StochasticWorld()
    {
        worldPanel.SetActive(false);
        DecisionMakingSceneParameters.stochasticWorld = true;
        InitiateEnemySettingsMenu();
    }

    private void InitiateWorldSettingsMenu()
    {
        MCTSOptionsPanel.SetActive(false);
        playoutsSliderPanel.SetActive(false);
        worldPanel.SetActive(true);
        backPanel.SetActive(true);
        abilitiesPanel.SetActive(false);
        algorithmOptionsPanel.SetActive(false);
    }

    private void InitiateEnemySettingsMenu()
    {
        enemyPanel.SetActive(true);
    }

    public void LineFormation()
    {
        DecisionMakingSceneParameters.formationsToUse = FormationsActive.Line;
        InitiateNextScene();
    }

    public void TriangleFormation()
    {
        DecisionMakingSceneParameters.formationsToUse = FormationsActive.Triangle;
        InitiateNextScene();
    }

    public void NoFormation()
    {
        DecisionMakingSceneParameters.formationsToUse = FormationsActive.None;
        InitiateNextScene();
    }

    public void InitiatiteAbilityMenu()
    {
        backPanel.SetActive(true);
        abilitiesPanel.SetActive(false);
        algorithmOptionsPanel.SetActive(false);
        abilitiesExplanation.SetActive(true);
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

    public void UpdatePlayoutNumber()
    {
        playoutNumber = (uint)playoutSlider.value;
        playoutText.text = "MCTS with " + playoutNumber + " Playouts" ;
    }



    // FORMATION ???
    // STOCHASTIC WORLD???
}
