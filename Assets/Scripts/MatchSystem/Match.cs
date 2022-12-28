using System.Collections.Generic;

public enum MatchType
{
    Horizontal,
    Vertical
}

public class Match
{
    public MatchType Type;

    public List<TileEntity> TilesEntities = new List<TileEntity>();

    public void AddTileEntity(TileEntity tileEntity)
    {
        TilesEntities.Add(tileEntity);
    }
}