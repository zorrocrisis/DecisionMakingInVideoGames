using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.Formations;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;
using Assets.Scripts.Game;

public class AutonomousCharacter : NPC
{
    //constants
    public const string SURVIVE_GOAL = "Survive";
    public const string GAIN_LEVEL_GOAL = "GainXP";
    public const string BE_QUICK_GOAL = "BeQuick";
    public const string GET_RICH_GOAL = "GetRich";

    public const float DECISION_MAKING_INTERVAL = 20.0f;
    public const float RESTING_INTERVAL = 5.0f;
    public const int REST_HP_RECOVERY = 2;
    public const float SPED_UP_INTERVAL = 10f;

    //UI Variables
    private Text SurviveGoalText;
    private Text GainXPGoalText;
    private Text BeQuickGoalText;
    private Text GetRichGoalText;
    private Text DiscontentmentText;
    private Text TotalProcessingTimeText;
    private Text BestDiscontentmentText;
    private Text ProcessedActionsText;
    private Text BestActionText;
    private Text BestActionSequence;
    private Text DiaryText;

    [Header("Character Settings")]
    public bool controlledByPlayer;
    public float Speed = 5.0f;

    [Header("Decision Algorithm Options")]
    public DecisionAlgorithmActive activeAlgorithm;

    [Header("MCTS Options "), Tooltip("Only alter one variable at a time")]
    public uint NumberOfPlayouts = 1;
    public bool BiasedPlayout;
    public bool LimitedPlayout;


    [Header("Character Info")]
    public bool Resting = false;
    public float RestInterval = RESTING_INTERVAL;
    public bool SpedUp = false;
    public float SpedUpInterval = SPED_UP_INTERVAL;

    public Goal BeQuickGoal { get; private set; }
    public Goal SurviveGoal { get; private set; }
    public Goal GetRichGoal { get; private set; }
    public Goal GainLevelGoal { get; private set; }
    public List<Goal> Goals { get; set; }
    public List<Action> Actions { get; set; }
    public Action CurrentAction { get; private set; }

    public GOBDecisionMaking GOBDecisionMaking { get; set; }
    public DepthLimitedGOAPDecisionMaking GOAPDecisionMaking { get; set; }
    public MCTS MCTSDecisionMaking { get; set; }

    //private fields for internal use only
    private float nextUpdateTime = 0.0f;
    private float previousGold = 0.0f;
    private int previousLevel = 1;
    public TextMesh playerText;
    private GameObject closestObject;
    private int totalActionCombinationsProcessed = 0;
    private float totalProcessingTime = 0.0f;

    private float RestTime = 1000f;
    private float SpedTime = 1000f;

    // Draw path settings
    private LineRenderer lineRenderer;


