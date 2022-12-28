using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Manager;
using PoolSystem;
using UnityEngine;
using Utilities;

namespace MatchSystem
{
    public class GameGrid : Singleton<GameGrid>
    {
        #region Variables

        public GamePools gamePools;
        public float blockFallSpeed = 0.3f;
        public float horizontalSpacing;
        public float verticalSpacing;
        public Transform levelLocation;
        public Sprite backgroundSprite;
        public Color backgroundColor;
        public Level level;

        [HideInInspector] public List<GameObject> tileEntities = new List<GameObject>();
        [HideInInspector] public List<Vector2> tilePositions = new List<Vector2>();

        private float _blockWidth;
        private float _blockHeight;
        private float tileWidth;
        private float tileHeight;

        private bool drag;
        private GameObject selectedTile;
        private TileEntity selectedTileEntity;
        private Camera mainCamera;
        private bool currentlySwapping;
        private bool inputLocked;

        private MatchDetector horizontalMatchDetector = new HorizontalMatchDetector();
        private MatchDetector verticalMatchDetector = new VerticalMatchDetector();
        private List<Match> _matches = new List<Match>();
        private HashSet<int> destroyColumns = new HashSet<int>();

        #endregion

        #region Unity Methods

        public void HandleInput()
        {
            if (inputLocked)
                return;

            if (currentlySwapping)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                drag = true;
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Block"))
                    selectedTile = hit.collider.gameObject;
            }

            if (Input.GetMouseButtonUp(0))
            {
                drag = false;
            }

            if (!drag || selectedTile == null) return;
            {
                var hit = Physics2D.Raycast(Camera.main!.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider == null || hit.collider.gameObject == selectedTile) return;

                selectedTileEntity = selectedTile.GetComponent<TileEntity>();
                var newSelectedTileEntity = hit.collider.gameObject.GetComponent<TileEntity>();

                if (Utility.NoTileNeighbor(selectedTileEntity.x, selectedTileEntity.y, newSelectedTileEntity.x,
                        newSelectedTileEntity.y))
                    return;

                var selectedTileCopy = selectedTile;
                var newSelectedTileCopy = hit.collider.gameObject;

                if (DetectSwap(selectedTileEntity, newSelectedTileEntity))
                    SwapTiles(selectedTileEntity, newSelectedTileEntity);
                else
                    SwapTilesAndReSwapTiles(selectedTileCopy, newSelectedTileCopy);
            }
        }

        #endregion

        #region Swap and Fall Methods

