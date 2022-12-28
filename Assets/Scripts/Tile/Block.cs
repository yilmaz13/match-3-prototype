public enum BlockType
{
    Block1,
    Block2,
    Block3,
    Block4,
    RandomBlock,
    Empty
}

public class Block : TileEntity
{
    public BlockType type;
}