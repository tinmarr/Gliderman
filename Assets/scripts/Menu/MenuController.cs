using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour, ISerializationCallbackReceiver, IPointerClickHandler
{
    public int currentIndex = -1;
    public bool pressed = false;
    [SerializeField] private Vector2Int currentPos = Vector2Int.zero;
    [SerializeField] private int gridWidth = 0;
    [SerializeField] private int gridHeight = 0;

    private int[,] layout;
    [SerializeField] private List<Package> buttonLayout;

    [SerializeField] private PlayerInput input;
    
    [System.Serializable]
    struct Package
    {
        public int[] row;

        public Package(int[] row)
        {
            this.row = row;
        }
    }

    public void OnBeforeSerialize()
    {
        buttonLayout = new List<Package>();
        for (int i = 0; i < gridHeight; i++)
        {
            int[] row = new int[gridWidth];
            for (int j = 0; j < gridWidth; j++)
            {
                row[j] = layout[i, j];
            }
            buttonLayout.Add(new Package(row));
        }
    }
    public void OnAfterDeserialize()
    {
        layout = new int[gridHeight, gridWidth];
        for (int i=0; i<gridHeight; i++)
        {
            for (int j=0; j<buttonLayout[i].row.Length; j++)
            {
                layout[i, j] = buttonLayout[i].row[j];
            }
        }
    }

    private void Start()
    {
        currentIndex = GetIndex();
        input.actions["Up"].performed += _ => UpdateIndex("up");
        input.actions["Down"].performed += _ => UpdateIndex("down");
        input.actions["Left"].performed += _ => UpdateIndex("left");
        input.actions["Right"].performed += _ => UpdateIndex("right");
        input.actions["Submit"].performed += _ => pressed = true;
    }

    void UpdateIndex(string dir)
    {
        switch (dir)
        {
            case "up": currentPos.y--; break;
            case "down": currentPos.y++; break;
            case "left": currentPos.x--; break;
            case "right": currentPos.x++; break;
            default: break;
        }

        int nextIndex = GetIndex();

        if (nextIndex == -1)
        {
            UpdateIndex(dir);
        }
        else
        {
            currentIndex = nextIndex;
            return;
        }
    }

    int GetIndex()
    {
        if (currentPos.x < 0)
        {
            currentPos.x = gridWidth - 1;
        } else if (currentPos.x >= gridWidth)
        {
            currentPos.x = 0;
        }

        if (currentPos.y < 0)
        {
            currentPos.y = gridHeight - 1;
        }
        else if (currentPos.y >= gridHeight)
        {
            currentPos.y = 0;
        }

        return layout[currentPos.y, currentPos.x];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        pressed = true;
    }
}
