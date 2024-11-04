using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;

public class BlockGridCreator : MonoBehaviour
{
    public int gridSizeX , gridSizeY ;
    public float blockWidth;
    [HideInInspector]
    public float blockHypotenuse;
    private List<List<Block>> grid;
    private List<Block> gridOneD;
    public Transform blockPrefab;
    private List<Vector3> normalPositions;
    private List<Vector3> upsideDownPositions;
    [HideInInspector]
    public Vector3 LeftJumpVector { private set;  get; }
    [HideInInspector]
    public Vector3 RightJumpVector { private set;  get; }

    private float gridFadeDuration ;
    private GameObject gridParent;
    public bool shouldFadeBlocks = false;
    public float CurrentAlpha
    {
        get;
        private set;
    }

    private void Awake()
    {
        blockHypotenuse = Mathf.Sqrt(blockWidth * 2);
        grid = new List<List<Block>>();
        CreateGrid();
        CopyGridToOneD();
        SetJumpVectors();
        CurrentAlpha = 1;
        FindObjectOfType<ProceduralGenerator>().onProceduralGenerationFinished.AddListener(CreatePositionLists);

    }

    private void Update()
    {
        if(shouldFadeBlocks)
            ReduceGridAlpha();
    }

    public void ResetAlpha()
    {
        CurrentAlpha = 1.01f;
        shouldFadeBlocks = true;
    }

    void ReduceGridAlpha()
    {
        if(CurrentAlpha>0)
            CurrentAlpha -= Time.deltaTime / gridFadeDuration;

        if (CurrentAlpha <= 0)
        {
            CurrentAlpha = 0;
        }
        
        SetGridAlpha(CurrentAlpha);    
    }
    
    
    
    void CopyGridToOneD()
    {
        gridOneD = new List<Block>();
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                gridOneD.Add(grid[i][j]);
            }
        }
    }


    public void SetAlphaToZero()
    {
        CurrentAlpha = 0;
    }
    
    private void CreateGrid()
    {
        gridParent = new GameObject("Grid");
        for(int i =0;i< gridSizeY;i++)
            AddRow(i);
    }

    Block CreateAndPositionAt(int xPos, int yPos)
    {
        Transform block = default;
        block = Instantiate(blockPrefab).transform;
        block.transform.parent = gridParent.transform;
        
        var newPos = new Vector3(xPos * blockHypotenuse + (yPos%2 * blockHypotenuse/2), yPos*blockWidth, yPos * blockHypotenuse/2);
        var newRot = new Vector3(0, 45f, 0);
        
        block.position = newPos;
        block.rotation= Quaternion.Euler(newRot);
        
        var toReturn = block.GetComponent<Block>();
        toReturn.row = yPos;
        toReturn.col = xPos;
        return toReturn;
        
    }

    void AddRow(int finalRowCount)
    {
        if (grid.Count < finalRowCount)
            AddRow(finalRowCount - 1);
        
        if(grid.Count== finalRowCount)
            grid.Add(new List<Block>());

       AddColumns(finalRowCount);
    }

    void AppendRow()
    {
        grid.Add(new List<Block>());
        AddColumns(grid.Count-1);
        
    }

    void AddColumns(int inRow)
    {
        Assert.AreEqual(grid.Count,inRow+1);//always add to the top row
                    
        for (int i = 0; i < gridSizeX; i++)
        {
            if (grid[inRow].Count <= i)//if a block doesn't exist in this row at this col pos
                grid[inRow].Add(CreateAndPositionAt(i,inRow));
            
        }

    }
    public Block GetBlock(int row, int col)
    {   
        return grid[row][col];
    }

    public Block GetBlock(Vec2Int atPos)
    {
        return GetBlock(atPos.row, atPos.col);
    }

    public void ReplaceAt(Vec2Int index, ref Block newBlock)
    {
        var blockatPos = GetBlock(index);

        newBlock.transform.position = blockatPos.transform.position;
        newBlock.transform.rotation = blockatPos.transform.rotation;
        newBlock.transform.parent = blockatPos.transform.parent;

        newBlock.row = blockatPos.row;
        newBlock.col = blockatPos.col;
        
        grid[index.row][index.col] = newBlock;
        gridOneD.Remove(blockatPos);
        gridOneD.Add(newBlock);
        Destroy(blockatPos.gameObject);
        
    }

    public Block GetRandomEmptyBlock()
    {
        var emptyBlockList = gridOneD.Where(x => x.isEmpty == true).ToList();
        var emptyBlock = emptyBlockList[Random.Range(0, emptyBlockList.Count)];
        return emptyBlock;
    }

    public Block GetRandomFilledBlock()
    {
        var filledBlockList = gridOneD.Where(x => !x.isEmpty 
                                                  && !x.HasBlackGem() 
                                                  && !x.HasPlayer 
                                                  && !x.HasRedGem() 
                                                  && !x.HasRedSphere()
                                                  && !x.HasBlackSphere()).ToList();
        var filledBlock = filledBlockList[Random.Range(0, filledBlockList.Count)];
        return filledBlock;
    }

    void SetGridAlpha(float a)
    {
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                var block = grid[i][j];
                block.SetAlpha(a);
            }
        }
    }

    void CreatePositionLists()
    {
        normalPositions = new List<Vector3>();
        upsideDownPositions = new List<Vector3>();
        
        for (int i = 0; i < gridOneD.Count; i++)
        {
            var normalBlock = gridOneD[i];
            var normPos = normalBlock.transform.position;
            normalPositions.Add(normPos);

            var upsideDownPos = normPos;
            upsideDownPos.y +=blockWidth;
            int rowNum = i / gridSizeX;
            float zPos = blockHypotenuse / 2 * (-normalBlock.row);
            //Debug.Log(zPos);
                            
            //upsideDownPos.z = (gridSizeY - 1 - rowNum) * blockHypotenuse/2;
            upsideDownPos.z = zPos;

            upsideDownPositions.Add(upsideDownPos);
        }
    }

    public void SwitchPositions(bool isUpsideDown)
    {
        SetJumpVectors(isUpsideDown);
        /*for (int i = 0; i < gridOneD.Count; i++)
        {
            var posToSet = isUpsideDown ? upsideDownPositions[i] : normalPositions[i];
            gridOneD[i].transform.position = posToSet;
        }*/
    }

    void SetJumpVectors(bool isUpsideDown = false)
    {
        if (isUpsideDown)
        {
            RightJumpVector = grid[0][1].transform.position- grid[1][0].transform.position  ;
            LeftJumpVector = grid[0][0].transform.position - grid[1][0].transform.position ;
            
        }
        else
        {
            LeftJumpVector = grid[1][0].transform.position - grid[0][1].transform.position;
            RightJumpVector = grid[1][0].transform.position - grid[0][0].transform.position;
        }
    }

    public void SetGridFadeDuration(float val)
    {
        gridFadeDuration = val;
    }
}



public enum Directions
{
    left,
    right,
    up,
    down
}
