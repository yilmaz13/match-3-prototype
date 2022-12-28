using System.Collections.Generic;

namespace MatchSystem
{
    public class HorizontalMatchDetector : MatchDetector
    {
        #region Public Method
        public override List<Match> DetectMatches()
        {
            var matches = new List<Match>();

            for (var j = 0; j < GameGrid.level.height; j++)
            {
                for (var i = 0; i < GameGrid.level.width - 2;)
                {
                    var tile = GameGrid.GetTile(i, j)?.GetComponent<TileEntity>();
                    if (tile != null)
                    {
                        var color = tile.blockType;
                        if (GameGrid.IsNullTileEntity(i + 1, j) && GameGrid.IsSameBlock(i + 1, j, color) &&
                            GameGrid.IsNullTileEntity(i + 2, j) && GameGrid.IsSameBlock(i + 2, j, color))
                        {
                            var match = new Match();
                            match.Type = MatchType.Horizontal;
                            do
                            {
                                //  match.AddTile(board.GetTile(i, j));
                                match.AddTileEntity(GameGrid.GetTileEntity(i, j));
                                i += 1;
                            } while (i < GameGrid.level.width && GameGrid.IsNullTileEntity(i, j) &&
                                     GameGrid.IsSameBlock(i, j, color));

                            matches.Add(match);
                            continue;
                        }
                    }

                    i += 1;
                }
            }

            return matches;
        }

        public override bool HasMatch(int x, int y)
        {
            var tile = GameGrid.GetTileEntity(x, y);
            if (tile != null)
            {
                var horzLen = 1;
                for (var i = x - 1;
                     i >= 0 && GameGrid.GetTileEntity(i, y) != null &&
                     GameGrid.GetTileEntity(i, y).blockType == tile.blockType;
                     i--, horzLen++)
                {
                }

                for (var i = x + 1;
                     i < GameGrid.level.width && GameGrid.GetTileEntity(i, y) != null &&
                     GameGrid.GetTileEntity(i, y).blockType == tile.blockType;
                     i++, horzLen++)
                {
                }

                if (horzLen >= 3) return true;
            }

            return false;
        }
    
        #endregion
    }
}