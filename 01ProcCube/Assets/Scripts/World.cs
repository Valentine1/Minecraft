using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour {

	public GameObject player;
    public Slider MinecraftLoading;
    public Button PlayButton;
    public Camera MainCam;

	public Material textureAtlas;
	public static int columnHeight = 4;
	public static int chunkSize = 8;
	public static int worldSize = 12;
	public static int radius = 4;
	public static Dictionary<string, Chunk> chunks;
    bool firstbuild = true;
    bool building = false;

	public static string BuildChunkName(Vector3 v)
	{
		return (int)v.x + "_" + 
			         (int)v.y + "_" + 
			         (int)v.z;
	}

	IEnumerator BuildChunkColumn()
	{
		for(int i = 0; i < columnHeight; i++)
		{
			Vector3 chunkPosition = new Vector3(this.transform.position.x, 
												i*chunkSize, 
												this.transform.position.z);
			Chunk c = new Chunk(chunkPosition, textureAtlas);
			c.chunk.transform.parent = this.transform;
			chunks.Add(c.chunk.name, c);
		}

		foreach(KeyValuePair<string, Chunk> c in chunks)
		{
			c.Value.DrawChunk();
		
		}
        yield return null;
	}

    public IEnumerator BuildWorld()
    {
        building = true;

        int posx = (int)Mathf.Floor(player.transform.position.x / chunkSize);
        int posz = (int)Mathf.Floor(player.transform.position.z / chunkSize);
        float ChunksCount = Mathf.Pow((radius * 2 + 1),2) * columnHeight;
        int ChunksDrawnCount = 0;

        for (int z = -radius; z <= radius; z++)
            for (int x = -radius; x <= radius; x++)
                for (int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3((x + posx) * chunkSize,
                                                        y * chunkSize,
                                                        (posz + z) * chunkSize);
                    Chunk c;
                    string n = BuildChunkName(chunkPosition);
                    if (chunks.TryGetValue(n, out c))
                    {
                        c.status = ChunkStatus.KEEP;
                        break;
                    }
                    else
                    {
                        c = new Chunk(chunkPosition, textureAtlas);
                        c.chunk.transform.parent = this.transform;
                        chunks.Add(c.chunk.name, c);
                    }
              
                }

        foreach (KeyValuePair<string, Chunk> c in chunks)
        {

            if (c.Value.status == ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
                c.Value.status = ChunkStatus.KEEP;
            }

            //delete old chunks here

            c.Value.status = ChunkStatus.DONE;
            if (firstbuild)
            {
                MinecraftLoading.value = ChunksDrawnCount++ * 100 / ChunksCount;
            }
            yield return null;
        }
        if (firstbuild)
        {
            player.SetActive(true);
            PlayButton.gameObject.SetActive(false);
            MinecraftLoading.gameObject.SetActive(false);
            firstbuild = false;
        }
        building = false;
    }

	// Use this for initialization
	void Start () {
		player.SetActive(false);
		chunks = new Dictionary<string, Chunk>();
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
      
		
	}
    void Update()
    {
        if (!building && !firstbuild)
            StartCoroutine(BuildWorld());
    }

    public void StartMinecraft_Click()
    {
        StartCoroutine(BuildWorld());
    }
}
