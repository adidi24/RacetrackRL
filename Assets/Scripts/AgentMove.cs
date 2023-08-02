using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentMove : MonoBehaviour
{
    [SerializeField] private int posX;
    [SerializeField] private int posY;
    private GridBehaviour environment;
    public AgentState agentState = new();
    public Button spawnBtn;


    public void SetPosition(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(posX, posY);
    }

    // Spawn the agent randomly at the starting line
    public void SpawnAgent()
    {
        environment = FindObjectOfType<GridBehaviour>();
        environment.startingLine = new List<GridCell>();
        // loop over the first row in the grid and add cells to starting line if not wall
        for (int i = 0; i < environment.columns; i++)
        {
            GridCell cell = environment.gameGrid[i, 0].GetComponent<GridCell>();
            if (!cell.isWall)
            {
                environment.startingLine.Add(cell);
            }
        }
        // loop over the last column in the grid and add cells to finish line if not wall
        for (int j = 0; j < environment.rows; j++)
        {
            GridCell cell = environment.gameGrid[environment.columns - 1, j].GetComponent<GridCell>();
            if (!cell.isWall)
            {
                cell.GetComponent<MeshRenderer>().material.color = Color.green;
                environment.finishLine.Add(cell);
            }
        }

        // take a random position at the starting line to instanciate the agent
        var random = new System.Random();
        int index = random.Next(environment.startingLine.Count);
        int x = environment.startingLine[index].getPosition().x;
        int y = environment.startingLine[index].getPosition().y;

        // Set agent position
        transform.position = new Vector3(x * environment.scale, y * environment.scale, -1.3f);
        gameObject.GetComponent<AgentMove>().SetPosition(x, y);

        // make the first cell occupied
        environment.startingLine[index].objectIsInGridSpace = gameObject;
        environment.startingLine[index].isOccupied = true;
        spawnBtn.enabled = false;

        gameObject.SetActive(true);
    }

    // Respawn the agent randomly at the starting line
    public void RespawnAgent()
    {
        environment = FindObjectOfType<GridBehaviour>();

        // take a random position at the starting line to instanciate the agent
        var random = new System.Random();
        int index = random.Next(environment.startingLine.Count);
        int x = environment.startingLine[index].getPosition().x;
        int y = environment.startingLine[index].getPosition().y;

        // Set agent position
        transform.position = new Vector3(x * environment.scale, y * environment.scale, -1.3f);
        gameObject.GetComponent<AgentMove>().SetPosition(x, y);

        // make the first cell occupied
        environment.startingLine[index].objectIsInGridSpace = gameObject;
        environment.startingLine[index].isOccupied = true;
        spawnBtn.enabled = false;

        gameObject.SetActive(true);
    }
}
