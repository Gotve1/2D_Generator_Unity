using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main class responsible for procedural terrain generation using Perlin noise
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    // Configuration parameters
    [Header("Terrain Settings")]
    [SerializeField] private int seed = 0;                    // Seed for random generation
    [SerializeField] private float noiseScale = 50f;          // Scale of the noise (larger = smoother terrain)
    [SerializeField] private float heightMultiplier = 1f;     // Height multiplier for terrain
    [SerializeField] private int octaves = 6;                 // Number of octaves for noise (more = more detail)
    [SerializeField] private float persistence = 0.6f;        // Persistence for noise (affects detail strength)
    [SerializeField] private float lacunarity = 2f;          // Lacunarity for noise (affects detail scale)
    [SerializeField] private float baseFrequency = 1f;       // Base frequency for noise
    [SerializeField] private float waterLevel = 0.4f;        // Water level threshold (0-1)

    [Header("Block Settings")]
    [SerializeField] private GameObject groundBlockPrefab;    // Ground block prefab
    [SerializeField] private GameObject waterBlockPrefab;     // Water block prefab
    [SerializeField] private float blockSize = 1f;           // Size of each block

    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 16;             // Size of each chunk (16x16)
    [SerializeField] private int viewDistance = 3;           // Number of chunks to generate in each direction

    // Dictionary to store generated chunks
    private Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();
    
    // Reference to the player transform for chunk loading
    private Transform playerTransform;

    private void Start()
    {
        // Initialize random seed if not set
        if (seed == 0)
        {
            seed = Random.Range(-10000, 10000);
        }

        // Find player transform
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure to tag your player object with 'Player'");
            return;
        }

        // Generate initial chunks around player
        GenerateChunksAroundPlayer();
    }

    private void Update()
    {
        // Check if player has moved enough to generate new chunks
        GenerateChunksAroundPlayer();
    }

    /// <summary>
    /// Generates chunks around the player based on view distance
    /// </summary>
    private void GenerateChunksAroundPlayer()
    {
        // Calculate current chunk position
        Vector2Int currentChunk = GetChunkPosition(playerTransform.position);

        // Generate chunks in view distance
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkPos = currentChunk + new Vector2Int(x, y);
                
                // Generate chunk if it doesn't exist
                if (!chunks.ContainsKey(chunkPos))
                {
                    GenerateChunk(chunkPos);
                }
            }
        }

        // Remove chunks that are too far
        RemoveDistantChunks(currentChunk);
    }

    /// <summary>
    /// Generates a single chunk at the specified position
    /// </summary>
    private void GenerateChunk(Vector2Int chunkPos)
    {
        // Create new chunk
        TerrainChunk chunk = new TerrainChunk(chunkPos, chunkSize);
        
        // Generate terrain data for the chunk
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Calculate world position
                Vector2 worldPos = new Vector2(
                    chunkPos.x * chunkSize + x,
                    chunkPos.y * chunkSize + y
                );

                // Generate height using Perlin noise
                float height = GenerateHeight(worldPos);

                // Determine block type based on height
                GameObject blockPrefab = height > waterLevel ? groundBlockPrefab : waterBlockPrefab;

                // Create block
                Vector3 blockPos = new Vector3(worldPos.x * blockSize, worldPos.y * blockSize, 0);
                GameObject block = Instantiate(blockPrefab, blockPos, Quaternion.identity);
                block.transform.parent = chunk.chunkObject.transform;
                chunk.blocks.Add(block);
            }
        }

        // Store chunk reference
        chunks.Add(chunkPos, chunk);
    }

    /// <summary>
    /// Generates height value using Perlin noise with multiple octaves
    /// </summary>
    private float GenerateHeight(Vector2 position)
    {
        float amplitude = 1f;
        float frequency = baseFrequency;
        float noiseHeight = 0f;
        float amplitudeSum = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float xCoord = (position.x + seed) / noiseScale * frequency;
            float yCoord = (position.y + seed) / noiseScale * frequency;
            float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;

            noiseHeight += perlinValue * amplitude;
            amplitudeSum += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Normalize and add some transformation to make it more Minecraft-like
        float normalizedHeight = (noiseHeight / amplitudeSum + 1) * 0.5f;
        return Mathf.Pow(normalizedHeight, 1.2f); // Slightly sharpen transitions
    }

    /// <summary>
    /// Removes chunks that are too far from the player
    /// </summary>
    private void RemoveDistantChunks(Vector2Int currentChunk)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in chunks)
        {
            if (Vector2Int.Distance(chunk.Key, currentChunk) > viewDistance + 1)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            if (chunks.TryGetValue(chunkPos, out TerrainChunk chunk))
            {
                Destroy(chunk.chunkObject);
                chunks.Remove(chunkPos);
            }
        }
    }

    /// <summary>
    /// Converts world position to chunk position
    /// </summary>
    private Vector2Int GetChunkPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / (chunkSize * blockSize)),
            Mathf.FloorToInt(worldPosition.y / (chunkSize * blockSize))
        );
    }
} 