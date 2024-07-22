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

    // MCTS variables
    public static uint numberOfPlayoutsParameter;
    public static bool biasedPlayoutParameter;
    public static bool limitedPlayoutParameter;

    public static bool stochasticWorld;
}
