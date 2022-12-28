using System.Collections.Generic;

namespace MatchSystem
{
    public class VerticalMatchDetector : MatchDetector
    {
        #region Public Method
        public override List<Match> DetectMatches()
        {
            var matches = new List<Match>();

            for (var i = 0; i < GameGrid.level.width; i++)
            {
                for (var j = 0; j < GameGrid.level.height - 2;)
                {
                    var tile = GameGrid.GetTile(i, j);
                    if (tile != null && tile.GetComponent<TileEntity>() != null)
                    {
                        var color = tile.GetComponent<TileEntity>().blockType;
                        if (GameGrid.IsNullTileEntity(i, j + 1) && GameGrid.IsSameBlock(i, j + 1, color) &&
                            GameGrid.IsNullTileEntity(i, j + 2) && GameGrid.IsSameBlock(i, j + 2, color))
                        {
                            var match = new Match();
                            match.Type = MatchType.Vertical;
                            do
                            {
                                //   match.AddTile(board.GetTile(i, j));
                                match.AddTileEntity(GameGrid.GetTileEntity(i, j));
                                j += 1;
                            } while (j < GameGrid.level.height && GameGrid.IsNullTileEntity(i, j) &&
                                     GameGrid.IsSameBlock(i, j, color));

                            matches.Add(match);
                            continue;
                        }
                    }

                    j += 1;
                }
            }

            return matches;
        }
    
        public override bool HasMatch(int x, int y)
        {
            var tile = GameGrid.GetTileEntity(x, y);
            if (tile != null)
            {
                var vertLen = 1;
                for (var j = y - 1;
                     j >= 0 && GameGrid.GetTileEntity(x, j) != null && GameGrid.GetTileEntity(x, j).blockType == tile.blockType;
                     j--, vertLen++)
                {
                }

                for (var j = y + 1;
                     j < GameGrid.level.height && GameGrid.GetTileEntity(x, j) != null && GameGrid.GetTileEntity(x, j).blockType == tile.blockType;
                     j++, vertLen++)
                {
                }

                if (vertLen >= 3) return true;
            }

            return false;
        }
        
        #endregion
    }
}