# Decision Making In VideoGames
This project, originally an evaluation component for the Artificial Intelligence in Games course (2023/2024), talking place in Instituto Superior Técnico, University of Lisbon, aimed to explore **decision making systems in video games**.

![Uthgard](https://github.com/user-attachments/assets/189abaeb-688d-48de-a60d-a3c427446d32)

The following document indicates how to access the source code, utilise the executable application and control the program. It also contains an analysis between the decision making algorithms. 

## **Source Files and Application**
The project's source files can be downloaded from this repository. To open the program using Unity (v.2021.3.10f1), simply clone the repository and open the project utilising Unity Hub.

To test the application, only the files contained in the "Build" folder are necessary. Once those files are downloaded, simply start the executable (see the controls below).

## **Application's Controls**

Main Menu:
- **LMB** interacts with the main menu's buttons, selecting the decision making algorithm and exiting the application.

In Simulation:
- **Esc** exits to the main menu.
- **Space** enables/disables NPCs' sleeping state (by default, NPCs are active).


## **Summary**
With the intent of exploring decision making in video games, multiple systems were studied and developed in C# (and Unity v.2021.3.10f1). The goal of this report is to detail the thought process and justify the decisions made in terms of implementation, but also to analyse and discuss the corresponding results.

Firstly, **Behaviour Trees** were implemented to create more lifelike behaviour for Orcs, who now patrol between two positions and chase after the main character (Sir Uthgard) if they spot him. Additionally, a **Shout action was created to further create the illusion of communication between Orcs**.

However, this type of behaviour followed strict and basic rules, rooting itself in a so-called **author design approach to decision making**. More complex and dynamic structures of decision making were sought-after, with a special focus on a **search approach**.

This way, the **GOB (Goal-Oriented Behaviour)** and **GOAP (Goal-Oriented Action Planning)** decision making algorithms were implemented, alongside a few other new abilities, giving the main character the competence to dynamically select their next course of action(s). For this, **four main goals were defined and then considered through insistence and an overall discontentment**: Survive, Gain Level, Be Quick and Get Rich.

The **MCTS (Monte Carlo Tree Search)** decision making algorithm allowed for a new perspective, relying on an **equilibrium between exploration and exploitation of game states (including future ones)**. Moreover, some **optimizations** for MCTS were developed, including **MCTS with Multiple Playouts**, **MCTS with Biased Playouts** and **MCTS with Limited Playouts**, all of which take into consideration the importance and impact of the Playout phase of this algorithm.

Finally, the behaviour of the Orcs was once again worked on with the addition of **coordinated movement - formations**.

## **Level 1 - Behaviour Trees for Orcs**
The implementation of the patrol behaviour started by editing the *Orc* class, so each specific instance of said class could find the two closest **patrol points**. These patrol points, which can be moved around using the editor, were created manually through Unity and are represented as **green circles**.

A ***Patrol Tree*** was then created to define the patrol itself. The *Patrol Tree* is, in essence, a *Sequence* composite task with two *Selector* composite tasks as its leaves. Each of these Selector tasks is also formed by a *Chase Tree*, which **handles the detection and pursuit of the player** (until they are too far away) and the *MoveTo* task, which is the **basis for the movement between patrol points**. This type of structure **allows the orcs to chase after Sir Uthgard if they spot him after reaching a patrol point**.

Within the *Chase Tree*, there’s also a ***Shout action***. **By detecting the player, a patrolling orc will shout (audio and visual cue are present), making the remaining orcs move to the location from which the shout came from**. After reaching their destination, if the orcs don’t spot the player, they will resume their normal patrol behaviour.

To account for this new action, the *RespondToShout* and *ResumeFromShout* methods were implemented inside the Orc class. As their names suggest, they are relevant to briefly interrupt the behaviour tree execution while a shout is investigated and to resume the patrol movement once the orc reaches the shout location.

A few notes should be taken regarding this level: perhaps the implementation of an Interruptor task would make for a smoother transition between behaviours and allow the detection of the player during the patrol (and not only after reaching a patrol point). The presence of multiple orcs in the same area (for example, with formations enabled) can also lead to the occurrence of a loop of shouts…

## **Level 2 - GOB (Goal-Oriented Behaviour) and GOAP (Goal-Oriented Action Planning)**
After implementing the **Overall Utility GOB** decision making algorithm, which **calculates an overall discontentment value and considers only the best next action to perform**, we can point out that:

- This algorithm is extremely **“short-sighted”**, missing the ability to plan multiple actions at a time, it tends to opt for immediate benefits (that can be minimal) over long-term smarter plays. For example, the character might choose to drink a health potion with only 2 HP points missing and after that choose to fight an enemy who deals 3 points of damage but makes the character level up and have a higher maximum HP.

- There are **actions whose effects are ambiguous while only considering four goals and one action at a time**. Thus, **there is a need for the decision making developer to explicitly “show” that a given action is indirectly good and will decrease the discontentment in the future (author design)**. As an example, getting mana potions might not have an immediate benefit but on the long run allows the main character to fend off more enemies.

- Despite this, GOB stands as an **easy-to-implement and performance light decision-making algorithm**.

The **Depth Limited GOAP** algorithm eliminates some of the disadvantages of having a simpler decision making process like GOB, namely by **considering a sequence of actions instead of just the next immediate action through the creation of copies of the current world state**. Once this depth-first algorithm reaches its pre-defined maximum depth, it calculates the overall discontentment of the last world state and keeps the best course of actions that minimizes this value. Although this results in a **better, more strategic completion of the dungeon**, it also holds the main downfalls of the algorithm:

- The current implementation of the Depth Limited GOAP is **not the best in short-term survivability**... For instance, Sir Uthgard may decide the best sequence of actions is to *SwordAttack* an Orc, *SwordAttack* the Dragon (which will most likely get him killed) and *UseHeathPotion*, ignoring whether he will live after the second action just because the final discontentment is better. We could improve this behaviour by checking if the character has not died after each hypothetical action, although this could lead to a decrease in performance!

- Other similarly odd behaviours are rooted in the same cause – by **choosing a sequence of actions based only on the future discontentment, the main character risks performing actions that hinder or even prevent them from completing “the current smartest move”**.

**This aspect of GOAP can be mitigated (but not eliminated) by increasing the action combinations processed per frame**, hopefully reaching better discontentment values and a sequence of action that does not risk the short-term livelihood of Sir Uthgard. This change, however, comes at a performance cost which should be noted.

Finally, **increasing the depth limit of the algorithm also gives rise to a higher number of action combinations**, although this change **can worsen the downfalls mentioned above since the algorithm looks further into the future, which decreases the relevance of short-term actions**.

## **Level 4 - MCTS (Monte Carlo Tree Search)**
While the GOB and GOAP algorithms both depend on discontentment (heuristics) to find the best possible actions or sequence of actions, the vanilla MCTS (Monte Carlo Tree Search) is not dependent on this and **works well in an isolated environment, where all it has to work with is the game information** (game mechanics/functionalities, states and so on).

Combining both exploration and exploitation to focus on selecting the most promising children (and the most promising sub-trees) and expanding unvisited ones, this algorithm **rests its fundaments on playouts, through which future world states are simulated by applying random actions** and, after reaching a terminal state, a win ratio is associated with each node visited, during the backpropagation phase.

Since the actions applied during the Playout are random, this can lead to an **unbalanced amount of losing games, making most winning ratios associated with nodes equal to 0. Consequently, the selection of the best final action might lead to unfavourable situations.**

For example, since the dragon is a very tough opponent, Sir Uthgard will most likely perish from attacking it if he is not well prepared (in most cases, this implies he needs to be at least Level 3). Since the action *SwordAttack(Dragon)* is always available, many playouts will end in Sir Uthgard attacking this opponent, failing/dying and thus propagating a win ratio of 0 throughout all the nodes visited during that iteration. This happens in a lot of iterations and significantly limits the number of nodes with a positive winning ratio, which leads to poor decision making.

Considering the importance of the Playout phase, some approaches can be implemented to **improve MCTS**:

- The simplest approach is to **boost the number of iterations processed per frame**. Like what happens in the GOAP algorithm, this gives us a wider range of better actions to work with (the exploration of more nodes increases the likelihood of higher/positive win ratios) but **can ultimately reduce the efficiency of the algorithm** (more time before each decision is made).

- By performing **multiple playouts** (introducing a stochastic environment), the number of nodes with a higher/positive win ratio also increases. This version of MCTS was implement in code – **MCTS Multiple Playouts** – and by selecting the MCTS option in the application's main menu (or changing the variables inside the Inspector window), we can even choose the number of Playouts to be performed (one playout corresponds to the vanilla MCTS, of course). **One of the major differences from the original algorithm is the processing time of the decision making**: using 5 playouts, the character went from deciding the next best action in just 40 milliseconds to taking around 150 milliseconds (almost 4 times as long)! Once again, this approach can **hinder the overall performance of the algorithm**.

- Other variations of the vanilla MCTS, such as **MCTS with Biased Playouts** and **MCTS with Limited Playouts** can lead to better results. Both options were implemented and once again, can be accessed through the application's main menu (or by changing the variables inside the Inspector window).

Regarding the **MCTS with Biased Playouts**, **heuristic information related to each action is added to help the playout phase choose estimated best actions**. As an example, health potions were made more relevant later in the game (when Sir Uthgard has a higher level and can therefore heal more health points with a single potion), while praying (which slowly regains health points) had an emphasis early on... The main character is also discouraged from performing attacks (namely sword attacks) that will get him killed, significantly boosting the win ratios of each node. An effort was made to always choose different heuristic values so there would not be a lot of ties between actions, in which case we would come full circle and have random playouts. It is also worth stating the current implementation of the biased playout follows the greedy approach, meaning the algorithm will always choose the action with the lowest heuristic (lowest "cost"). Alternatively, we could have opted for the Gibbs/Softmax approach, although that would increase the complexity and duration of the playouts. Finally, it is worth noting **the performance of the algorithm did not decrease significantly**: the character went from deciding the next best action in just 40 milliseconds to taking around 100 milliseconds (processing timed increased 2.5 times).

Lastly, the **MCST with Limited Playouts** aims to increase the efficiency of MCTS by **limiting the depth of playouts and using a heuristic function to estimate the quality of the world state reached**, which replaces the usual reward value. The heuristic function was implemented in the *FutureStateWorldModel* class and takes into account important stats of the main character in the simulated world state (health points, level and money) which hint to a possible victory or simply a favourable position to be in. This function consists of a weighted sum of these stats, with the resulting heuristic value being normalized and finally returned.

## **Level 5 - Formations For Orcs **
Through the main menu, it is also possible to turn on line and triangle formations for the orc enemies, thus displaying **coordinated movement which is only interrupted once the patrolling squad of monsters detect the player**.

![orcline](https://github.com/user-attachments/assets/e26c6a6d-c6df-4a7a-bb6b-4130a07da163)
![orctriangle](https://github.com/user-attachments/assets/cb4e80a9-2055-4ac2-8413-6c2d02446829)

The NPC squad, composed of a leader and two followers, traverse the environment in the desired formation, following an invisible *NavMeshAgent* – the anchor - that moves between two formation patrol points, which are visually represented by two red squares on the map. The anchor is represented by a yellow diamond shape, which in turn is followed by two blue circles, showcasing the expected positions of the following NPCs. These representations were purposedly kept in so visualising the formation movement could be easier.

In order to implement the patrol movement, instances of the *FormationManager* and *FormationPatrol* classes were initialized in the *GameManager*, considering the type of formation wanted and associating these instances with the orcs mentioned above. The *FormationPatrol* class was created to manage the movement of the anchor point between the two formation patrol points. Finally, both the *GameManager* and the *FormationPatrol* work together to ensure that if the player gets too close to the anchor point (and to the formation itself), the formation is disbanded, and the orcs resume their normal behaviour (which would be attacking the main character).

Some notes can be taken regarding these implementations:

- Although the triangle formation does indeed work as intended, it has a scalability issue, as most fixed formations do – in its current state, it is necessary to implement a different expression for each orc belonging to the formation.

- Secondly, its common to verify unexpected behaviours following this logic for formations since the slots can often go out of bounds/go through walls, leading to a seemingly odd movement from certain formation elements, especially in tight passageways. This can be somewhat mitigated by decreasing the offset variable inside each formation class.

## **Final Thoughts**
It is clear that **most of the non-traditional decision making algorithms need long processes of playtesting and balancing – from deciding how goal values change with actions, to heuristics and change rates, the almost infinite combinations can lead to extremely diverse results**. This way, the implementation of these algorithms should be carefully studied and fully documented to have a deep knowledge of how each variable affects the results. As an example, Pray, which impacted the health points and had a considerable duration, led to multiple balancing issues since Sir Uthgard abused this action.

Secondly, we can state that **although non-traditional decision making algorithms tend to avoid author design, instead relying on a search approach, there are ways of improving the decision making process through author design implementations**. One such example would be the heuristic functions created for the MCTS variants, which are based on what the author thinks is better (and not necessarily the program thinking for itself).

# **Brief Explanation of Sir Uthgard's Abilities**
- Sword Attack - can only be performed at melee range, and monsters will attack Uthgard back. Attacking an enemy will decrease Uthgard's health according to their type, as specified above. The enemy will be dead, and the character will gain experience points (XP) accordingly.
  
- Divine Smite - only usable against undead (skeletons), this ranged special attack allows Uthgard to kill an undead immediately without suffering any damage. It uses 2 mana for each attack.
  
- Speed Up (only available after Sir Uthgard attains level 2) - Uthgard's movement speed is doubled at the cost of 5 Mana. This power up only lasts 10 seconds.

- Shield of Faith - gives a temporary 5 HP Shield that stacks on top of Uthgard's HP. Whenever you take damage, damage is first deducted from the Shield and the remaining damage (if any) is then deducted from Uthgard's HP. If Uthgard recasts Shield of Faith while under the effect of a shield of faith, the new shield will replace the old one. Costs 5 mana points.
  
- Pray - Utghard can pray for 5 seconds, and recover 2 HP at the end of this period.

## **Authors and Acknowledgements**

This project was developed by **[Miguel Belbute (zorrocrisis)](https://github.com/zorrocrisis)** with contributions from Guilherme Serpa.

The initial code was supplied by **[Prof. Pedro dos Santos](https://fenix.tecnico.ulisboa.pt/homepage/ist12886)**

