using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GemController : MonoBehaviour
{
    public MeshRenderer gemMesh;
    private bool isUpsideDown = false;
    private Sequence floatSequence;
    private void Awake()
    {
       // gemMesh.enabled = false;
       FallFromSky();
    }

    void PlayFloatSequence()
    {
        if (floatSequence != null)
            floatSequence.Kill();
        float duration = 2f;
        float initialYPos = gemMesh.transform.position.y;
        float verticalOffset = initialYPos +0.1f* (isUpsideDown ? -1f : 1f);

        floatSequence = DOTween.Sequence();

        floatSequence.Append(gemMesh.transform.DOLocalRotate(new Vector3(0, 0, 180f), duration/2f,RotateMode.LocalAxisAdd));
        floatSequence.Join(gemMesh.transform.DOMoveY(verticalOffset, duration / 2f));
        floatSequence.Append(gemMesh.transform.DOLocalRotate(new Vector3(0, 0, 180f), duration/2f,RotateMode.LocalAxisAdd));
        floatSequence.Join(gemMesh.transform.DOMoveY(initialYPos, duration / 2f));

        floatSequence.SetLoops(-1,LoopType.Restart);
        floatSequence.SetEase(Ease.Linear);
        floatSequence.Play();
        
    }

    private void Flip(bool isUpsideDown)
    {
        this.isUpsideDown = isUpsideDown;
        floatSequence.Kill();
        if (!isUpsideDown)
        {
            gemMesh.transform.localRotation = Quaternion.Euler(105f,0f,0f);
            gemMesh.transform.localPosition = new Vector3(0, 0, -0.4f);
        }
        else
        {
            gemMesh.transform.localRotation = Quaternion.Euler(-0.862f,0f,0f);
            gemMesh.transform.localPosition = new Vector3(0f, 0f, -0.58f);
        }
        PlayFloatSequence();
    }


    void FallFromSky()
    {
        float duration = 0.2f;
        var pos = gemMesh.transform.localPosition;
        gemMesh.transform.localPosition = gemMesh.transform.localPosition + Vector3.up * 10f;

        gemMesh.transform.DOLocalMove(pos, duration).OnComplete(() =>
        {
            FindObjectOfType<ProceduralGenerator>().OnGemAnimationFinished();
            PlayFloatSequence();
        });
    }
}
