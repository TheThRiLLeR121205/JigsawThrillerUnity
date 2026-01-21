using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [Header("References")]
    public Transform correctSlot;
    public LevelManager levelManager;

    [Header("Settings")]
    public float snapDistance = 0.3f;

    [HideInInspector]
    public bool isPlaced = false;

    private bool isDragging = false;
    private Vector3 offset;

    void Update()
    {
        if (isPlaced)
            return;

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            transform.position = mousePos + offset;
        }
    }

    void OnMouseDown()
    {
        if (isPlaced)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        offset = transform.position - mousePos;
        isDragging = true;
    }

    void OnMouseUp()
    {
        if (isPlaced)
            return;

        isDragging = false;

        float distance = Vector2.Distance(transform.position, correctSlot.position);

        if (distance <= snapDistance)
        {
            transform.position = correctSlot.position;
            isPlaced = true;

            levelManager.PiecePlaced();
        }
    }
}
