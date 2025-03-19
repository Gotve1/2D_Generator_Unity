using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a single chunk of terrain in the world
/// </summary>
public class TerrainChunk
{
    // Position of the chunk in chunk coordinates
    public Vector2Int position { get; private set; }
    
    // Size of the chunk (width and height)
    public int size { get; private set; }
    
    // GameObject that contains all blocks in this chunk
    public GameObject chunkObject { get; private set; }
    
    // List of all blocks in this chunk
    public List<GameObject> blocks { get; private set; }

    /// <summary>
    /// Constructor for creating a new terrain chunk
    /// </summary>
    public TerrainChunk(Vector2Int position, int size)
    {
        this.position = position;
        this.size = size;
        this.blocks = new List<GameObject>();
        
        // Create the chunk container object
        this.chunkObject = new GameObject($"Chunk_{position.x}_{position.y}");
        this.chunkObject.transform.position = new Vector3(
            position.x * size,
            position.y * size,
            0
        );
    }

    /// <summary>
    /// Gets a block at the specified local position within the chunk
    /// </summary>
    public GameObject GetBlock(int x, int y)
    {
        if (x < 0 || x >= size || y < 0 || y >= size)
        {
            return null;
        }

        int index = y * size + x;
        return index < blocks.Count ? blocks[index] : null;
    }

    /// <summary>
    /// Sets a block at the specified local position within the chunk
    /// </summary>
    public void SetBlock(int x, int y, GameObject block)
    {
        if (x < 0 || x >= size || y < 0 || y >= size)
        {
            return;
        }

        int index = y * size + x;
        if (index < blocks.Count)
        {
            blocks[index] = block;
        }
    }
} 