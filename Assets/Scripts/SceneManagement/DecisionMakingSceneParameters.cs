using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Auxiliary variable for algorithm selection
public enum DecisionAlgorithmActive
{
    GOB,
    GOAP,
    MCTS
}


public static class DecisionMakingSceneParameters
{

	public static DecisionAlgorithmActive algorithmToUse;
}
