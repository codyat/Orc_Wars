using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS{
	public static class ResourceManager{
		//HealthBar TExtures

		private static Texture2D healthyTexture, damagedTexture, criticalTexture;
		public static Texture2D HealthyTexture { get { return healthyTexture; } }
		public static Texture2D DamagedTexture { get { return damagedTexture; } }
		public static Texture2D CriticalTexture { get { return criticalTexture; } }
		private static Dictionary< ResourceType, Texture2D > resourceHealthBarTextures;

		//for moving the camera around and zoom in and out
		public static int ScrollWidth { get { return 15; } }
		public static float ScrollSpeed { get { return 25; } }
		public static float RotateAmount { get { return 10; } }
		public static float RotateSpeed { get { return 100; } }
		public static float MinCameraHeight { get { return 10; } }
		public static float MaxCameraHeight { get { return 80; } }
		public static float MinCameraX { get { return 0; } }
		public static float MaxCameraX { get { return 500; } }
		public static float MinCameraZ { get { return 0; } }
		public static float MaxCameraZ { get { return 250; } }
		
		// Invalid position (for clicking within the screen )
		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		public static Vector3 InvalidPosition { get { return invalidPosition; } }
		private static Bounds invalidBounds = new Bounds(new Vector3(-99999, -99999, -99999), new Vector3(0, 0, 0));
		public static Bounds InvalidBounds { get { return invalidBounds; } }

		// Selection Box GUI information
		private static GUISkin selectBoxSkin;
		public static GUISkin SelectBoxSkin { get { return selectBoxSkin; } }
		public static void StoreSelectBoxItems(GUISkin skin) {
			selectBoxSkin = skin;
		}

		//determines how fast the units are built in a building
		public static int BuildSpeed { get { return 2; } }
		private static GameObjectList gameObjectList;

		// Used to set the game object list
		public static void SetGameObjectList(GameObjectList objectList) {
			gameObjectList = objectList;
		}

		// Used in the Game Object List Wrappermethods
		public static GameObject GetBuilding(string name) {
			return gameObjectList.GetBuilding(name);
		}
		
		public static GameObject GetUnit(string name) {
//			Debug.Log ("FETCHING UNIT OF NAME: " + name);
			return gameObjectList.GetUnit(name);
		}
		
		public static GameObject GetWorldObject(string name) {
			return gameObjectList.GetWorldObject(name);
		}
		
		public static GameObject GetPlayerObject() {
			return gameObjectList.GetPlayerObject();
		}
		
		public static Texture2D GetBuildImage(string name) {
			return gameObjectList.GetBuildImage(name);
		}

		public static void StoreSelectBoxItems(GUISkin skin, Texture2D healthy, Texture2D damaged, Texture2D critical) {
			selectBoxSkin = skin;
			healthyTexture = healthy;
			damagedTexture = damaged;
			criticalTexture = critical;
		}

		public static void SetResourceHealthBarTextures(Dictionary<ResourceType, Texture2D> images) {
			resourceHealthBarTextures = images;
		}
		
		public static Texture2D GetResourceHealthBar(ResourceType resourceType) {
			if(resourceHealthBarTextures != null && resourceHealthBarTextures.ContainsKey(resourceType)) return resourceHealthBarTextures[resourceType];
			return null;
		}

		public static int O, S, B, D;
		public static float timer = 601f;
		public static int previousLevel;

		private static string[,] objectives = {  {"Select a Building", "Create an Orc"}, 
			{"Attack an Enemy Troop", "Defeat an Enemy Troop"},
			{"Create 5 Orcs", "Create 1 Blue Goblin"}, 
			{"Destroy Enemy's Gold Mine", "Get a Troop to Level 2"},                                      
			{"Create 5 Blue Goblins", "Create 1 Skull Warrior"},
			{"Destroy Enemy's Barracks", "Get a Troop to Level 3"},
			{"Create 5 Skull Warriors", "Create 1 Demon"}, 
			{"Destroy the Enemy's Castle", "Win in 10 Minutes"} };
		
		private static bool[,] completions = {  {false, false},
			{false, false},
			{false, false},
			{false, false},
			{false, false},
			{false, false},
			{false, false},
			{false, false} };
		
		public static string GetObjective(int level, int n) {
			if (level < 8 && n < 2) return objectives [level, n];
			else return "";
		}
		
		public static bool GetCompletions(int level, int n) {
			if (level < 8 && n < 2) {
//								Debug.Log ("LEVEL IS: " + level + " N : " + n);
								return completions [level, n];
						}
			else return false;
		}
		
		public static void SetCompletions(int level, int n) {
			if (level < 8 && n < 2) completions [level, n] = true;
		}
		
		public static void ResetCompletions(int level, int n) {
			if (level < 8 && n < 2) completions [level, n] = false;
		}
		
		public static void ResetAll() {
			completions = new bool[,] {{false, false},
				{false, false},
				{false, false},
				{false, false},
				{false, false},
				{false, false},
				{false, false}, 
				{false, false}};
		}
	}
}