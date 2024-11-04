using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class TestGridGenerator : MonoBehaviour
{
    public List<GridConfig> gridConfigs;
    public List<GameObject> lights;

    [Header("Level Design")] 
    private float blockHypotenuse;
    public float blockWidth = 1;
    public int gridSizeX = 3;
    public int gridSizeY = 6;
    public GameObject blockPrefabNormal;
    public CinemachineMixingCamera mixingCamera;

    public bool switchGrid = false;
    private int currentGrid = 0;
    private void Awake()
    {
        blockHypotenuse = Mathf.Sqrt(2 * (blockWidth * blockWidth));
        gridConfigs = new List<GridConfig>();
        CreateNormalGrid();
        CreateUpsideDownGrid();
    }

    private void Start()
    {
        SwitchGrid(1);

    }

    private void Update()
    {
        if (switchGrid)
        {
            switchGrid = false;
            int newGrid = (currentGrid + 1) % gridConfigs.Count;
            SwitchGrid(newGrid);
        }
    }

    public void SwitchGrid(int toGridIndex)
    {
        DeactivateGridsExcept(toGridIndex);
        currentGrid = toGridIndex;
    }

    private void DeactivateGridsExcept(int except)
    {
        for (int i = 0; i < gridConfigs.Count; i++)
        {
             var parent = gridConfigs[i].grid[0].transform.parent;
             if (i == except)
             {
                 parent.gameObject.SetActive(true);
                 lights[i].SetActive(true);
                 mixingCamera.SetWeight(i,1);
             }
             else
             {
                 parent.gameObject.SetActive(false);
                 lights[i].SetActive(false);
                 mixingCamera.SetWeight(i,0);

             }
        }
    }

    void CreateNormalGrid()
    {
        GameObject normalGridParent = new GameObject("Normal Grid");
        List<Block> normalGrid = new List<Block>();
        
        for (int i = 0; i < gridSizeY; i++)
        {
            for (int j = 0; j < gridSizeX; j++)
            {
                var block = CreateAndPositionAt(j, i);
                block.transform.SetParent(normalGridParent.transform);
                normalGrid.Add(block);
            }
        }

        gridConfigs.Add( new GridConfig(normalGrid));
    }
    
    Block CreateAndPositionAt(int xPos, int yPos)
    {
        Transform block = default;
        block = Instantiate(blockPrefabNormal).transform;
        
        var newPos = new Vector3(xPos * blockHypotenuse + (yPos%2 * blockHypotenuse/2), yPos*blockWidth, yPos * blockHypotenuse/2);
        var newRot = new Vector3(0, 45f, 0);
        
        block.position = newPos;
        block.rotation= Quaternion.Euler(newRot);
        
        var toReturn = block.GetComponent<Block>();
        toReturn.row = yPos;
        toReturn.col = xPos;
        return toReturn;
        
    }

    void CreateUpsideDownGrid()
    {
        GameObject upsideDownGridParent = new GameObject("UpsideDown Grid");
        List<Block> upsideDownGrid = new List<Block>();
        
        for (int i = 0; i < gridSizeY; i++)
        {
            for (int j = 0; j < gridSizeX; j++)
            {
                var block = Instantiate(blockPrefabNormal).transform;
                var normalBlock = gridConfigs[0].grid[i * gridSizeX + j].transform;
                var normPos = normalBlock.position;
                normPos.y = i+blockWidth;
                normPos.z = (gridSizeY - 1 - i) * blockHypotenuse/2;
                //normPos.z = i * blockHypotenuse / 2;
                block.position = normPos;
                
                block.rotation = normalBlock.rotation;
                
                block.SetParent(upsideDownGridParent.transform);
                upsideDownGrid.Add(block.GetComponent<Block>());
            }
        }

        gridConfigs.Add(new GridConfig(upsideDownGrid));
    }


}

public struct GridConfig
{
    public List<Block> grid;

    public GridConfig(List<Block> grid)
    {
        this.grid = grid;
    }
    
    
}
