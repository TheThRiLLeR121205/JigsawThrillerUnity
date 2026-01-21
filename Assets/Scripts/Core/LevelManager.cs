using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text levelText;

    [Header("Puzzle Pieces")]
    public List<PuzzlePiece> puzzlePieces;   // 9 PuzzlePiece prefabs
    public Transform puzzleBoard;            // Empty parent object for pieces
    public float shuffleRadius = 3f;         // How far pieces spawn from board center

    [Header("Level Images (Texture2D)")]
    public List<Texture2D> levelImages;      // Original rectangle textures

    [Header("Board & UI")]
    public float boardSize = 5f;             // Size of the board in Unity units
    public GameObject levelCompletePanel;

    private int currentLevel = 0;
    private int piecesPlaced = 0;

    void Start()
    {
        StartLevel();
    }

    // ------------------------
    // Initialize Level
    // ------------------------
    public void StartLevel()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + (currentLevel + 1);
        }

        if (levelImages == null || levelImages.Count == 0)
        {
            Debug.LogError("No level images assigned!");
            return;
        }

        piecesPlaced = 0;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        Texture2D fullTexture = levelImages[currentLevel];

        // Make sure texture is readable
        if (!fullTexture.isReadable)
        {
            Debug.LogError(fullTexture.name + " is not readable! Check import settings (Read/Write Enabled).");
            return;
        }

        Texture2D squareTexture = CropSquare(fullTexture);

        List<Sprite> slicedSprites = SliceTexture(squareTexture, 3, 3);

        int count = Mathf.Min(puzzlePieces.Count, slicedSprites.Count);
        float pieceSize = boardSize / 3f;

        for (int i = 0; i < count; i++)
        {
            // Assign new sprite
            puzzlePieces[i].GetComponent<SpriteRenderer>().sprite = slicedSprites[i];
            puzzlePieces[i].isPlaced = false;

            // Resize piece to fit board
            puzzlePieces[i].transform.localScale = new Vector3(pieceSize, pieceSize, 1f);

            // ------------------------
            // Shuffle pieces completely outside the board
            // ------------------------
            float spawnMargin = 0.5f; // extra distance outside board
            Vector2 halfBoard = new Vector2(boardSize / 2f, boardSize / 2f);
            Vector2 halfPiece = new Vector2(pieceSize / 2f, pieceSize / 2f);

            Vector3 randomPos;

            if (Random.value < 0.5f) // horizontal sides
            {
                float x = Random.value < 0.5f
                    ? puzzleBoard.position.x - halfBoard.x - halfPiece.x - spawnMargin // left
                    : puzzleBoard.position.x + halfBoard.x + halfPiece.x + spawnMargin; // right
                float y = Random.Range(
                    puzzleBoard.position.y - halfBoard.y + halfPiece.y,
                    puzzleBoard.position.y + halfBoard.y - halfPiece.y
                );
                randomPos = new Vector3(x, y, 0f);
            }
            else // vertical sides
            {
                float y = Random.value < 0.5f
                    ? puzzleBoard.position.y - halfBoard.y - halfPiece.y - spawnMargin // bottom
                    : puzzleBoard.position.y + halfBoard.y + halfPiece.y + spawnMargin; // top
                float x = Random.Range(
                    puzzleBoard.position.x - halfBoard.x + halfPiece.x,
                    puzzleBoard.position.x + halfBoard.x - halfPiece.x
                );
                randomPos = new Vector3(x, y, 0f);
            }

            puzzlePieces[i].transform.position = randomPos;

            puzzlePieces[i].gameObject.SetActive(true);
        }

        Debug.Log("Level " + currentLevel + " started with " + count + " pieces.");
    }

    // ------------------------
    // Crop rectangle to square
    // ------------------------
    Texture2D CropSquare(Texture2D original)
    {
        int size = Mathf.Min(original.width, original.height);
        int xOffset = (original.width - size) / 2;
        int yOffset = (original.height - size) / 2;

        Color[] pixels = original.GetPixels(xOffset, yOffset, size, size);
        Texture2D cropped = new Texture2D(size, size, original.format, false);
        cropped.SetPixels(pixels);
        cropped.Apply();
        return cropped;
    }

    // ------------------------
    // Slice Texture2D into NxN Sprites
    // ------------------------
    List<Sprite> SliceTexture(Texture2D texture, int cols, int rows)
    {
        List<Sprite> sprites = new List<Sprite>();
        int pieceWidth = texture.width / cols;
        int pieceHeight = texture.height / rows;

        for (int y = rows - 1; y >= 0; y--) // start from top row
        {
            for (int x = 0; x < cols; x++)
            {
                Color[] pixels = texture.GetPixels(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight);
                Texture2D pieceTex = new Texture2D(pieceWidth, pieceHeight, texture.format, false);
                pieceTex.SetPixels(pixels);
                pieceTex.Apply();

                Sprite pieceSprite = Sprite.Create(pieceTex,
                    new Rect(0, 0, pieceWidth, pieceHeight),
                    new Vector2(0.5f, 0.5f),
                    pieceWidth);

                sprites.Add(pieceSprite);
            }
        }

        return sprites;
    }

    // ------------------------
    // Called by PuzzlePiece when placed correctly
    // ------------------------
    public void PiecePlaced()
    {
        piecesPlaced++;
        if (piecesPlaced >= puzzlePieces.Count)
        {
            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(true);
        }
    }

    // ------------------------
    // Retry current level
    // ------------------------
    public void RetryLevel()
    {
        Debug.Log("Retry pressed");
        StartLevel();
    }

    // ------------------------
    // Go to next level
    // ------------------------
    public void NextLevel()
    {
        Debug.Log("Next Level pressed");
        currentLevel++;
        if (currentLevel >= levelImages.Count)
            currentLevel = 0; // wrap around
        StartLevel();
    }
}
