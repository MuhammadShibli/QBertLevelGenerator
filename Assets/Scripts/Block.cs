using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Block:MonoBehaviour
{
    [HideInInspector]
    public int row { get; set; }
    [HideInInspector]
    public int col { get; set; }
    public bool isEmpty;
    public MeshRenderer[] blockRenderers;
    bool hasBlackGem = false;
    bool hasRedGem = false;
    private bool hasRedSphere = false;
    private bool hasBlackSphere = false;
    private RedGemController redGem;
    private bool hasPlayer = false;
    public GameObject fireFX;
    public bool HasPlayer {
        get
        {
            return hasPlayer;
        }
        set
        {
            hasPlayer = value;
        }
    }
    public LayerMask blockLayerMask;
    public GameObject upBlock;
    public GameObject downBlock;
    public GameObject blackTop;
    
    private void Awake()
    {
        if (!isEmpty)
        {
            FindObjectOfType<ProceduralGenerator>().onProceduralGenerationFinished.AddListener(PlaySettleFX);
        }
        else
        {
            blackTop.SetActive(false);
        }

    }

    private void onUpsideDownSwitched(bool isUpsideDown)
    {
        upBlock.SetActive(!isUpsideDown);
        downBlock.SetActive(isUpsideDown);
    }

   
    
    private void PlaySettleFX()
    {
        PlayFallFX();
    }

    void PlayAppearFX()
    {
        float duration = 0.2f;
        blockRenderers[0].material.DOFade(1, 0.15f).
            SetDelay(row * 0.1f + col * 0.05f).
            OnComplete(() =>
            {
                FindObjectOfType<ProceduralGenerator>().OnBlockAnimationFinished();
            });
    }

    void PlayFallFX()
    {
        float duration = 0.2f;
        var pos = transform.position;
        transform.position = transform.position + Vector3.up * 10f;
        
        Sequence settleSeq = DOTween.Sequence();
        settleSeq.PrependInterval((row+1) * 0.015f);
        settleSeq.Append(transform.DOMove(pos, duration));
        settleSeq.Append( transform.DOPunchPosition(Vector3.up*0.5f, 0.1f,20));
        settleSeq.OnComplete(() =>
        {
            FindObjectOfType<ProceduralGenerator>().OnBlockAnimationFinished();
        });
        settleSeq.Play();
    }

    public float GetAlpha()
    {
        return GetColor().a;
    }

    public Color GetColor()
    {
        return blockRenderers[0].material.color;
    }

    public void SetColor(Color c)
    {
        for (int i = 0; i < blockRenderers.Length; i++)
            blockRenderers[i].material.color = c;
    }

    public void SetAlpha(float a)
    {
        if (isEmpty)
            return;
        Color c = GetColor();
        c.a = a;
        SetColor(c);
    }

    public void AddBlackGem(Transform blackGem, float verticalOffset=0 )
    {
        blackGem.transform.position = this.transform.position + Vector3.up * verticalOffset;
        Debug.Log($"added black gem to {row},{col}");
        hasBlackGem = true;
    }

    public bool HasBlackGem()
    {
        return hasBlackGem;
    }

    public void AddRedGem(Transform redGem, float verticalOffset = 0)
    {
        redGem.transform.position = this.transform.position + Vector3.up * verticalOffset;
        this.redGem = redGem.GetComponent<RedGemController>();
        hasRedGem = true;
    }

    public RedGemController GetRedGem()
    {
        Assert.IsTrue(hasRedGem);
        Assert.IsNotNull(redGem);
        return redGem;
    }

    public bool HasRedGem()
    {
        return hasRedGem;
    }

    public RedGemController RemoveRedGem()
    {
        Assert.IsTrue(hasRedGem);
        Assert.IsNotNull(redGem);

        hasRedGem = false;
        return redGem;
    }

   

    public void AddRedSphere(Transform redSphere, float verticcalOffset =0)
    {
        redSphere.transform.position = this.transform.position + Vector3.up * verticcalOffset;
        hasRedSphere = true;
    }

    public bool HasRedSphere()
    {
        return hasRedSphere;
    }

    

    public void AddBlackSphere(Transform blackSphere, float verticalOffset = 0)
    {
        blackSphere.transform.position = this.transform.position + Vector3.up * verticalOffset;
        hasBlackSphere = true;
    }

    public bool HasBlackSphere()
    {
        return hasBlackSphere;
    }

    public void RemoveBlackSphere()
    {
        hasBlackSphere = false;
    }

    public Block GetImmediateNeighbour(int jumpDirection, bool isUpsideDown)
    {
        Ray ray = CreateRay(jumpDirection, isUpsideDown, transform.position);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit, 2f, blockLayerMask))
        {
            Block neighbour = raycastHit.collider.GetComponentInParent<Block>();
            return neighbour;
        }
        else
        {
            return null;
        }
    }
    
    public Block GetNeighbourWrapAround(int jumpDirection, bool isUpsideDown)
    {
        BlockGridCreator grid = FindObjectOfType<BlockGridCreator>();
        int nextRow, nextCol;
        if (row == 0 && isUpsideDown)
            nextRow = grid.gridSizeY - 1;
        else if (row == grid.gridSizeY - 1 && !isUpsideDown)
            nextRow = 0;
        else
            nextRow = isUpsideDown ? row - 1 : row + 1;
        

        if (col == 0 && jumpDirection == -1)
            nextCol = grid.gridSizeX - 1;
        else if (col == grid.gridSizeX - 1 && jumpDirection == 1)
            nextCol = 0;
        else
            nextCol = jumpDirection == 1 ? col + 1 : col - 1;

        return grid.GetBlock(nextRow, nextCol);
    }

    public Block GetAnyNeighbour(int jumpDirection, bool isUpsideDown)
    {
        var block = GetImmediateNeighbour(jumpDirection, isUpsideDown);
        if (block == null)
            block = GetNeighbourWrapAround(jumpDirection, isUpsideDown);
        return block;
    }

    Ray CreateRay(int jumpDirection, bool isUpsideDown, Vector3 origin)
    {
        Vector3 rayOrigin = default;
        Vector3 rayDir = default;
        
        if (isUpsideDown)
        {
            if (jumpDirection == 1)
            {
                rayOrigin = origin + ( Vector3.right + Vector3.back ) * 0.5f ;
            }
            else
            {
                rayOrigin =  origin + ( Vector3.left + Vector3.back ) * 0.5f ;
            }
            rayDir = Vector3.down;
        }
        else
        {
            rayOrigin = origin + Vector3.up*0.5f;
            if (jumpDirection == 1)
            {
                rayDir = Vector3.forward + Vector3.right ;
            }
            else
            {
                rayDir = Vector3.forward + Vector3.left;
            }
        }

        return new Ray(rayOrigin, rayDir);
    }
    public bool debugNeighbours = false;
    
    private void OnDrawGizmos()
    {
        if (debugNeighbours)
        {
            Gizmos.color = Color.yellow;
            var rightBlock = GetAnyNeighbour(1, false);

            if(rightBlock!=null)
                Gizmos.DrawWireSphere(rightBlock.transform.position,0.3f);
            
            var leftBlock = GetAnyNeighbour(-1, false);
            if(leftBlock!=null)
                Gizmos.DrawWireSphere(leftBlock.transform.position,0.3f);
            
            var downRight = GetAnyNeighbour(1, true);
            if(downRight!=null)
                Gizmos.DrawWireSphere(downRight.transform.position,0.3f);
            
            var downLeft = GetAnyNeighbour(-1, true);
            if(downLeft!=null)
                Gizmos.DrawWireSphere(downLeft.transform.position,0.3f);
        }
    }
}
