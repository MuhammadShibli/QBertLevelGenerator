using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    private Block block;
        
    void Start()
    {
        /*var grid = FindObjectOfType<BlockGridCreator>();
        
        if (grid == null)
        {
            var testGridController = FindObjectOfType<TestGridGenerator>();
            var testGrid  = testGridController.gridConfigs[0].grid;
            var sizeX = testGridController.gridSizeX;
            var sizeY = testGridController.gridSizeY;
            var block = testGrid[Mathf.FloorToInt(sizeY/ 2) * sizeX +  Mathf.FloorToInt(sizeX / 2)];
            transform.position = block.transform.position;
        }
        else
        {
            var block =grid.GetBlock(Mathf.FloorToInt(grid.gridSizeY/2),Mathf.FloorToInt(grid.gridSizeX/2));
            transform.position = block.transform.position;    
        }*/
        
    }
    

}
