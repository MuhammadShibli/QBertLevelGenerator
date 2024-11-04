using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

using Vector3 = UnityEngine.Vector3;


using DG;
using Sirenix.OdinInspector;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
  /*  
    public Directions currentDirection;
    
    public Block CurrentBlock { get; private set; }
    private BlockGridCreator gridCreator;
    //public float verticalOffset=0.5f;
    
    public InputAction jumpInputAction;
    private bool isJumping = false;
    public JumpEvent OnJumpEnded;
    public JumpEvent OnJumpStarted;
    private float jumpDuration=0.12f;
    public Transform model;
    private Transform cloneModel;
    private bool isFalling = false;
    private bool canFall = true;
    private bool landedOnEmptyBlock = false;
    
    private WaitForSeconds waitForJumpEnd;

    private Vector3 debugClonePosition;
    private Vector3 debugCloneRay;

    public bool inputActivated = false;

    public LayerMask blockRaycastMask;
    private bool isUpsideDown = false;
    
    float VerticalOffset { get; set; }
    float JumpPower { get; set; }
    private float GravityDirection { get; set; }

    private AudioManager audioManager;
    public MeshRenderer playerRenderer;
    private float autoJumpInterval = 1f;
    private int autoJumpDirection = 1;
    private float lastAutoJumpTime = 0;
    private bool wasJumpPressed = false;
    public bool autoJumpEnabled = false;
    
    private void Awake()
    {
        audioManager =  FindObjectOfType<AudioManager>();
        debugClonePosition = model.position;
        debugCloneRay =Vector3.back;
        
        gridCreator = FindObjectOfType<BlockGridCreator>();
        jumpInputAction.started += OnJumpInputAction;
        waitForJumpEnd = new WaitForSeconds(jumpDuration);
    }

    
    private void Update()
    {
        if (wasJumpPressed)
        {
            wasJumpPressed = false;
            lastAutoJumpTime = Time.time;
        }

        if (!isJumping && inputActivated)
        {
            if (autoJumpEnabled && (Time.time - lastAutoJumpTime >= autoJumpInterval))
            {
                if(ShouldAutoJump())
                    HandleInput(autoJumpDirection,1);
                lastAutoJumpTime = Time.time;
            }    
        }
    }

    public void SetAutoJumpTimer()
    {
        lastAutoJumpTime = Time.time;
    }
    

    private void OnEnable()
    {
        jumpInputAction.Enable();
    }

    private void OnDisable()
    {
        jumpInputAction.Disable();
    }

    private void OnJumpInputAction(InputAction.CallbackContext obj)
    {
        wasJumpPressed = true;
        
        int inputDirection = obj.ReadValue<float>() > 0 ? 1 : -1;
        autoJumpDirection = inputDirection;

        HandleInput(inputDirection);
    }

    public void HandleInput(int inputDirection, int numOfBlocks = 1)
    {
        if (!inputActivated)
            return;
        PreJump(inputDirection,numOfBlocks);

    }

    void PreJump(int inputDir, int numOfBlocks)
    {
        if(isJumping || isFalling)
            return;

        isJumping = true;
        
        OnJumpStarted.Invoke(CurrentBlock.row,CurrentBlock.col);
        Block startingBlock = CurrentBlock;
        Block landingBlock = default;
        bool spawnClone = false;
        for (int i = 0; i < numOfBlocks; i++)
        {
            landingBlock = startingBlock.GetImmediateNeighbour(inputDir, isUpsideDown);
            if (landingBlock == null)
            {
                spawnClone = true;
                landingBlock = startingBlock.GetNeighbourWrapAround(inputDir, isUpsideDown);
            }

            startingBlock = landingBlock;
        }
       
        if(!spawnClone){
            
            StartModelJump(landingBlock);
        }
        else
        {
            Vector3 cloneStartPosition = GetCloneStartingPosition(landingBlock,inputDir,numOfBlocks);
            Vector3 jumpDir = inputDir == 1 ? gridCreator.RightJumpVector : gridCreator.LeftJumpVector;
            StartCloneJump(cloneStartPosition,landingBlock,jumpDir);
            
            Vector3 modelLandingPosition = GetModelLandingPosition(inputDir, numOfBlocks);
            StartModelFakeJump(modelLandingPosition);
            
        }
        audioManager.PlayJumpSFX();
    }
    
    
    Vector3 GetCloneStartingPosition(Block landingBlock, int inputDir, int numOfBlocks)
    {
        Vector3 cloneStartingPos = default;
        Vector3 jumpVector = inputDir == 1 ? gridCreator.RightJumpVector : gridCreator.LeftJumpVector;

        cloneStartingPos = landingBlock.transform.position - (jumpVector * numOfBlocks);
        return cloneStartingPos;

    }

    Block GetCloneLandingBlock( int inputDir, int numOfBlocks )
    {
        int nextCol = inputDir==1? CurrentBlock.col+1 : CurrentBlock.col-1;
        if (inputDir == 1 && CurrentBlock.col + numOfBlocks > gridCreator.gridSizeX - 1) //jumping right out of screen
        {
            int extraBlocks = (CurrentBlock.col + 1 + numOfBlocks) - gridCreator.gridSizeX;
            nextCol = extraBlocks / gridCreator.gridSizeX;

        }
        else if (inputDir==-1 && (CurrentBlock.col -numOfBlocks<0))
        {
            nextCol = (gridCreator.gridSizeX  - (Mathf.Abs(CurrentBlock.col - numOfBlocks) % gridCreator.gridSizeX));
        }
        

        int nextRow = default;
        if (isUpsideDown)
        {
            nextRow = CurrentBlock.row - numOfBlocks;

            if (nextRow < 0)
            {
                nextRow = (gridCreator.gridSizeY - Mathf.Abs(nextRow) % gridCreator.gridSizeY)% gridCreator.gridSizeY;

            }
        }
        else
        {
            nextRow  = (CurrentBlock.row + numOfBlocks) % gridCreator.gridSizeY;
        }

        Block cloneLandingBlock = gridCreator.GetBlock(nextRow, nextCol);
        return cloneLandingBlock;
    }

    Vector3 GetModelLandingPosition(int inputDir, int numOfBlocks)
    {
        Vector3 jumpVector = inputDir == 1 ? gridCreator.RightJumpVector : gridCreator.LeftJumpVector;
        Vector3 modelLandingPosition = model.transform.position + jumpVector * numOfBlocks;
        return modelLandingPosition;
    }
    
    void StartModelJump(Block landingBlock)
    {
        Vector3 landingPos = landingBlock.transform.position + Vector3.up * VerticalOffset;
        Sequence modelJumpSeq = DOTween.Sequence();
        modelJumpSeq.Append(model.DOJump(landingPos, JumpPower, 1, jumpDuration));
        Sequence modelStretchSeq = DOTween.Sequence();
        
        var cube = model.GetChild(0);
        modelStretchSeq.Prepend(cube.DOScale(new Vector3(0.25f, 1.5f, 0.5f), jumpDuration *0.6f));
        modelStretchSeq.Append(cube.DOScale(Vector3.one, jumpDuration*0.4f));
        modelStretchSeq.Play();
        
        StartCoroutine(WaitForJumpEndCoroutine(modelJumpSeq,landingBlock));
    }
    
    void StartCloneJump(Vector3 cloneStartPosition, Block landingBlock, Vector3 jumpDir)
    {
        cloneModel = Instantiate(model);
        cloneModel.transform.position = cloneStartPosition;

        Sequence cloneJumpSeq = DOTween.Sequence();
        cloneJumpSeq.Append((cloneModel.DOJump(landingBlock.transform.position + Vector3.up * VerticalOffset, JumpPower, 1,
            jumpDuration)));

        var cloneChild = cloneModel.GetChild(0);
        cloneChild.DOScale(Vector3.zero, jumpDuration/2f).From().SetDelay(jumpDuration/2f) ;
        StartCoroutine(CloneReplaceCoroutine(landingBlock,cloneJumpSeq));

    }

    void StartModelFakeJump(Vector3 landingPos)
    {
        Sequence modelJumpSeq = DOTween.Sequence();
        modelJumpSeq.Append(model.DOJump(landingPos, JumpPower, 1, jumpDuration));
        modelJumpSeq.Join(model.DOScale(Vector3.zero, jumpDuration/3f));
    }

    IEnumerator CloneReplaceCoroutine(Block landingBlock,Sequence cloneJumpSeq)
    {
        yield return cloneJumpSeq.WaitForPosition(0.5f);
        yield return cloneJumpSeq.WaitForCompletion();
        ReplaceOriginalModelWithClone(landingBlock);
        PostJump(landingBlock);
    }
    
    IEnumerator WaitForJumpEndCoroutine(Sequence jumpSequence, Block landingBlock)
    {
        yield return jumpSequence.WaitForCompletion();
        PostJump(landingBlock);
    }
    
    void ReplaceOriginalModelWithClone(Block block)
    {
        GameObject toDestroy = model.gameObject;
        model = cloneModel;
        Destroy(toDestroy);
        PostJump(block);
    }

    void PostJump(Block landingBlock)
    {
        CurrentBlock.HasPlayer = false;
        CurrentBlock = landingBlock;
        CurrentBlock.HasPlayer = true;
        
        isJumping = false;
        OnJumpEnded.Invoke(landingBlock.row,landingBlock.col);
        if (CurrentBlock.HasRedGem())
        {
            StartCoroutine(Teleport());
        }
        else if(ShouldFall())
        {
            if (canFall)
                Fall();
            else
                landedOnEmptyBlock = true;
        }
        else //landed on a filled block, check if previously it landed on an empty block
        {
            if (canFall == false && landedOnEmptyBlock)
            {
                canFall = true;
                landedOnEmptyBlock = false;
            }
        }
        
    }

    int FindAutoJumpDirection()
    {
        int randJumpDirection = Random.value >= 0.5f ? -1 : 1;
        Block aNeighbour = CurrentBlock.GetAnyNeighbour(randJumpDirection, isUpsideDown);
        Block anotherNeighbour = CurrentBlock.GetAnyNeighbour(randJumpDirection * -1, isUpsideDown);
        
        if ( aNeighbour!= null && !aNeighbour.isEmpty)
            return randJumpDirection;
        else if (anotherNeighbour!= null && !anotherNeighbour.isEmpty)
            return randJumpDirection * -1;

        return 0;
    }

    bool ShouldAutoJump()
    {
        Block aNeighbour = CurrentBlock.GetAnyNeighbour(autoJumpDirection, isUpsideDown);
        if (aNeighbour != null && !aNeighbour.isEmpty)
            return true;
        
        return false;
    }

    public void OnTouchedBlackGemCAllBACK()
    {
        canFall = false;
        CurrentBlock.RemoveBlackSphere();
        
    }

    public bool CannotFall()
    {
//        Debug.Log("Player can fall : "+canFall.ToString());
        return !canFall;
    }
    
    public void SpawnAt(Block block,Directions startingDirection = Directions.up)
    {
        CurrentBlock = block;
        SetMovementDirections(isUpsideDown);
        RepositionPlayer();
        currentDirection = startingDirection;
        //OnJumpEnded.Invoke(CurrentBlock.row,CurrentBlock.col);//make the grid spawn more blocks
    }

    void Fall()
    {
        if (!isFalling)
        {
            isFalling = true;
            inputActivated = false;

            var cubeModel = model.transform.GetChild(0);
            cubeModel.localPosition = new Vector3(0, 0, 0);

            model.DOMove(Vector3.down * GravityDirection * 8f, 1f).SetRelative(true).SetEase(Ease.OutExpo).SetLoops(1)
                .OnComplete(() =>
                {
                    FindObjectOfType<BlockGridCreator>().SetAlphaToZero();
                    var gm = FindObjectOfType<GameManager>();
                    gm.EndLevelAfterFall();
                    model.DOKill();
                });

        }
    }

    public void ForceFall()
    {
        Fall();
    }

    bool ShouldFall()
    {
        return CurrentBlock.isEmpty;
    }

    public void Rewind()
    {
        Debug.Log("Rewinding player");
        FindObjectOfType<AudioManager>().PlayGemSFX();
        inputActivated = false;
        var redGem = CurrentBlock.RemoveRedGem();
        int numOfSteps = redGem.numOfStepsToRewind;
        Destroy(redGem.gameObject);
        
        var gameStateRec = FindObjectOfType<GameStateRecorder>();
        var newState = gameStateRec.Rewind(numOfSteps);
        var block = gridCreator.GetBlock(newState.gridPosition);

        CurrentBlock.HasPlayer = false;
        
        CurrentBlock = block;
        CurrentBlock.HasPlayer = true;
        gridCreator.Rewind(newState);
        RepositionPlayer();        
        inputActivated = true;
        FindObjectOfType<ProceduralGenerator>().SpawnRedGemAfterDelay();
    }

    IEnumerator Teleport()
    {
        FindObjectOfType<AudioManager>().PlayGemSFX();
        inputActivated = false;
        while (isJumping)
        {
            yield return null;
        }

        Debug.Log("Teleporting Player");

        var redGem = CurrentBlock.RemoveRedGem();
        Destroy(redGem.gameObject);

        var newBlock = gridCreator.GetRandomFilledBlock();
        CurrentBlock.HasPlayer = false;
        CurrentBlock = newBlock;
        CurrentBlock.HasPlayer = true;
        RepositionPlayer();
        inputActivated = true;
    }

    public void HidePlayer()
    {
        if (playerRenderer == null)
            playerRenderer = model.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        playerRenderer.enabled = false;
    }

    public void ShowPlayer()
    {
        if (playerRenderer == null)
            playerRenderer = model.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        playerRenderer.enabled = true;
    }

    public void SetMovementDirections(bool isUpsideDown)
    {
        this.isUpsideDown = isUpsideDown;
        float absoluteJumpPower = 1.3f;
        float absoluteVerticalOffset =0.3f + gridCreator.blockWidth / 2f;
        
        GravityDirection = isUpsideDown ? -1 : 1;
        JumpPower = isUpsideDown ? -absoluteJumpPower : absoluteJumpPower;
        VerticalOffset = isUpsideDown ? absoluteVerticalOffset/2f : absoluteVerticalOffset;
    }

    private IEnumerator flipUpsideDownCoroutine;
    
    public void FlipUpsideDown(bool isUpsideDown)
    {
        if (flipUpsideDownCoroutine==null)
        {
            this.isUpsideDown = isUpsideDown;
            flipUpsideDownCoroutine = FlipUpsideDownCoroutine();
            StartCoroutine(flipUpsideDownCoroutine);
        }
    }

    IEnumerator FlipUpsideDownCoroutine()
    {
        while (isJumping)
            yield return null;
        SetMovementDirections(isUpsideDown);
        RepositionPlayer();
        OrientPlayer(isUpsideDown);
        flipUpsideDownCoroutine = null; 
    }

    void RepositionPlayer()
    {
        if (!isJumping)
        {
            model.position = CurrentBlock.transform.position + Vector3.up * VerticalOffset;
        }
    }
    
    void OrientPlayer(bool isUpsideDown )
    {
        var cubeModel = model.transform.GetChild(0);
        if (isUpsideDown)
        {
            cubeModel.localRotation = Quaternion.Euler(-93.35f,-161.586f,160.758f);
            cubeModel.localPosition = new Vector3(-0.019f, 0.92f, -1.08f);
        }
        else
        {
            cubeModel.localRotation = Quaternion.Euler(0,0,0);
            cubeModel.localPosition = new Vector3(0, 0, 0);
        }
    }
    public Vec2Int CurrentGridPosition()
    {
        return new Vec2Int(CurrentBlock.row, CurrentBlock.col);
    }

    public void TouchedBlackGem()
    {
        inputActivated = false;
        audioManager.PlayGemSFX();
       PlayDanceAnimation();
    }
    //ADD HEADER FOR DANCE AND MAKE THE VARIABLES PUBLIC SO THAT THEY CAN BE CHANGED IN THE INSPECTOR
    
        
   
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        //Gizmos.DrawSphere(debugClonePosition,0.5f);
        Gizmos.DrawRay(debugClonePosition,debugCloneRay);
        
        Gizmos.color = Color.green;

        Vector3 rayOrigin = model.position;
        Vector3 rayDirRight = Vector3.forward + Vector3.right ;
        Vector3 rayDirLeft = Vector3.forward + Vector3.left;
        Gizmos.DrawRay(rayOrigin,rayDirRight);
        Gizmos.DrawRay(rayOrigin,rayDirLeft);
        
        
        Gizmos.color = Color.magenta;

        Vector3 rayRightDown = model.position + ( Vector3.right + Vector3.back ) * 0.5f ;
        Vector3 rayLeftDown =model.position + ( Vector3.left + Vector3.back ) * 0.5f;
        Gizmos.DrawRay(rayRightDown,Vector3.down*2);
        Gizmos.DrawRay(rayLeftDown,Vector3.down*2
        );
        
    }
    
    void PreJumpBackup(int inputDir, int numOfBlocks)
    {
        if(isJumping || isFalling)
            return;

        isJumping = true;
        
        OnJumpStarted.Invoke(CurrentBlock.row,CurrentBlock.col);
        
        Vector3 jumpDir = inputDir == 1 ? gridCreator.RightJumpVector : gridCreator.LeftJumpVector;
        
        RaycastHit hitInfo;
        Vector3 rayOrigin = model.position;
        rayOrigin.y+= gridCreator.blockWidth * (numOfBlocks-1);//offset vertically to cover the number of blocks in the jump
        Vector3 rayDir = inputDir == 1 ? Vector3.forward + Vector3.right : Vector3.forward + Vector3.left;
        rayDir = rayDir.normalized;
        
        bool blockFound = false;
        
        blockFound = Physics.Raycast(new Ray(rayOrigin,rayDir), out hitInfo, 5f+numOfBlocks * gridCreator.blockHypotenuse*2f,blockRaycastMask);

        if (blockFound)
        {
            Block landingBlock = hitInfo.collider.GetComponentInParent<Block>();
            
            //Debug.LogFormat("Found {0} and it has Block component : {1}", hitInfo.collider.name, landingBlock!=null);
            
            Vector3 modelLandingPosition = landingBlock.transform.position + Vector3.up * VerticalOffset;
            
            StartModelJump(landingBlock);
            
        }
        else// spawn a clone, jump to an imaginary offscreen block and make the clone jump to the landing block from offscreen
        {
            //Debug.LogFormat("Didn't find block jumping {0} from {1},{2}", inputDir==1?"right":"left",CurrentBlock.row,CurrentBlock.col);

            Block landingBlock = GetCloneLandingBlock(inputDir,numOfBlocks);

            Vector3 cloneStartPosition = GetCloneStartingPosition(landingBlock,inputDir,numOfBlocks);
            
            debugClonePosition = cloneStartPosition;
            debugCloneRay = rayDir;
            
           
            Vector3 modelLandingPosition = GetModelLandingPosition(inputDir, numOfBlocks);
            StartModelFakeJump(modelLandingPosition);
            
            StartCloneJump(cloneStartPosition,landingBlock,jumpDir);

        }
    }
    
    [Header("Dance")]
    public DanceDATA danceData;
    [SerializeField] private STAGE currentStage;
    IEnumerator currentCoroutine;
    
    [Button]
    public void PlayDanceAnimation()
    {
        if(currentCoroutine==null)
        {
            currentCoroutine = PlayAnimationCoroutine();
            StartCoroutine(currentCoroutine);
        }
    }
    
    IEnumerator PlayAnimationCoroutine()
    {
        
        float moveDuration = 0.06f;
        float height = 0.2f * (isUpsideDown ? -1 : 1);
        float offset = 1f * (isUpsideDown ? -1 : 1);
        model.transform.position = CurrentBlock.transform.position + Vector3.up * VerticalOffset;
        float yPos = model.position.y+offset;
        int loops = 12; 

        Sequence danceSeq = DOTween.Sequence();
        danceSeq.Append(model.DOMoveY(yPos+height, moveDuration)).SetEase(Ease.Linear);
        danceSeq.Append(model.DOMoveY(yPos,moveDuration)).SetEase(Ease.Linear);
        danceSeq.SetLoops(loops,LoopType.Yoyo);
        danceSeq.Play();
        yield return danceSeq.WaitForCompletion();
        
        currentCoroutine = null;
        FindObjectOfType<TutorialManager>().TouchedBlackGem();
        FindObjectOfType<GameManager>().TouchedBlackGem();
    }
}
//ADD HEADER FOR DANCE AND MAKE THE VARIABLES PUBLIC SO THAT THEY CAN BE CHANGED IN THE INSPECTOR
[Serializable]
public  class DanceDATA
{
    public List<STAGE> stages;
    public STAGE CurrentStage { get; }
    public int currentStageIndex;
    public int currentDanceMoveIndex;
    public int currentLoopIndex;
    public int StageLoopCount;
    public int DanceMoveCount;
    public int TotalDuration;
    private PlayerController player;
    public void Initialize(PlayerController playerController)
    {
        this.player = playerController;
        currentStageIndex = 0;
        currentDanceMoveIndex = 0;
        currentLoopIndex = 0;
        StageLoopCount = 0;
        DanceMoveCount = 0;
        TotalDuration = 0;
        
        foreach (var stage in stages)
        {
            TotalDuration += (int)stage.GetStageDuration();
            DanceMoveCount += stage.DanceMoves.Count;
            StageLoopCount += stage.loopType;
        }
    }
    
    public DanceMove? GetCurrentMove()
    {
        if (currentStageIndex < stages.Count)
        {
            return stages[currentStageIndex].GetCurrentMove();
        }
        else
        {
            return null;
        }
    }
    
    public void NextStage()
    {
        currentStageIndex++;
        currentDanceMoveIndex = 0;
        currentLoopIndex = 0;
    }
    
    public DanceMove NextDanceMove()
    {
        var nextMove = stages[currentStageIndex].GetNextMove();
        if(nextMove==null)
        {
            NextStage();
            nextMove = GetCurrentMove();
        }

        return nextMove;
    }
    
    public Vector3 GetCurrentPosition()
    {
        Vector3 currentPosition = stages[currentStageIndex].GetCurrentPosition();
        return currentPosition;
    }
    
    */
}

[Serializable]
public class JumpEvent : UnityEvent<int, int>
{
    
}




