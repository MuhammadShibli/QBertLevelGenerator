using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

using System.Linq;
public class ProceduralGenerator : MonoBehaviour
{
    [HideInInspector] public UnityEvent onProceduralGenerationFinished;
    
    [Header("Level Design")] 
    public int walkerSteps = 5;
    public int fillMapNumber = 5;

    private const float gemYOffset = 1f;


    [Header("Prefabs")] public GameObject emptyBlock;
    public GameObject blackGemPrefab;
    public GameObject filledBlock;
    public GameObject redGem;
    public GameObject redSpherePrefab;
    public GameObject blackSpherePrefab;
    public GameObject playerPrefab;
    
    private int numberOfFilledBlocks;
    private int numberOfBlockAnimationsFinished = 0;
    private PlayerController player;
    private BlockGridCreator grid;

    private Block lastBlock = null;
    private Block startingBlock = null;
    private bool playerSpawned = false;
    
    private void Awake()
    {
        grid = FindObjectOfType<BlockGridCreator>();
        numberOfFilledBlocks = fillMapNumber + walkerSteps;
    }

    private void Start()
    { 
        GenerateLevel();
    }
    
    private void FillMap(Vec2Int[] emptyBlocks)
    {
        // fill the entire grid with filled blocks except for the empty blocks
        for (int iRow = 0; iRow < grid.gridSizeY; iRow++)
        {
            for (int iCol = 0; iCol < grid.gridSizeX; iCol++)
            {
                
                var indexNotFoundInEmptyBlocks = emptyBlocks.FirstOrDefault(entry => entry.row == iRow && entry.col == iCol) ==null;
                
                var block = grid.GetBlock(iRow,iCol);
                if (block != startingBlock  && indexNotFoundInEmptyBlocks)
                {
                    block = ReplaceWithFilled(block);
                }
            }
        }
    }

    public void GenerateLevel()
    {
        startingBlock = grid.GetBlock(Random.Range(0, grid.gridSizeY), Random.Range(0, grid.gridSizeX));
        startingBlock = ReplaceWithFilled(startingBlock);
        lastBlock = SpawnPath(startingBlock);
        FillMap();
        onProceduralGenerationFinished.Invoke();
    }
    
    void AssignStartingBlock(Vec2Int pos)
    {
        var startPos = pos;
        startingBlock = grid.GetBlock(startPos.row, startPos.col);
    }

    public void OnBlockAnimationFinished()
    {
        numberOfBlockAnimationsFinished++;
        if (numberOfBlockAnimationsFinished == numberOfFilledBlocks)
        {
            //Debug.Log("BlockAnimationFinished");
            SpawnBlackGem(lastBlock);
            SpawnRandomPowerUp();
            if(!playerSpawned)
                SpawnPlayer(startingBlock);
        }
    }

    void SpawnRandomPowerUp()
    {
        float spawnProbability = Random.value;
        if (spawnProbability <= 0.15f)
        {
            int randSpawn = Random.Range(0, 3);
            switch (randSpawn)
            {
                case 0:
                    SpawnRedGem();
                    break;
                case 1:
                    SpawnRedSphere();
                    break;
                case 2:
                    SpawnBlackSphere();
                    break;
            }    
        }
        
    }

    private bool gemAnimationFinished = false;
    
    public void OnGemAnimationFinished()
    {
        if (!gemAnimationFinished)
        {
            //Debug.Log("GemAnimationFinished");
            gemAnimationFinished = true;
        }    
    }

    void SpawnRedGem()
    {
        var redGemInstantiated = Instantiate(this.redGem).GetComponent<RedGemController>();
        
        var aFilledBlock = grid.GetRandomFilledBlock();
        aFilledBlock.AddRedGem(redGemInstantiated.transform,gemYOffset);
        redGemInstantiated.block = aFilledBlock;
    }

    void SpawnRedSphere()
    {
        var redSphere = Instantiate(redSpherePrefab).GetComponent<RedSphereController>();
        var aFilledBlock = grid.GetRandomFilledBlock();
        aFilledBlock.AddRedSphere(redSphere.transform,gemYOffset);
        redSphere.myBlock = aFilledBlock;
    }
    
    void FillMap()
    {
        for (int i = 0; i < fillMapNumber; i++)
        {
            ReplaceWithFilled(grid.GetRandomEmptyBlock());
        }
    }