    public void Start()
    {
        //This is the actual speed of the agent
        lineRenderer = this.GetComponent<LineRenderer>();
        playerText.text = "";

        // Initializing UI Text
        this.BeQuickGoalText = GameObject.Find("BeQuickGoal").GetComponent<Text>();
        this.SurviveGoalText = GameObject.Find("SurviveGoal").GetComponent<Text>();
        this.GainXPGoalText = GameObject.Find("GainXP").GetComponent<Text>();
        this.GetRichGoalText = GameObject.Find("GetRichGoal").GetComponent<Text>();
        this.DiscontentmentText = GameObject.Find("Discontentment").GetComponent<Text>();
        this.TotalProcessingTimeText = GameObject.Find("ProcessTime").GetComponent<Text>();
        this.BestDiscontentmentText = GameObject.Find("BestDicont").GetComponent<Text>();
        this.ProcessedActionsText = GameObject.Find("ProcComb").GetComponent<Text>();
        this.BestActionText = GameObject.Find("BestAction").GetComponent<Text>();
        this.BestActionSequence = GameObject.Find("BestActionSequence").GetComponent<Text>();
        DiaryText = GameObject.Find("DiaryText").GetComponent<Text>();


        //initialization of the GOB decision making
        //let's start by creating 4 main goals

        this.SurviveGoal = new Goal(SURVIVE_GOAL, 1.0f);

        this.GainLevelGoal = new Goal(GAIN_LEVEL_GOAL, 1.0f)
        {
            ChangeRate = 0.1f
        };

        this.GetRichGoal = new Goal(GET_RICH_GOAL, 5.0f)
        {
            InsistenceValue = 5.0f,
            ChangeRate = 0.5f
        };

        this.BeQuickGoal = new Goal(BE_QUICK_GOAL, 1.0f)
        {
            ChangeRate = 0.1f
        };

        this.Goals = new List<Goal>();
        this.Goals.Add(this.SurviveGoal);
        this.Goals.Add(this.BeQuickGoal);
        this.Goals.Add(this.GetRichGoal);
        this.Goals.Add(this.GainLevelGoal);

        //initialize the available actions
        //Uncomment commented actions after you implement them

        this.Actions = new List<Action>();

        this.Actions.Add(new LevelUp(this));


        foreach (var chest in GameObject.FindGameObjectsWithTag("Chest"))
        {
            this.Actions.Add(new PickUpChest(this, chest));
        }

        foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
        {
            this.Actions.Add(new GetManaPotion(this, potion));
        }

        foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
        {
            this.Actions.Add(new GetHealthPotion(this, potion));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Skeleton"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Orc"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
        }

        //Only valid for skeletons (undead)
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Skeleton"))
        {
            this.Actions.Add(new DivineSmite(this, enemy));
        }

        //ShieldOfFaith doesnt require a target
        this.Actions.Add(new ShieldOfFaith(this));

        //Pray doesnt require a target
        this.Actions.Add(new Pray(this));

        //SpeedUp doesn't require a target
        this.Actions.Add(new SpeedUp(this));

        // Initialization of decision making Algorithms
        DefineDecisionMakingAlgorithm();
        var worldModel = new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals);
        this.GOBDecisionMaking = new GOBDecisionMaking(this.Actions, this.Goals);
        this.GOAPDecisionMaking = new DepthLimitedGOAPDecisionMaking(worldModel, this.Actions, this.Goals);

        if (activeAlgorithm == DecisionAlgorithmActive.MCTS)
        {
            //Do we want multiple playouts? If so, use the child class MCTSMultiplePlayouts
            if (NumberOfPlayouts > 1)
            {
                this.MCTSDecisionMaking = new MCTSMultiplePlayouts(worldModel, NumberOfPlayouts);
            }
            //Do we want a biased playout? If so, use the child class MCTSBiasedPlayout
            else if (BiasedPlayout)
            {
                this.MCTSDecisionMaking = new MCTSBiasedPlayout(worldModel);
            }
            else if (LimitedPlayout)
            {
                this.MCTSDecisionMaking = new MCTSLimitedPlayout(worldModel);
            }
            else
            {
                this.MCTSDecisionMaking = new MCTS(worldModel);
            }
        }

        this.Resting = false;

