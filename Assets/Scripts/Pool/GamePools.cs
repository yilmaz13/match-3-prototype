using PoolSystem;
using UnityEngine;

public class GamePools : MonoBehaviour
{
    public TileEntity GetTileEntity(BlockTile tile)
    {
        var obj = ObjectPooler.Instance.Spawn(tile.type.ToString(), new Vector3());
        return obj.GetComponent<TileEntity>();
    }

    public GameObject GetParticles(Vector3 pos, BlockType tile)
    {
        var fx = ObjectPooler.Instance.Spawn("Exlope" + tile.ToString(), new Vector3());
        fx.transform.position = pos;
        return fx;
    }
}