        /// <summary>
        /// SwapTilesAndReSwapTiles DOTween anims func
        /// </summary>
        private void SwapTiles(TileEntity tileA, TileEntity tileB)
        {
            tileA.GetComponent<SpriteRenderer>().sortingOrder = 1;
            currentlySwapping = true;

            var tileAPos = tileA.transform.position;
            var tileBPos = tileB.transform.position;
            tileA.transform.DOMove(tileBPos, 0.25f).SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    currentlySwapping = false;
                    tileA.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    DestroyMatches();
                });
            tileB.transform.DOMove(tileAPos, 0.25f).SetEase(Ease.OutBounce);
            var idxA = tileA.FindIndex;
            var idxB = tileB.FindIndex;
            tileEntities[idxA] = tileB.gameObject;
            tileEntities[idxB] = tileA.gameObject;

            tileA.FindIndex = idxB;
            tileB.FindIndex = idxA;
            selectedTile = null;
            Move();
        }

        private void Move()
        {
            level.limit--;
            UIManager.Instance.SetMoveText(level.limit);
        }

        /// <summary>
        /// SwapTilesAndReSwapTiles DOTween anims func
        /// </summary>
        private void SwapTilesAndReSwapTiles(GameObject tileA, GameObject tileB)
        {
            currentlySwapping = true;

            var tileAPos = tileA.transform.position;
            var tileBPos = tileB.transform.position;

            tileA.GetComponent<SpriteRenderer>().sortingOrder = 1;
            tileA.transform.DOMove(tileBPos, 0.2f).SetEase(Ease.OutBounce);
            tileB.transform.DOMove(tileAPos, 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                tileA.transform.DOMove(tileAPos, 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    currentlySwapping = false;
                    tileA.GetComponent<SpriteRenderer>().sortingOrder = 0;
                });
                tileB.transform.DOMove(tileBPos, 0.2f).SetEase(Ease.OutBounce);
            });
            tileA.GetComponent<TileEntity>().PlaySound("PressError");
            selectedTile = null;
        }

        private IEnumerator CalculateFallsAsync(float delay = 0.0f)
        {
            inputLocked = true;
            yield return new WaitForSeconds(delay);
            CalculateFalls();
            yield return new WaitForSeconds(0.75f);
            if (!DestroyMatches())
            {
                inputLocked = false;
                GameManager.Instance.CheckEndGame();
            }
        }

        private int GetMinIndex(List<TileEntity> tiles)
        {
            var min = level.height * level.width;
            foreach (var tile in tiles)
                if (tile.FindIndex < min)
                    min = tile.FindIndex;

            return min;
        }

        private void FallTile(TileEntity tile, int destroyTile, int index, float fallTime)
        {
            if (tile == null) return;
            var numTilesToFall = destroyTile;
            tileEntities[index + (numTilesToFall * level.width)] = tileEntities[index];
            tile.transform.DOMove(tilePositions[index + level.width * numTilesToFall], fallTime).SetEase(Ease.OutBounce)
                .OnComplete(() => { tile.y += numTilesToFall; });

            tileEntities[index] = null;
            destroyColumns.Add(tile.x);
        }

        private void CalculateFalls()
        {
            foreach (var match in _matches)
            {
                var min = GetMinIndex(match.TilesEntities);

                if (min < level.height)
                    destroyColumns.Add(min);

                if (match.Type == MatchType.Vertical)
                {
                    var destroyTile = match.TilesEntities.Count;
                    for (var x = min; x >= 0;)
                    {
                        var tile = tileEntities[x]?.GetComponent<TileEntity>();
                        FallTile(tile, destroyTile, (x), blockFallSpeed);
                        x -= level.height;
                    }
                }
                else if (match.Type == MatchType.Horizontal)
                {
                    foreach (var tile in match.TilesEntities)
                    {
                        for (int x = tile.FindIndex; x >= 0; x -= level.height)
                        {
                            var childTile = tileEntities[x]?.GetComponent<TileEntity>();
                            FallTile(childTile, 1, (x), blockFallSpeed);
                        }
                    }
                }
            }

            foreach (var x in destroyColumns)
            {
                float y = 1;
                for (var j = level.height - 1; j >= 0; j--)
                {
                    var tileEntity = GetTileEntity(x, j);
                    if (tileEntity != null)
                        continue;
                    else
                    {
                        if (level.noSpawnerColumns.Any(noSpawnerColumn => x == noSpawnerColumn))
                        {
                            continue;
                        }

                        var tile = CreateTileEntity(x, j);
                        var sourcePos = new Vector3(tilePositions[x].x, tilePositions[x].y + (tileHeight * y), 0);
                        var targetPos = tilePositions[x + (j * level.height)];
                        tile.transform.position = sourcePos;
                        tile.transform.DOJump(targetPos, 0.3f, 1, blockFallSpeed).SetEase(Ease.OutBounce);
                        tileEntities[x + (j * level.height)] = tile.gameObject;
                        tile.GetComponent<SpriteRenderer>().sortingOrder = j;
                        y++;
                    }
                }
            }

            _matches = new List<Match>();
        }

        private void ExplodeTile(GameObject tile)
        {
            var idx = tileEntities.FindIndex(x => x == tile);
            if (idx != -1)
            {
                gamePools.GetParticles(tile.transform.position, tile.GetComponent<TileEntity>().blockType);
                tile.GetComponent<PoolObject>().GoToPool();
                var tileEntity = tile.GetComponent<TileEntity>().blockType;
                foreach (var goal in level.goals)
                {
                    if (goal.blockType == tileEntity)
                    {
                        if (goal.amount > 0)
                            goal.amount--;
                        else
                            goal.amount = 0;
                    }
                }

                tileEntities[idx] = null;
            }
        }

        #endregion

        #region Level Generate

        private void CreateBackgroundTiles()
        {
            var backgroundTiles = new GameObject("BackgroundTiles");
            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var tileIndex = i + (j * level.width);
                    var tile = level.tiles[tileIndex];
                    if (tile != null && tile.type == BlockType.Empty)
                    {
                        continue;
                    }

                    var go = new GameObject("Background")
                    {
                        transform =
                        {
                            parent = backgroundTiles.transform
                        }
                    };

                    var sprite = go.AddComponent<SpriteRenderer>();
                    sprite.sprite = backgroundSprite;
                    sprite.color = backgroundColor;
                    sprite.sortingLayerName = "Game";
                    sprite.sortingOrder = -2;
                    sprite.transform.position = tileEntities[tileIndex].transform.position;
                }
            }
        }

        private void ResetLevelData()
        {
            GetLevelData();

            UIManager.Instance.SetMoveText(level.limit);
            UIManager.Instance.SetLevelText(level.id);
            UIManager.Instance.SetGoalsUI(level.goals);

            ObjectPooler.Instance.AllGotoPool();

            tileEntities.Clear();
            tilePositions.Clear();

            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var blockTile = level.tiles[i + (j * level.height)];
                    var tile = CreateTile(i, j, blockTile.type);
                    var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                    var bounds = spriteRenderer.bounds;
                    _blockWidth = bounds.size.x;
                    _blockHeight = bounds.size.y;
                    Vector3 position = new Vector2(i * (_blockWidth + horizontalSpacing),
                        -j * (_blockHeight + verticalSpacing));
                    tileEntities.Add(tile);
                    spriteRenderer.sortingOrder = level.height - j;

                    var totalWidth = (level.width - 1) * (_blockWidth + horizontalSpacing);
                    var totalHeight = (level.height - 1) * (_blockHeight + verticalSpacing);

                    var newPos = position;
                    newPos.x -= totalWidth / 2;
                    newPos.y += totalHeight / 2;
                    newPos.y += levelLocation.position.y;
                    position = newPos;
                    tile.transform.position = position;
                    tilePositions.Add(newPos);
                }
            }

            horizontalMatchDetector.SetGrid(this);
            verticalMatchDetector.SetGrid(this);

            tileHeight = tilePositions[0].y - tilePositions[level.width].y;
            var totalWidth1 = (level.width - 1) * (_blockWidth + horizontalSpacing);
            var zoomLevel = 1.4f;
            mainCamera.orthographicSize = (totalWidth1 * zoomLevel) * (Screen.height / (float)Screen.width) * 0.5f;
        }

        private void GetLevelData()
        {
            level = ScriptableObject.CreateInstance<Level>();
            var levelId = PlayerPrefs.GetInt("level");
            var levelTemplate = Resources.Load<Level>("ScriptableObjects/Levels/" + "Level" + levelId);
            level.CopyLevel(level, levelTemplate);
        }

        private BlockType? GetBlockType(int x, int y)
        {
            var block = GetTile(x, y);
            if (block == null)
                return null;
            return block.GetComponent<TileEntity>().blockType;
        }

        #endregion

        #region Create Tile

        private TileEntity CreateTileEntity(int x, int y)
        {
            return CreateTile(x, y).GetComponent<TileEntity>();
        }

        private GameObject CreateTile(int x, int y, BlockType blockType = BlockType.RandomBlock)
        {
            var eligibleTiles = new List<BlockType>();
            eligibleTiles.AddRange(level.availableTypes);

            var leftTile1 = GetBlockType(x - 1, y);
            var leftTile2 = GetBlockType(x - 2, y);
            if (leftTile1 == leftTile2)
            {
                var tileToRemove = eligibleTiles.Find(t => t == leftTile1);
                eligibleTiles.Remove(tileToRemove);
            }

            var topTile1 = GetBlockType(x, y - 1);
            var topTile2 = GetBlockType(x, y - 2);
            if (topTile1 == topTile2)
            {
                var tileToRemove = eligibleTiles.Find(t => t == topTile1);
                eligibleTiles.Remove(tileToRemove);
            }

            var type = eligibleTiles[UnityEngine.Random.Range(0, eligibleTiles.Count)];
            if (blockType != BlockType.RandomBlock) type = blockType;
            var levelTile = new BlockTile { type = type };
            var tileToGet = gamePools.GetTileEntity(levelTile);
            var tile = CreateBlock(tileToGet.gameObject);
            tileToGet.blockType = type;
            tileToGet.SetCoordinate(x, y, level.height, level.height);
            return tile;
        }

        public GameObject GetTile(int x, int y)
        {
            if (x >= 0 && x < level.width && y >= 0 && y < level.height)
            {
                return tileEntities[x + (y * level.width)];
            }

            return null;
        }

        public TileEntity GetTileEntity(int x, int y)
        {
            return GetTile(x, y)?.GetComponent<TileEntity>();
        }

        #endregion

        #region Game State Check

        public void RestartLevel()
        {
            ResetLevelData();
        }

        public IEnumerator StartLevel()
        {
            yield return new WaitForSeconds(0f);
            mainCamera = Camera.main;
            ResetLevelData();
            CreateBackgroundTiles();
        }

        public void NextLevel()
        {
            ResetLevelData();
        }

        #endregion

        #region CreateBlock

        private static GameObject CreateBlock(GameObject go)
        {
            go.GetComponent<TileEntity>().Spawn();
            return go;
        }

        #endregion

        #region CheckMatch

        private bool DestroyMatches()
        {
            var matches = new List<Match>();
            var matchedEntities = new HashSet<TileEntity>();
            var horizontalMatches = horizontalMatchDetector.DetectMatches();
            var verticalMatches = verticalMatchDetector.DetectMatches();

            foreach (var match in verticalMatches)
            {
                for (var i = 0; i < match.TilesEntities.Count;)
                {
                    var tileEntity = match.TilesEntities[i];
                    if (!matchedEntities.Add(tileEntity))
                        match.TilesEntities.Remove(tileEntity);
                    else
                        i++;
                }
            }

            foreach (var match in horizontalMatches)
            {
                for (var i = 0; i < match.TilesEntities.Count;)
                {
                    var tileEntity = match.TilesEntities[i];
                    if (!matchedEntities.Add(tileEntity))
                        match.TilesEntities.Remove(tileEntity);
                    else
                        i++;
                }
            }

            matches.AddRange(verticalMatches);
            matches.AddRange(horizontalMatches);

            if (matches.Count <= 0) return false;
            {
                _matches.AddRange(matches);
                foreach (var match in matches)
                {
                    foreach (var tile in match.TilesEntities)
                    {
                        ExplodeTile(tile.gameObject);
                        SoundManager.instance.PlaySound("Match");
                    }
                }

                StartCoroutine(CalculateFallsAsync());
                return true;
            }
        }

        private bool HasMatch(int x, int y)
        {
            return horizontalMatchDetector.HasMatch(x, y) || verticalMatchDetector.HasMatch(x, y);
        }

        #endregion

        #region Queries

        private bool DetectSwap(TileEntity tileA, TileEntity titleB)
        {
            var tileAx = tileA.x;
            var tileAy = tileA.y;

            var tileBx = titleB.x;
            var tileBy = titleB.y;

            var hasMatch = false;
            {
                SetTileEntity(tileA, tileBx, tileBy);
                SetTileEntity(titleB, tileAx, tileAy);

                if (HasMatch(tileAx, tileAy) || HasMatch(tileBx, tileBy))
                {
                    hasMatch = true;
                }
                else
                {
                    hasMatch = false;
                }
            }

            SetTileEntity(tileA, tileAx, tileAy);
            SetTileEntity(titleB, tileBx, tileBy);

            return hasMatch;
        }

        public bool IsNullTileEntity(int x, int y)
        {
            return GetTileEntity(x, y) != null;
        }

        public bool IsSameBlock(int x, int y, TileEntity tile)
        {
            return GetTileEntity(x, y).blockType == tile.blockType;
        }

        public bool IsSameBlock(int x, int y, BlockType block)
        {
            return GetTileEntity(x, y).blockType == block;
        }

        private void SetTileEntity(TileEntity tile, int x, int y)
        {
            if (x >= 0 && x < level.width && y >= 0 && y < level.height)
            {
                tileEntities[x + (y * level.width)] = tile.gameObject;
            }
        }

        #endregion
    }
}