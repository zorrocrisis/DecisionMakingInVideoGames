using Assets.Scripts.IAJ.Unity.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Formations;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static class GameConstants
    {
        public const float UPDATE_INTERVAL = 2.0f;
        public const int TIME_LIMIT = 200;
        public const int PICKUP_RANGE = 16;
        public const int SPELL_RANGE = 40;

    }
    //public fields, seen by Unity in Editor

    public AutonomousCharacter Character;

    [Header("UI Objects")]
    public Text HPText;
    public Text ShieldHPText;
    public Text ManaText;
    public Text TimeText;
    public Text XPText;
    public Text LevelText;
    public Text MoneyText;
    public Text DiaryText;
    public GameObject GameEnd;

    [Header("Enemy Settings")]
    public bool StochasticWorld;
    public bool BehaviourTreeNPCs;
    public bool usingFormations;
    public bool lineFormation = true;
    public bool triangleFormation;


    //fields
    public List<GameObject> chests { get; set; }
    public List<GameObject> skeletons { get; set; }
    public List<GameObject> orcs { get; set; }
    public List<GameObject> dragons { get; set; }
    public List<GameObject> enemies { get; set; }
    public Dictionary<string, List<GameObject>> disposableObjects { get; set; }
    public bool WorldChanged { get; set; }

    private float nextUpdateTime = 0.0f;
    private float enemyAttackCooldown = 0.0f;
    public bool gameEnded { get; set; } = false;
    public Vector3 initialPosition { get; set; }

    //Formations
    public FormationManager formationManager { get; set; }
    public FormationPatrol formationPatrol;
    public FormationPattern formationType;
    private List<Monster> formationOrcs;
    private bool formationJustBroken = false;

    // User interactions
    public bool SleepingNPCs { get; set; }


    void Awake()
    {
        Instance = this;
        UpdateDisposableObjects();
        this.WorldChanged = false;
        this.Character = GameObject.FindGameObjectWithTag("Player").GetComponent<AutonomousCharacter>();

        this.initialPosition = this.Character.gameObject.transform.position;

        if(usingFormations)
        {
            //Get non leaders + leader orcs to join formation (this is a bit messy...
            //Alternatively, we would find the 3 closest orcs to the dragon)
            GameObject orcLeader = GameObject.Find("Orc2");
            GameObject orc4 = GameObject.Find("Orc0");
            GameObject orc5 = GameObject.Find("Orc1");

            //Get their monster components (needed for formation manager)
            this.formationOrcs = new List<Monster>();
            this.formationOrcs.Add(orcLeader.GetComponent<Monster>());
            this.formationOrcs.Add(orc4.GetComponent<Monster>());
            this.formationOrcs.Add(orc5.GetComponent<Monster>());

            //Get anchor point
            var anchorPoint = GameObject.Find("Anchor1");

            //Define the formation wanted (line formation is default)
            if (triangleFormation)
            {
                this.formationType = new TriangleFormation();
            }
            else if (lineFormation)
            {
               this.formationType = new LineFormation();
            }

            //Create a formation manager and formation patrol instance
            Vector3 orcLeaderRotation = new Vector3(orcLeader.transform.rotation.x, orcLeader.transform.rotation.y, orcLeader.transform.rotation.z);
            this.formationManager = new FormationManager(this.formationOrcs, formationType, anchorPoint.transform.position, orcLeaderRotation);
            this.formationPatrol = new FormationPatrol(this.formationManager, anchorPoint);
        }
        
    }

    public void UpdateDisposableObjects()
    {
        this.enemies = new List<GameObject>();
        this.disposableObjects = new Dictionary<string, List<GameObject>>();
        this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
        this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
        this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
        this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
        this.enemies.AddRange(this.skeletons);
        this.enemies.AddRange(this.orcs);
        this.enemies.AddRange(this.dragons);

     
        //adds all enemies to the disposable objects collection
        foreach (var enemy in this.enemies)
        {
            if (disposableObjects.ContainsKey(enemy.name))
            {
                this.disposableObjects[enemy.name].Add(enemy);
            }
            else this.disposableObjects.Add(enemy.name, new List<GameObject>() { enemy });
        }
        //add all chests to the disposable objects collection
        foreach (var chest in this.chests)
        {
            if (disposableObjects.ContainsKey(chest.name))
            {
                this.disposableObjects[chest.name].Add(chest);
            }
            else this.disposableObjects.Add(chest.name, new List<GameObject>() { chest });
        }
        //adds all health potions to the disposable objects collection
        foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
        {
            if (disposableObjects.ContainsKey(potion.name))
            {
                this.disposableObjects[potion.name].Add(potion);
            }
            else this.disposableObjects.Add(potion.name, new List<GameObject>() { potion });
        }
        //adds all mana potions to the disposable objects collection
        foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
        {
            if (disposableObjects.ContainsKey(potion.name))
            {
                this.disposableObjects[potion.name].Add(potion);
            }
            else this.disposableObjects.Add(potion.name, new List<GameObject>() { potion });
        }
    }

    void Update()
    {
        UserInputHandler();
    }

    void FixedUpdate()
    {
        if (!this.gameEnded)
        {

            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + GameConstants.UPDATE_INTERVAL;
                this.Character.baseStats.Time += GameConstants.UPDATE_INTERVAL;
            }

            this.HPText.text = "HP: " + this.Character.baseStats.HP;
            this.XPText.text = "XP: " + this.Character.baseStats.XP;
            this.ShieldHPText.text = "Shield HP: " + this.Character.baseStats.ShieldHP;
            this.LevelText.text = "Level: " + this.Character.baseStats.Level;
            this.TimeText.text = "Time: " + this.Character.baseStats.Time;
            this.ManaText.text = "Mana: " + this.Character.baseStats.Mana;
            this.MoneyText.text = "Money: " + this.Character.baseStats.Money;

            if (this.Character.baseStats.HP <= 0 || this.Character.baseStats.Time >= GameConstants.TIME_LIMIT)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "You Died";
            }
            else if (this.Character.baseStats.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory \n GG EZ";
            }

            if(!SleepingNPCs && usingFormations)
            {
                //If we're using formations and if it hasn't been destroyed yet
                if (usingFormations && !this.formationPatrol.disableFormation)
                {
                    this.formationPatrol.UpdatePatrol();
                }

                //The player has been detected while in a formation
                if (this.formationPatrol.disableFormation && !formationJustBroken && BehaviourTreeNPCs)
                {
                    //Break formation
                    this.usingFormations = false;
                    formationJustBroken = true;

                    //Proceed with normal behaviour
                    foreach (Monster orc in this.formationOrcs)
                        orc.GetComponent<Orc>().FormationBroken();
                }

            }            

        }
    }

    public void SwordAttack(GameObject enemy)
    {
        int damage = 0;

        Monster.EnemyStats enemyData = enemy.GetComponent<Monster>().enemyStats;

        if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
        {
            this.Character.AddToDiary(" I Sword Attacked " + enemy.name);

            if (this.StochasticWorld)
            {
                damage = enemy.GetComponent<Monster>().DmgRoll.Invoke();

                //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                int attackRoll = RandomHelper.RollD20() + 7;

                if (attackRoll >= enemyData.AC)
                {
                    //there was an hit, enemy is destroyed, gain xp
                    this.enemies.Remove(enemy);
                    this.disposableObjects[enemy.name].Remove(enemy);
                    enemy.SetActive(false);
                    Object.Destroy(enemy);
                }
            }
            else
            {
                damage = enemyData.SimpleDamage;
                this.enemies.Remove(enemy);
                this.disposableObjects[enemy.name].Remove(enemy);
                enemy.SetActive(false);
                Object.Destroy(enemy);
            }

            this.Character.baseStats.XP += enemyData.XPvalue;

            int remainingDamage = damage - this.Character.baseStats.ShieldHP;
            this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

            if (remainingDamage > 0)
            {
                this.Character.baseStats.HP -= remainingDamage;
            }

            this.WorldChanged = true;
        }
    }

    public void EnemyAttack(GameObject enemy)
    {
        if (Time.time > this.enemyAttackCooldown)
        {

            int damage = 0;

            Monster monster = enemy.GetComponent<Monster>();

            if (enemy != null && enemy.activeSelf) //BUG FIXED
            {

                this.Character.AddToDiary(" I was Attacked by " + enemy.name);
                this.enemyAttackCooldown = Time.time + GameConstants.UPDATE_INTERVAL;

                if (this.StochasticWorld)
                {
                    damage = monster.DmgRoll.Invoke();

                    //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                    int attackRoll = RandomHelper.RollD20() + 7;

                    if (attackRoll >= monster.enemyStats.AC)
                    {
                        //there was an hit, enemy is destroyed, gain xp
                        this.enemies.Remove(enemy);
                        this.disposableObjects.Remove(enemy.name);
                        enemy.SetActive(false);
                        Object.Destroy(enemy);
                    }
                }
                else
                {
                    damage = monster.enemyStats.SimpleDamage;
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);
                    enemy.SetActive(false);
                    Object.Destroy(enemy);
                }

                this.Character.baseStats.XP += monster.enemyStats.XPvalue;

                int remainingDamage = damage - this.Character.baseStats.ShieldHP;
                this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

                if (remainingDamage > 0)
                {
                    this.Character.baseStats.HP -= remainingDamage;
                    this.Character.AddToDiary(" I was wounded with " + remainingDamage + " damage");
                }

                this.WorldChanged = true;
            }
        }
    }

    public void DivineSmite(GameObject enemy)
    {
        //Mana cost for this specific spell
        int manaCost = 2;

        //The monster is sure to be a skeleton (check Autonomous Character)
        Monster.EnemyStats enemyData = enemy.GetComponent<Monster>().enemyStats;

        //We need to make sure we have enough mana...
        if (enemy != null && enemy.activeSelf && InSpellRange(enemy) && manaCost <= this.Character.baseStats.Mana)
        {
            this.Character.AddToDiary(" I used Divine Smite on " + enemy.name);

            //Character doesn't suffer any damage by doing this attack

            //We don't need to calculate the damage to the monster, since it immediately destroys the skeletons

            //There was an hit, enemy is destroyed, gain xp and lose mana
            this.enemies.Remove(enemy);
            this.disposableObjects[enemy.name].Remove(enemy);
            enemy.SetActive(false);
            Object.Destroy(enemy);

            this.Character.baseStats.XP += enemyData.XPvalue;
            this.Character.baseStats.Mana -= manaCost;

            this.WorldChanged = true;
        }
    }

    public void ShieldOfFaith()
    {
        int manaCost = 5;
        int shieldHPGain = 5;

        //If the character has enough mana
        if(manaCost <= this.Character.baseStats.Mana)
        {
            this.Character.AddToDiary(" I used Shield Of Faith");
            this.Character.baseStats.Mana -= manaCost;

            //Doesnt matter if the shield of faith is recast while
            //under effect of another shield of faith, since we always
            //update ShieldHP to shieldHPGain
            this.Character.baseStats.ShieldHP = shieldHPGain;

            this.WorldChanged = true;
        }
    }

    public void Pray()
    {
        this.Character.Resting = true;
        this.Character.AddToDiary(" I Prayed");
        //The characters hp is only increment in AutonomousCharacter.cs
        this.WorldChanged = true;
    }

    public void SpeedUp()
    {
        int manaCost = 5;

        //If the character has enough mana
        if (manaCost <= this.Character.baseStats.Mana && !this.Character.SpedUp)
        {
            this.Character.SpedUp = true;
            this.Character.AddToDiary(" I used Speed Up");
            this.Character.baseStats.Mana -= manaCost;
            //Only works by changing MeshAgent speed
            this.Character.GetComponent<NPC>().navMeshAgent.speed *= 2;
            this.WorldChanged = true;
        }
    }

    public void PickUpChest(GameObject chest)
    {

        if (chest != null && chest.activeSelf && InChestRange(chest))
        {
            this.Character.AddToDiary(" I opened  " + chest.name);
            this.chests.Remove(chest);
            this.disposableObjects[chest.name].Remove(chest);
            Object.Destroy(chest);
            this.Character.baseStats.Money += 5;
            this.WorldChanged = true;
        }
    }


    public void GetHealthPotion(GameObject potion)
    {
        if (potion != null && potion.activeSelf && InPotionRange(potion))
        {
            this.Character.AddToDiary(" I drank " + potion.name);
            this.disposableObjects[potion.name].Remove(potion);
            Object.Destroy(potion);
            this.Character.baseStats.HP = this.Character.baseStats.MaxHP;
            this.WorldChanged = true;
        }
    }

    public void GetManaPotion (GameObject potion)
    {
        int manaGain = 10;

        if (potion != null && potion.activeSelf && InPotionRange(potion))
        {
            this.Character.AddToDiary(" I drank " + potion.name);
            this.disposableObjects[potion.name].Remove(potion);
            Object.Destroy(potion);
            //Each mana potion gives 10 mana points
            this.Character.baseStats.Mana += manaGain;
            this.WorldChanged = true;
        }
    }


    public void LevelUp()
    {
        if (this.Character.baseStats.Level >= 4) return;

        if (this.Character.baseStats.XP >= this.Character.baseStats.Level * 10)
        {
            this.Character.baseStats.Level++;
            this.Character.baseStats.MaxHP += 10;
            this.Character.baseStats.XP = 0;
            this.WorldChanged = true;
            this.Character.AddToDiary(" I leveled up to level " + this.Character.baseStats.Level);
        }
    }


    private bool CheckRange(GameObject obj, float maximumSqrDistance)
    {
        var distance = (obj.transform.position - this.Character.gameObject.transform.position).sqrMagnitude;
        return distance <= maximumSqrDistance;
    }


    public bool InMeleeRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE);
    }

    public bool InChestRange(GameObject chest)
    {

        return this.CheckRange(chest, GameConstants.PICKUP_RANGE);
    }

    public bool InPotionRange(GameObject potion)
    {
        return this.CheckRange(potion, GameConstants.PICKUP_RANGE);
    }

    public bool InSpellRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.SPELL_RANGE);
    }

    private void UserInputHandler()
    {
        // Control sleep of Orcs
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ControlOrcsSleep();
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitToMainMenu();
        }
    }

    private void ControlOrcsSleep()
    {
        if(!SleepingNPCs)
        {
            foreach (GameObject orc in this.orcs)
            {
                orc.GetComponent<Orc>().Sleep();
                SleepingNPCs = true;
            }
        }
        else
        {
            foreach (GameObject orc in this.orcs)
            {
                orc.GetComponent<Orc>().AwakeFromSleeping();
                SleepingNPCs = false;
            }
        }
    }

    private void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
