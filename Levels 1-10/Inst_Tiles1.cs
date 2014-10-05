﻿using UnityEngine;
using System.Collections;
using Pathfinding;

public class Inst_Tiles1 : MonoBehaviour {
	
	//public GameObject Ore_Fields;
	public GameObject water;
	public GameObject tree;
	int[,] ground_tiles = new int[,] { 
		{0,2,0,0,0, 0,0,0,0,0, 0,2,0,0,0, 0,2,0,0,0, 0,0,0,2,0, 2,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},  // 0 - Empty, 0 - Water, 2 - Castle, 3 - Gold_mine
		{0,0,0,0,0, 0,2,0,0,2, 0,0,2,0,2, 0,0,2,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0}, // 0 - Farm, 5 - Barracks
		{0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,0,0,0,0, 0,0,2,0,0, 0,2,0,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,0,0,0,0, 0,0,0,0,0, 0,0,0,2,0, 0,0,0,0,2, 0,0,0,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		
		{0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 2,0,0,0,0, 0,0,0,0,0},
		{0,0,0,0,0, 0,0,0,0,2, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,2, 0,0,0,0,0},
		{0,0,0,0,0, 0,0,2,0,0, 0,0,0,2,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,0,0,0},
		{0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,6,0,0, 2,0,0,0,0, 0,0,0,0,0},
		{0,0,0,0,2, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,0,0,0},
		
		{2,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,0,2,0,0, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,2,0, 0,0,2,0,0, 0,0,0,0,0, 0,6,0,0,0, 0,6,0,0,0, 0,0,0,2,0, 0,2,0,0,0},
		{0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 2,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,0,0,0},
		{0,2,0,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,2,0,0,0, 0,0,0,0,0},
		{0,0,2,0,0, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,0,0,0,0},
		
		{0,0,0,0,0, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,2,0,0,0},
		{0,0,2,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,2,0,0,2, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,0,0,2,0, 0,0,2,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,0,2,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,2,0,0,0, 0,0,2,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,6,0, 0,2,0,0,0},
		{0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,2,0,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		
		{0,2,0,0,0, 0,2,0,0,0, 0,2,0,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{2,0,0,0,2, 0,0,2,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,2,0, 0,2,0,0,0, 0,0,0,2,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{0,2,0,0,0, 0,0,0,0,2, 0,0,0,0,2, 0,2,0,2,0, 0,2,0,0,0, 0,0,2,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0},
		{2,0,0,2,0, 0,2,0,0,0, 0,2,0,0,0, 0,0,0,0,0, 0,0,2,0,0, 0,2,0,0,0, 0,0,2,0,0, 0,0,0,2,0, 0,0,0,0,0, 0,2,0,0,0},
		{0,2,0,0,0, 0,0,0,2,0, 0,0,0,2,0, 0,0,2,0,0, 0,0,0,0,2, 0,0,0,2,0, 0,0,0,0,0, 0,0,2,0,0, 0,2,0,2,0, 0,0,0,0,0} };
		
	//	private float castle_offset = 10.0f;	
	
	// Use this for initialization
	void Start () {
		//	float ter_Width = Terrain.activeTerrain.terrainData.size.x;
		//	float ter_Height =  Terrain.activeTerrain.terrainData.size.z;
		
		for(int i = 0; i < 25; ++i)
		{
			for(int j = 0; j < 50; ++j)
			{
				if(ground_tiles[i,j] == 0)
				{
					
				}
				else if(ground_tiles[i,j] == 1)
				{
					//Vector3 pos = new Vector3((-12 * j) + 5, 0.1f, (ter_Height - (250 * i) - 30f) / 4.0f);
					Vector3 pos = new Vector3(10 * j + 5, 0.1f, 250 - (10 * i)-5f);
					GameObject obj = (GameObject)Instantiate(water, pos, Quaternion.identity);
					Bounds b = obj.collider.bounds;
					GraphUpdateObject guo = new GraphUpdateObject(b);
					AstarPath.active.UpdateGraphs(guo);
				}
				else if(ground_tiles[i,j] == 2)
				{
					Vector3 pos = new Vector3(10 * j + 5, 0.1f, 250 - (12 * i)-5f);
					float treeHeight = Random.Range(5f,15f);
					tree.transform.localScale = new Vector3(treeHeight,treeHeight,treeHeight);
					GameObject obj = (GameObject)Instantiate(tree, pos, Quaternion.Euler(-90,0,0));
					Bounds b = obj.collider.bounds;
					GraphUpdateObject guo = new GraphUpdateObject(b);
					AstarPath.active.UpdateGraphs(guo);
					
				}
				//else if(ground_tiles[i,j] == 3)
				//{
				//    Vector3 pos = new Vector3(10 * j + 5, 0.1f, ter_Height - (10 * i)-5f);
				//    Instantiate(goldmine, pos, Quaternion.identity);
				//}
				//else if(ground_tiles[i,j] == 4)
				//{
				//    Vector3 pos = new Vector3(10 * j + 5, 0.1f, ter_Height - (10 * i)-5f);
				//    Instantiate(farm, pos, Quaternion.identity);
				//}
				//else if(ground_tiles[i,j] == 5)
				//{
				//    Vector3 pos = new Vector3(10 * j + 5, 0.1f, ter_Height - (10 * i)-5f);
				//    Instantiate(barracks, pos, Quaternion.identity);
				//}
				else if(ground_tiles[i,j] == 6)
				{
					/*//Vector3 pos = new Vector3(10 * j + 5, 0.1f, (ter_Height - (250 * i) - 30f) / 4.0f);
					Vector3 pos = new Vector3(10 * j + 5, 0.1f, ter_Height - (10 * i)-5f);
					Instantiate(Ore_Fields, pos, Quaternion.identity); */
				}
				else
				{
					Debug.Log ("Invalid tile index");
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
