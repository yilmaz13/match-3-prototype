using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.Events;
public enum AnimState
{
    Unpressed,
    Falling,
    Pressed
}

public class TileEntity : MonoBehaviour
{
    #region Variables
    [HideInInspector] public UnityEvent onSpawn;
    [HideInInspector] public UnityEvent onExplode;
    [HideInInspector] public BlockType blockType;
    public int x { get; set; }
    public int y { get; set; }
    public int gridHeight = 8; //TODO: set with level.Height
    public int gridWeight = 8; //TODO: set with level.Weight
    private int _findIndex;

    public int FindIndex
    {
        get => x + (y * gridHeight);
        set
        {
            _findIndex = value;
            y = _findIndex / gridHeight;
            x = _findIndex % gridWeight;
        }
    }

    #endregion
    
    #region Public Method
    public void SetCoordinate(int xCoordinate, int ySetCoordinate, int gridH, int gridW)
    {
        x = xCoordinate;
        y = ySetCoordinate;
        gridHeight = gridH;
        gridWeight = gridW;
        _findIndex = x + (y * gridH);
    }

    public void PlaySound(string sound)
    {
        SoundManager.instance.PlaySound(sound);
    }

    public virtual void OnEnable()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var newColor = spriteRenderer.color;
            newColor.a = 1.0f;
            spriteRenderer.color = newColor;
        }

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        transform.localRotation = Quaternion.identity;
    }

    public virtual void OnDisable()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var newColor = spriteRenderer.color;
            newColor.a = 1.0f;
            spriteRenderer.color = newColor;
        }

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        transform.localRotation = Quaternion.identity;
    }

    public void Spawn()
    {
        onSpawn.Invoke();
    }

    public void Explode()
    {
        onExplode.Invoke();
    }
    
    #endregion
}