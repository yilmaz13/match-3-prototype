using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/LevelScriptableObject", order = 1)]
public class Level : ScriptableObject
{
    public int id;
    public int width;
    public int height;
    public int limit;
    public List<int> noSpawnerColumns = new List<int>();
    [SerializeField] public List<BlockTile> tiles = new List<BlockTile>();
    [SerializeField] public List<Goal> goals = new List<Goal>();
    public List<BlockType> availableTypes = new List<BlockType>();
    
    public void CopyLevel(Level level, Level level2)
    {
        level.height = level2.height;
        level.width = level2.height;
        level.id = level2.id;
        level.limit = level2.limit;
        level.availableTypes.AddRange(level2.availableTypes);

        foreach (var goal in level2.goals)
        {
            var goalTarget = new Goal()
            {
                amount = goal.amount,
                blockType = goal.blockType
            };
            level.goals.Add(goalTarget);
        }

        level.tiles.AddRange(level2.tiles);
        level.noSpawnerColumns.AddRange(level2.noSpawnerColumns);
    }
}