    Block SpawnPath(Block startingBlockRef)
    {
        ReplaceWithFilled(startingBlockRef);
        Block stepRefBlock = startingBlockRef;
        for (int i = 0; i < walkerSteps; i++)
        {
            Vec2Int nextStep = TakeRandomStep(stepRefBlock);
            //spawn a tile at this new position
            var newBlock = ReplaceWithFilled(nextStep);
            //set the block at this position as the stepRefBlock
            stepRefBlock = newBlock;
        }

        return stepRefBlock;
    }

    void SpawnPlayer(Block startingBlockRef)
    {
        var playerGameObject = Instantiate(playerPrefab);
        playerGameObject.transform.position =  this.startingBlock.transform.position + Vector3.up * 1.5f;
        startingBlockRef.HasPlayer = true;
    }

    void SpawnBlackGem(Block onBlock)
    {
        var blackGem = Instantiate(this.blackGemPrefab).transform;
        //log the position of the black gem from onblocks row and col
        onBlock.AddBlackGem(blackGem,gemYOffset);
    }

    Vec2Int TakeRandomStep(int fromRow, int fromCol)
    {
        int jumpDir = Random.value > 0.5f ? 1 : -1;
        
        return TakeStep(fromRow, fromCol, jumpDir);
    }
    
    Vec2Int TakeStep(int fromRow, int fromCol, int jumpDir)
    {

        int nextRow, nextCol;

        if (jumpDir==-1)//go left
        {
            if(fromRow % 2==0 || fromRow==grid.gridSizeY-1)//if it is even or at the top
                nextCol = fromCol - 1;
            else
            {
                nextCol = fromCol;
            }
            if (nextCol < 0)//wrap left to right
                nextCol = grid.gridSizeX - 1;
        }
        else //go right
        {
            if(fromRow % 2==0 )//if it is even 
                nextCol = fromCol;
            else
            {
                nextCol = fromCol+1;
            }
            if (nextCol >= grid.gridSizeX)//wrap right to left
                nextCol = 0;
        }

        nextRow = (fromRow+1) % grid.gridSizeY;
        
        Vec2Int toReturn = new Vec2Int(nextRow,nextCol);

        return toReturn;
    }

    Vec2Int TakeRandomStep(Block from)
    {
        return TakeRandomStep(from.row, from.col);
    }

    Block ReplaceWithFilled(Vec2Int blockPos)
    {
        Block toReturn = default;
        
        var blockatPos = grid.GetBlock(blockPos);
        if (blockatPos.isEmpty)
        {
            toReturn = Instantiate(filledBlock).GetComponent<Block>();
            grid.ReplaceAt(blockPos,ref toReturn);
        }
        else
        {
            toReturn = blockatPos;

        }
        return toReturn;
    }

    Block ReplaceWithFilled(Block block)
    {
        return ReplaceWithFilled(new Vec2Int(block.row, block.col));
    }

    public void SpawnRedGemAfterDelay()
    {
        StartCoroutine(SpawnRedGemCoroutine());
    }

    IEnumerator SpawnRedGemCoroutine()
    {
        float randomWaitTime = Random.Range(0.5f, 1.2f);
        yield return new WaitForSeconds(randomWaitTime);
        SpawnRedGem();
    }

    public void SpawnRedSphereAfterDelay()
    {
        StartCoroutine(SpawnRedSphereCoroutine());
    }

    IEnumerator SpawnRedSphereCoroutine()
    {
        float randomWaitTime = Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomWaitTime);
        SpawnRedSphere();
    }

    public void SpawnBlackSphere()
    {
        //StartCoroutine(SpawnBlackSphereCoroutine());
        var blackSphere = Instantiate(blackSpherePrefab).GetComponent<BlackSphereCont>();
        var randomFilledBlock = grid.GetRandomFilledBlock();
        randomFilledBlock.AddBlackSphere(blackSphere.transform, gemYOffset);
        blackSphere.myBlock = randomFilledBlock;
    }

    IEnumerator SpawnBlackSphereCoroutine()
    {
        bool shouldSpawn = false;
        while (!shouldSpawn)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            var prob = Random.value;
            if (prob <= 0.5f)
            {
                shouldSpawn = true;
            }
        }
        
        var blackSphere = Instantiate(blackSpherePrefab).GetComponent<BlackSphereCont>();
        var randomFilledBlock = grid.GetRandomFilledBlock();
        randomFilledBlock.AddBlackSphere(blackSphere.transform, gemYOffset);
        blackSphere.myBlock = randomFilledBlock;
    }

    
}

[Serializable]
public class Vec2Int
{
    public int row;
    public int col;

    public Vec2Int(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
}