        DiaryText.text += "My Diary \n I awoke. What a wonderful day to kill Monsters! \n";
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.gameEnded) return;

        if (Time.time > this.nextUpdateTime || GameManager.Instance.WorldChanged)
        {
            GameManager.Instance.WorldChanged = false;
            this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;

            //first step, perceptions
            //update the agent's goals based on the state of the world

            // Max Health minus current Health
            this.SurviveGoal.InsistenceValue = baseStats.MaxHP - baseStats.HP;
            // Normalize it to 0-10
            this.SurviveGoal.InsistenceValue = NormalizeGoalValues(this.SurviveGoal.InsistenceValue, 0, baseStats.Level * 10);

            this.BeQuickGoal.InsistenceValue = baseStats.Time;
            this.BeQuickGoal.InsistenceValue = NormalizeGoalValues(this.BeQuickGoal.InsistenceValue, 0, (float)GameManager.GameConstants.TIME_LIMIT);

            this.GainLevelGoal.InsistenceValue += this.GainLevelGoal.ChangeRate; //increase in goal over time
            if (baseStats.Level > this.previousLevel)
            {
                this.GainLevelGoal.InsistenceValue -= baseStats.Level - this.previousLevel;
                this.previousLevel = baseStats.Level;
            }
            this.GainLevelGoal.InsistenceValue = NormalizeGoalValues(this.GainLevelGoal.InsistenceValue, 0, baseStats.Level * 10);

            this.GetRichGoal.InsistenceValue += this.GetRichGoal.ChangeRate;
            // Is this the best way to increment it?
            this.GetRichGoal.InsistenceValue = NormalizeGoalValues(this.GetRichGoal.InsistenceValue, 0, 25);

            this.SurviveGoalText.text = "Survive: " + this.SurviveGoal.InsistenceValue;
            this.GainXPGoalText.text = "Gain Level: " + this.GainLevelGoal.InsistenceValue.ToString("F1");
            this.BeQuickGoalText.text = "Be Quick: " + this.BeQuickGoal.InsistenceValue.ToString("F1");
            this.GetRichGoalText.text = "GetRich: " + this.GetRichGoal.InsistenceValue.ToString("F1");
            this.DiscontentmentText.text = "Discontentment: " + this.CalculateDiscontentment().ToString("F1");

            //To have a new decision lets initialize Decision Making Proccess
            this.CurrentAction = null;
            if (activeAlgorithm == DecisionAlgorithmActive.GOAP)
                this.GOAPDecisionMaking.InitializeDecisionMakingProcess();
            else if (activeAlgorithm == DecisionAlgorithmActive.GOB)
                this.GOBDecisionMaking.InProgress = true;
            else if (activeAlgorithm == DecisionAlgorithmActive.MCTS)
                this.MCTSDecisionMaking.InitializeMCTSearch();
        }

        if (!Resting)
        {
            //Keep stored the last moment the player wasn't resting
            this.RestTime = Time.time;
        }

        if (this.controlledByPlayer)
        {
            //Using the old Input System
            if (Input.GetKey(KeyCode.W))
                this.transform.position += new Vector3(0.0f, 0.0f, 0.1f) * this.Speed;
            if (Input.GetKey(KeyCode.S))
                this.transform.position += new Vector3(0.0f, 0.0f, -0.1f) * this.Speed;
            if (Input.GetKey(KeyCode.A))
                this.transform.position += new Vector3(-0.1f, 0.0f, 0.0f) * this.Speed;
            if (Input.GetKey(KeyCode.D))
                this.transform.position += new Vector3(0.1f, 0.0f, 0.0f) * this.Speed;
            if (Input.GetKey(KeyCode.F))
                if (closestObject != null)
                {
                    //Simple way of checking which object is closest to Sir Uthgard
                    var s = playerText.text.ToString();
                    if (s.Contains("Potion"))
                        PickUpPotion(s);
                    if (s.Contains("Potion"))
                        PickUpPotion(s);
                    else if (s.Contains("Chest"))
                        PickUpChest();
                    else if (s.Contains("Enemy"))
                        AttackEnemy();
                }
            if (Input.GetKey(KeyCode.L))
                GameManager.Instance.LevelUp();
        }
        else if (Resting)
        {
            this.GetComponent<NPC>().navMeshAgent.isStopped = true;

            //Wait the resting interval (don't do any decision making)
            if (Time.time >= this.RestTime + RestInterval)
            {
                //After that, the resting is over
                Resting = false;

                //Gain HP (but be careful not to go over max limit)
                int maxHP = GameManager.Instance.Character.baseStats.MaxHP;
                if (GameManager.Instance.Character.baseStats.HP + REST_HP_RECOVERY > maxHP)
                    GameManager.Instance.Character.baseStats.HP = maxHP;
                else
                    GameManager.Instance.Character.baseStats.HP += REST_HP_RECOVERY;

                this.GetComponent<NPC>().navMeshAgent.isStopped = false;
            }
        }
        else if (activeAlgorithm == DecisionAlgorithmActive.GOAP)
        {
            this.UpdateDLGOAP();
        }
        else if (activeAlgorithm == DecisionAlgorithmActive.GOB)
        {
            this.UpdateGOB();
        }
        else if (activeAlgorithm == DecisionAlgorithmActive.MCTS)
        {
            this.UpdateMCTS();
        }

        if (this.CurrentAction != null)
        {
            if (this.CurrentAction.CanExecute())
            {
                this.CurrentAction.Execute();
            }
        }

        if (navMeshAgent.hasPath)
        {
            DrawPath();
        }

        //SpeedUp action's wait time
        if(!SpedUp)
        {
            //Keep stored the last moment the player wasn't sped up
            this.SpedTime = Time.time;
        }
        if (SpedUp)
        {
            //Wait the defined interval (don't do any decision making)
            if (Time.time >= this.SpedTime + SpedUpInterval)
            {
                //After that, the speed boost is over
                SpedUp = false;
                this.GetComponent<NPC>().navMeshAgent.speed /= 2;
            }
        }
    }

    private void DefineDecisionMakingAlgorithm()
    {
        #if UNITY_EDITOR
            Debug.Log("Algorithm selected from the inspector...");
        #else
            Debug.Log("Algorithm selected from main menu...");
            activeAlgorithm = DecisionMakingSceneParameters.algorithmToUse;

            if(activeAlgorithm == DecisionAlgorithmActive.MCTS)
            {
                NumberOfPlayouts = DecisionMakingSceneParameters.numberOfPlayoutsParameter;
                BiasedPlayout = DecisionMakingSceneParameters.biasedPlayoutParameter;
                LimitedPlayout = DecisionMakingSceneParameters.limitedPlayoutParameter;
            }
        #endif
    }

    public void AddToDiary(string s)
    {
        DiaryText.text += Time.time + s + "\n";

        if (DiaryText.text.Length > 600)
            DiaryText.text = DiaryText.text.Substring(500);
    }

   
    private void UpdateGOB()
    {
        bool newDecision = false;
        if (this.GOBDecisionMaking.InProgress)
        {
            //choose an action using the GOB Decision Making process
            var action = this.GOBDecisionMaking.ChooseAction();
            if (action != null && action != this.CurrentAction)
            {
                this.CurrentAction = action;
                newDecision = true;
                if (newDecision)
                {
                    AddToDiary(" I decided to " + action.Name);
                    this.BestActionText.text = "Best Action: " + action.Name + "\n";
                    this.BestActionSequence.text = " Second Best:" + this.GOBDecisionMaking.secondBestAction.Name + "\n";
                }

            }

        }

    }

    private void UpdateDLGOAP()
    {
        bool newDecision = false;
        if (this.GOAPDecisionMaking.InProgress)
        {
            //choose an action using the GOB Decision Making process
            var action = this.GOAPDecisionMaking.ChooseAction();

            //Auxiliary variables since FixedUpdate (re)initializes the GOAP...
            totalActionCombinationsProcessed += this.GOAPDecisionMaking.TotalActionCombinationsProcessed;
            totalProcessingTime += this.GOAPDecisionMaking.TotalProcessingTime;

            if (action != null && action != this.CurrentAction)
            { 
                this.CurrentAction = action;
                newDecision = true;
            }
        }

        this.TotalProcessingTimeText.text = "Process. Time: " + totalProcessingTime.ToString("F");
        this.BestDiscontentmentText.text = "Best Discontentment: " + this.GOAPDecisionMaking.BestDiscontentmentValue.ToString("F");
        this.ProcessedActionsText.text = "Act. comb. processed: " + totalActionCombinationsProcessed.ToString("F0");

        if (this.GOAPDecisionMaking.BestAction != null)
        {
            if (newDecision)
            {
                AddToDiary(" I decided to " + GOAPDecisionMaking.BestAction.Name);
            }
            var actionText = "";
            foreach (var action in this.GOAPDecisionMaking.BestActionSequence)
            {
                actionText += "\n" + action.Name;
            }
            this.BestActionSequence.text = "Best Action Sequence: " + actionText;
            this.BestActionText.text = "Best Action: " + GOAPDecisionMaking.BestAction.Name;
        }
        else
        {
            this.BestActionSequence.text = "Best Action Sequence:\nNone";
            this.BestActionText.text = "Best Action: \n None";
        }
    }


    void UpdateMCTS()
    {
        bool newDecision = false;
        if (this.MCTSDecisionMaking.InProgress)
        {
            //choose an action using the MCTS run method
            var action = this.MCTSDecisionMaking.Run();

            //Auxiliary variables since FixedUpdate (re)initializes the MCTS...
            this.totalProcessingTime += this.MCTSDecisionMaking.TotalProcessingTime;

            if (action != null && action != this.CurrentAction)
            {
                this.CurrentAction = action;
                newDecision = true;
            }
        }

        //Implement text to show other important variables?
        this.TotalProcessingTimeText.text = "Process. Time: " + totalProcessingTime.ToString("F");

        if (this.MCTSDecisionMaking.BestFirstChild != null)
        {
            if (newDecision)
            {
                AddToDiary(" I decided to " + MCTSDecisionMaking.BestFirstChild.Action.Name);
            }
            var actionText = "";
            foreach (var action in this.MCTSDecisionMaking.BestActionSequence)
            {
                actionText += "\n" + action.Name;
            }
            this.BestActionSequence.text = "Best Action Sequence: " + actionText;
            this.BestActionText.text = "Best Action: " + MCTSDecisionMaking.BestFirstChild.Action.Name;
        }
        else
        {
            this.BestActionSequence.text = "Best Action Sequence:\nNone";
            this.BestActionText.text = "Best Action: \n None";
        }
    }


    void DrawPath()
    {
       
        lineRenderer.positionCount = navMeshAgent.path.corners.Length;
        lineRenderer.SetPosition(0, this.transform.position);

        if (navMeshAgent.path.corners.Length < 2)
        {
            return;
        }

        for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(navMeshAgent.path.corners[i].x, navMeshAgent.path.corners[i].y, navMeshAgent.path.corners[i].z);
            lineRenderer.SetPosition(i, pointPosition);
        }

    }


    public float CalculateDiscontentment()
    {
        var discontentment = 0.0f;

        foreach (var goal in this.Goals)
        {
            discontentment += goal.GetDiscontentment();
        }
        return discontentment;
    }

    //Functions designed for when the Player has control of the character
    void OnTriggerEnter(Collider col)
    {
        if (this.controlledByPlayer)
        {
            if (col.gameObject.tag.ToString().Contains("Potion"))
            {
                playerText.text = "Pickup Potion";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Chest"))
            {
                playerText.text = "Pickup Chest";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Orc") || col.gameObject.tag.ToString().Contains("Skeleton"))
            {
                playerText.text = "Attack Enemy";
                closestObject = col.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag.ToString() != "")
            playerText.text = "";
    }


    //Functions designed for when the Player has control of the character
    void PickUpPotion(string type)
    {
        if (closestObject != null)
            if (GameManager.Instance.InPotionRange(closestObject))
            {
                if (type.Contains("Mana"))
                    //Debug.Log("Trying to Pickup Mana but Method is not implemented");
                    GameManager.Instance.GetManaPotion(closestObject);
                else
                    GameManager.Instance.GetHealthPotion(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void PickUpChest()
    {
        if (closestObject != null)
            if (GameManager.Instance.InChestRange(closestObject))
            {
                GameManager.Instance.PickUpChest(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void AttackEnemy()
    {
        if (closestObject != null)
            if (GameManager.Instance.InMeleeRange(closestObject))
            {
                GameManager.Instance.SwordAttack(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }


    // Normalize different goal values to 0-10 ranges according to their max
    float NormalizeGoalValues(float value, float min, float max)
    {
        if (value < 0) value = 0.0f;
        // Normalizing to 0-1
        var x = (value - min) / (max - min);

        // Multiplying it by 10
        x *= 10;

        return x;
    }

}
