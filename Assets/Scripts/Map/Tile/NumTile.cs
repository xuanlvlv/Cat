using UnityEngine;
public class NumTile : TileEntity
{
    public BlockType type;
    public int num;

    private void OnDestroy()
    {
        //if (SoundManager.instance != null)
        //{
        //    SoundManager.instance.PlaySound("CubePress");
        //}
    }
}
