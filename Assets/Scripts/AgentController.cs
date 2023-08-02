using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AgentController : MonoBehaviour
{
    private GridBehaviour environment;

    [SerializeField] private int leftVelocity; // left
    [SerializeField] private int rightVelocity; // right
    [SerializeField] private int forwardVelocity; // forward

    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private LayerMask WhatIsAGridLayer;

    private readonly Dictionary<Vector3Int, Dictionary<Tuple<Increments, AgentDirections>, float>> valueFunction = new();
    private readonly Dictionary<Vector3Int, Dictionary<Tuple<Increments, AgentDirections>, List<int>>> Returns = new();
    private readonly List<int> possibleStepsValues = new();
    private readonly List<Vector3Int> possibleStates = new();
    private readonly Actions possibleActions = new();

    public Tuple<Increments, AgentDirections> currentAction = new(Increments.zero, AgentDirections.forward);
    private Vector3Int currentState;
    private int episodesCount = 1000;
    private int currentEpisode = 1;
    private List<Vector3Int> episodeStatesList = new();
    private readonly List<Tuple<Increments, AgentDirections>> episodeActionsList = new();
    public int G = 0; // cummulativeReward

    public Button startBtn;
    private bool started = false;

    public Text episode, timeStep, cummulativeReward;



    // Start is called before the first frame update
    void Start()
    {
        possibleStates.Clear();
        valueFunction.Clear();
        // Initialize possibleStepsValues [-1, 1..5]
        possibleStepsValues.Add(-1);
        for (int i = 1; i < 6; i++)
        {
            possibleStepsValues.Add(i);
        }


        environment = FindObjectOfType<GridBehaviour>();

        //Initialize possibleStates (total of 216 possibilities)
        foreach (int i in possibleStepsValues)
        {
            foreach (int j in possibleStepsValues)
            {
                foreach (int k in possibleStepsValues)
                {
                    possibleStates.Add(new Vector3Int(i, j, k));
                }
            }
        }

        // Initialize Action-Value Function and Returns (total of 216*9=1944 state-action pairs for each dictionary)
        Dictionary<Tuple<Increments, AgentDirections>, float> tempDictVF = new();
        Dictionary<Tuple<Increments, AgentDirections>, List<int>> tempDictR = new();
        tempDictVF.Clear();
        tempDictR.Clear();
        List<int> tempList = new();
        tempList.Clear();
        foreach (var a in possibleActions.list)
        {
            tempDictVF.Add(a, 0);
            tempDictR.Add(a, tempList);
        }
        foreach (var s in possibleStates)
        {
            valueFunction.Add(s, tempDictVF);
            Returns.Add(s, tempDictR);
        }

        episodeStatesList.Clear();
        episodeActionsList.Clear();

        episode.text = "Episode: " + currentEpisode + " / " + episodesCount;
        timeStep.text = "Time step: " + episodeStatesList.Count;
        cummulativeReward.text = "Cummulative Reward: " + G;
        Debug.Log("Done Init!");
        //print("possibleStepsValues " + possibleStepsValues.Count);
        //print("possibleStates " + possibleStates.Count);
        //print(possibleStates[0]);
        //print(valueFunction[possibleStates[0]][new(Increments.zero, AgentDirections.forward)]);
    }

    private void Update()
    {
        if (started)
            StartRun();
    }

    public void StartBtnOnClick()
    {
        started = !started;
    }


    public void StartRun()
    {
        // loop trough episodes
        if (currentEpisode <= episodesCount)
        {
            if (episodeStatesList.Count == 0)
            {
                // Choose S0 ∈ S and A0 ∈ A(S0)
                agentPrefab.GetComponent<AgentMove>().agentState.SetState(agentPrefab, WhatIsAGridLayer, leftVelocity, rightVelocity, forwardVelocity);
                currentState = agentPrefab.GetComponent<AgentMove>().agentState.GetState();
                currentAction = Egreedy(currentState);

                episodeStatesList.Add(currentState);
                episodeActionsList.Add(currentAction);
            }
            else
            {
                if (agentPrefab.GetComponent<AgentMove>().GetPosition().x != environment.columns - 1)
                {
                    TakeAction();
                    currentState = agentPrefab.GetComponent<AgentMove>().agentState.GetState();
                    currentAction = Egreedy(currentState);
                    episodeStatesList.Add(currentState);
                    episodeActionsList.Add(currentAction);
                }
                else
                {
                    G = 0;
                    for (int j = 0; j < episodeStatesList.Count - 1; j++)
                    {
                        G--; // Because all rewards are -1 except at the finish line
                        Vector3Int S_t = episodeStatesList[j];
                        Tuple<Increments, AgentDirections> A_t = episodeActionsList[j];
                        // check if A_t and S_t doesn't exist in the sublist from 0 to j-1
                        bool notFound = NotFoundInSubList(j, S_t, A_t);

                        if (notFound)
                        {
                            Returns[S_t][A_t].Add(G);
                            UpdateActionValueFunction(S_t, A_t, (float)Returns[S_t][A_t].Average());
                        }

                    }

                    currentEpisode++;
                    episodeStatesList.Clear();
                    episodeActionsList.Clear();
                    agentPrefab.GetComponent<AgentMove>().RespawnAgent();
                }
            }
            episode.text = "Episode: " + currentEpisode + " / " + episodesCount;
            timeStep.text = "Time step: " + episodeStatesList.Count;
            cummulativeReward.text = "Cummulative Reward: " + G;

        }
    }

    private bool NotFoundInSubList(int j, Vector3Int S_t, Tuple<Increments, AgentDirections> A_t)
    {
        bool S_tNotFound = true; // Initialize a flag to indicate if S_t is not found in the sublist
        bool A_tNotFound = true;
        for (int k = 0; k < j; k++)
        {
            if (episodeStatesList[k] == S_t)
            {
                S_tNotFound = false; // S_t exists in the sublist
                break;
            }
            if (episodeActionsList[k] == A_t)
            {
                A_tNotFound = false; // S_t exists in the sublist
                break;
            }
        }

        return S_tNotFound && A_tNotFound;
    }

    public void MoveStep()
    {
        Vector2 prevPos = agentPrefab.GetComponent<AgentMove>().GetPosition();
        ComputeNewPosition(prevPos, out int posX, out int posY);

        // Dettach agent from the old cell
        environment.gameGrid[(int)prevPos.x, (int)prevPos.y].GetComponent<GridCell>().objectIsInGridSpace = null;
        environment.gameGrid[(int)prevPos.x, (int)prevPos.y].GetComponent<GridCell>().isOccupied = false;


        // Check the new position from boundaries and walls
        if (posX < 0 || posY >= environment.rows || posY < 0)
        {
            agentPrefab.GetComponent<AgentMove>().RespawnAgent();
            ResetVelocity();
        }
        else
        {

            if (posX < environment.columns)
            {
                CheckWallsAndTakeAction(prevPos, posX, posY);

            }
            else
            {
                CheckWallsAndTakeAction(prevPos, environment.columns - 1, posY);
            }
        }
    }

    // Compute New Position with probability 0.1 of using 0 velocity
    private void ComputeNewPosition(Vector2 prevPos, out int posX, out int posY)
    {
        System.Random rnd = new();
        int val = rnd.Next(1, 10);
        if (val < 2)
        {
            posX = (int)prevPos.x;
            posY = (int)prevPos.y;
        } else
        {
            posX = (leftVelocity > rightVelocity)
                        ? (int)prevPos.x - leftVelocity
                        : (int)prevPos.x + rightVelocity;
            posY = (int)prevPos.y + forwardVelocity;
        }
        
    }

    // Reset Velocity Components
    private void ResetVelocity()
    {
        rightVelocity = 0;
        forwardVelocity = 0;
        leftVelocity = 0;
    }

    // Attach new cell to the agent
    private void AttachAgentToNewCell(int X, int Y)
    {
        environment.gameGrid[X, Y].GetComponent<GridCell>().objectIsInGridSpace = agentPrefab;
        environment.gameGrid[X, Y].GetComponent<GridCell>().isOccupied = true;
    }

    // Check Walls And Take Action
    private void CheckWallsAndTakeAction(Vector2 prevPos, int X, int Y)
    {
        bool wallExists = false;
        // Check the new position from walls
        if ((int)prevPos.x != X)
        {
            for (int i = Math.Min((int)prevPos.x, X); i <= Math.Max((int)prevPos.x, X); i++)
            {
                if (environment.gameGrid[i, Y].GetComponent<GridCell>().isWall)
                {
                    agentPrefab.GetComponent<AgentMove>().RespawnAgent();
                    ResetVelocity();
                    wallExists = true;
                    break;
                }

            }
        }
        else if ((int)prevPos.y != Y)
        {

            for (int j = Math.Min((int)prevPos.y, Y); j <= Math.Max((int)prevPos.y, Y); j++)
            {
                if (environment.gameGrid[X, j].GetComponent<GridCell>().isWall)
                {
                    agentPrefab.GetComponent<AgentMove>().RespawnAgent();

                    ResetVelocity();
                    wallExists = true;
                    break;
                }
            }
        }
        if (!wallExists)
        {
            agentPrefab.GetComponent<AgentMove>().SetPosition(X, Y);

            // Move the agent prefab
            agentPrefab.transform.position = new Vector3(X * environment.scale, Y * environment.scale, -1.3f);
            AttachAgentToNewCell(X, Y);
        }
    }


    // Take Action (move agent)
    public void TakeAction()
    {
        // Change the velocity components
        if (currentAction.Item2 == AgentDirections.left)
        {
            leftVelocity += (int)currentAction.Item1;
            leftVelocity = (leftVelocity > 5) ? 5 : leftVelocity;
            leftVelocity = (leftVelocity < 0) ? 0 : leftVelocity;
            rightVelocity = 0;
            forwardVelocity = 0;
        }
        else if (currentAction.Item2 == AgentDirections.right)
        {
            rightVelocity += (int)currentAction.Item1;
            rightVelocity = (rightVelocity > 5) ? 5 : rightVelocity;
            rightVelocity = (rightVelocity < 0) ? 0 : rightVelocity;
            leftVelocity = 0;
            forwardVelocity = 0;
        } else
        {
            forwardVelocity += (int)currentAction.Item1;
            forwardVelocity = (forwardVelocity > 5) ? 5 : forwardVelocity;
            forwardVelocity = (forwardVelocity < 0) ? 0 : forwardVelocity;
            rightVelocity = 0;
            leftVelocity = 0;
        }

        

        // Move the agent
        MoveStep();
        agentPrefab.GetComponent<AgentMove>().agentState.SetState(agentPrefab, WhatIsAGridLayer, leftVelocity, rightVelocity, forwardVelocity);
    }

    // Update Action-Value function
    public void UpdateActionValueFunction(Vector3Int s, Tuple<Increments, AgentDirections> act, float newVal)
    {
        valueFunction[s][act] = newVal;
    }

    // Greedy Policy function (mapping from state to action)
    public Tuple<Increments, AgentDirections> Pi(Vector3Int s)
    {
        // Get the list of keys corresponding to the max value
        List<Tuple<Increments, AgentDirections>> maxKeys = valueFunction[s].Where(kv => kv.Value == valueFunction[s].Values.Max()).Select(kv => kv.Key).ToList();

        // Get a random key from the list
        System.Random rnd = new();
        Tuple<Increments, AgentDirections> randomMaxKey = maxKeys[rnd.Next(0, maxKeys.Count)];

        return randomMaxKey;
    }

    // epsilon-Greedy Policy function (mapping from state to action)
    public Tuple<Increments, AgentDirections> Egreedy(Vector3Int s)
    {
        System.Random rand = new();
        System.Random rnd = new();
        var e = rand.Next(0, 10);
        if (e < 3)
        {
            Tuple<Increments, AgentDirections> ranKey = valueFunction[s].Keys.ToList()[rnd.Next(0, valueFunction[s].Count)];
            return ranKey;
        }
        else
        {
            // Get the list of keys corresponding to the max value
            List<Tuple<Increments, AgentDirections>> maxKeys = valueFunction[s].Where(kv => kv.Value == valueFunction[s].Values.Max()).Select(kv => kv.Key).ToList();

            // Get a random key from the list

            Tuple<Increments, AgentDirections> randomMaxKey = maxKeys[rnd.Next(0, maxKeys.Count)];

            return randomMaxKey;
        }
        
    }
}
