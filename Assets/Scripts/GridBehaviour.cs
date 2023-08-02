using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBehaviour : MonoBehaviour
{
    public int rows = 30;
    public int columns = 30;
    public float scale = 1.1f;
    [SerializeField] private GameObject gridPrefab;
    public GameObject[,] gameGrid;
    [SerializeField] private Transform _cam;
    public List<GridCell> startingLine;
    public List<GridCell> finishLine;

    // Start is called before the first frame update
    void Start()
    {
        if (gridPrefab)
            GenerateGrid();
        else print("missing grid prefab, please assign");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateGrid()
    {
        gameGrid = new GameObject[columns, rows];
        for (int y = 0; y < columns; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                gameGrid[x, y] = Instantiate(gridPrefab, new Vector3(x * scale, y * scale), Quaternion.identity);
                gameGrid[x, y].GetComponent<GridCell>().setPosition(x, y);
                gameGrid[x, y].transform.parent = transform;
                gameGrid[x, y].gameObject.name = "Grid Space (x :" + x.ToString() + ", y : " + y.ToString() + ")";

            }

        }

        _cam.transform.position = new Vector3((float)rows * scale / 2, -3, -19);
        _cam.transform.Rotate(-34, 0, 0);
    }

    public Vector2Int GetGridPosFromWorld(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / scale);
        int y = Mathf.FloorToInt(worldPosition.z / scale);

        x = Mathf.Clamp(x, 0, rows);
        y = Mathf.Clamp(y, 0, columns);

        return new Vector2Int(x, y);
    }


    public Vector3 GetworldPosFromGridPos(Vector2Int gridPos)
    {
        float x = gridPos.x * scale;
        float y = gridPos.y * scale;
        return new Vector3(x, 0, y);
    }


}
