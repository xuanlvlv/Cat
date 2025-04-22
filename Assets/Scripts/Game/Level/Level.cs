using System.Collections.Generic;


/// <summary>
/// �洢��Ϸ�ؿ�������
/// </summary>
public class Level
{
    public int id;
    public int width;
    public int height;
    public List<LevelTile> tiles = new List<LevelTile>();
    public List<ColorBlockType> availableColors = new List<ColorBlockType>();
}
