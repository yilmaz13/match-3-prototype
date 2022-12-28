using System.Collections;
using MatchSystem;
using UnityEngine;

namespace Manager
{
    public class GameManager : Singleton<GameManager>
    {
        #region Variables  
    
        private bool gameStarted;
        private bool gameFinished;
        private int nextLevel;
        public Level level;
        public GameGrid grid;

        #endregion
        
        #region Unity Method
        private void Start()
        {
            gameStarted = true;
            StartCoroutine(grid.StartLevel());
        }

        private void Update()
        {
            if (gameStarted)
            {
                grid.HandleInput();
            }
        }

        #endregion
        
        #region Public Method
        public IEnumerator RestartLevel(float delay = 0.5f)
        {
            UIManager.Instance.RemoveGoalsUI();
            gameStarted = true;
            gameFinished = false;
            yield return new WaitForSeconds(delay);
            grid.RestartLevel();
        }

        public IEnumerator NextLevel(float delay = 0.5f)
        {
            UIManager.Instance.RemoveGoalsUI();
            gameStarted = true;
            gameFinished = false;
            yield return new WaitForSeconds(delay);
            grid.NextLevel();
        }

        public void StartGame()
        {
            gameStarted = true;
        }

        private void EndGame()
        {
            gameFinished = true;
            gameStarted = false;
        }

        [ContextMenu("next")]
        public void next()
        {
            EndGame();
            UIManager.Instance.OpenPopup("WinPopUp");
            var levelId = PlayerPrefs.GetInt("level");
            if (levelId == 4)
            {
                levelId = 0;
                PlayerPrefs.SetInt("level", 0);
            }
            else
                PlayerPrefs.SetInt("level", levelId + 1);
        }

        public void CheckEndGame()
        {
            if (gameFinished)
            {
                return;
            }

            var goalsComplete = false;
            int goalsCheck = 0;
            foreach (var goal in grid.level.goals)
            {
                if (goal.amount == 0)
                {
                    goalsCheck++;
                }
            }

            if (goalsCheck == grid.level.goals.Count)
            {
                goalsComplete = true;
            }

            if (grid.level.limit == 0)
            {
                EndGame();
            }

            if (goalsComplete)
            {
                EndGame();
                UIManager.Instance.OpenPopup("WinPopUp");
                var levelId = PlayerPrefs.GetInt("level");
                if (levelId == 4)
                {
                    levelId = 0;
                    PlayerPrefs.SetInt("level", 0);
                }
                else
                    PlayerPrefs.SetInt("level", levelId + 1);
            }
            else
            {
                if (gameFinished)
                {
                    UIManager.Instance.OpenPopup("LossPopUp");
                }
            }
        }
        #endregion
    }
}