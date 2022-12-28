using System.Collections.Generic;

namespace MatchSystem
{
    public abstract class MatchDetector
    {
        #region Protected Method

        protected GameGrid GameGrid;

        #endregion

        #region Public Method

        public void SetGrid(GameGrid grid)
        {
            GameGrid = grid;
        }

        #endregion

        #region Public Abstract Method

        public abstract List<Match> DetectMatches();
        public abstract bool HasMatch(int x, int y);

        #endregion
    }
}