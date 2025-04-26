using System.Collections.Generic;


public class Level
{
    public int id;
    public int width;
    public int height;
    public List<LevelTile> tiles = new List<LevelTile>();
    public List<ColorBlockType> availableColors = new List<ColorBlockType>();
}
