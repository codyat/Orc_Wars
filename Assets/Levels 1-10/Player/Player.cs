using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour {
	public string username;
	public int startMoney, startMoneyLimit, Money;
	public bool human;
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public Material notAllowedMaterial, allowedMaterial;
	public Color teamColor;
	
	private Building tempBuilding;
	private Unit tempCreator;
	private bool findingPlacement = false;
	private Dictionary< ResourceType, int > resources, resourceLimits;
	public List<Unit> Units;
	public List<Building> buildings;

	public Swordsman swordsman;

	public GameObject pCastle;
	public GameObject eCastle;
	Castle playercastle;
	Castle enemycastle;

	GoldMine playergoldmine;

	//AI
	List<Unit> p1units = new List<Unit> ();
	List<Unit> p2units = new List<Unit> ();

	//Win Conditions
	private bool level3Timer = true;

	public const float COMP_GETS_NEW_UNITS = 30.0f;
	public const float FIND_ALL_UNITS = 0.5f;
	public const float CHECK_CASTLE_STATUS = 0.5f;
	
	// Use this for initialization
	void Start () {
		hud = GetComponentInChildren< HUD > ();
		AddStartResourceLimits();
		AddStartResources();
		Money = startMoney;
		if (Application.loadedLevel > 3) {
						playercastle = pCastle.GetComponent<Castle> ();
						enemycastle = eCastle.GetComponent<Castle> ();
				}
	}
	
	void Awake() {
		resources = InitResourceList();
		resourceLimits = InitResourceList();
		ResourceManager.O = 0;
		ResourceManager.B = 0;
		ResourceManager.S = 0;
		ResourceManager.D = 0;
		if(Application.loadedLevel > 3)
			findCastles ();
		if (Application.loadedLevel == 11)
			ResourceManager.timer = 601f;
		if (Application.loadedLevel == 0)
			ResourceManager.ResetAll ();
	}
	
	float timer_getnewunits = 0.0f;
	float timer_findallunits = Mathf.Infinity;
	float timer_checkcastlestatus = 0.0f;
	void Update () 
	{
		if (Application.loadedLevel > 3 )
	    {
			if (ResourceManager.timer >= 0.0f) ResourceManager.timer -= .5f * Time.deltaTime; 
			else level3Timer = false;

			if (Application.loadedLevel == 6 && !ResourceManager.GetCompletions (2, 0) && ResourceManager.O >= 5) ResourceManager.SetCompletions (2, 0);
			if (Application.loadedLevel == 8 && !ResourceManager.GetCompletions (4, 0) && ResourceManager.B >= 5) ResourceManager.SetCompletions (4, 0);
			if (Application.loadedLevel == 10 && !ResourceManager.GetCompletions (6, 0) && ResourceManager.S >= 5) ResourceManager.SetCompletions (6, 0);

			if(Application.loadedLevel == 11 && ResourceManager.GetCompletions(7,0) && level3Timer) ResourceManager.SetCompletions(7, 1);


			timer_checkcastlestatus += Time.deltaTime;
			if(timer_checkcastlestatus > CHECK_CASTLE_STATUS)
			{
				timer_checkcastlestatus = 0.0f;
				findCastles ();
				if (tag == "Player1" && playercastle.hitPoints <= 0)
					AutoFade.LoadLevel(1,1f,1f,Color.black);
				else if (ResourceManager.GetCompletions(Application.loadedLevel - 4, 0) && ResourceManager.GetCompletions(Application.loadedLevel - 4, 1)) {
					ResourceManager.previousLevel = Application.loadedLevel;
					AutoFade.LoadLevel(2,1f,1f,Color.black);
				}
			}
			
			if (human)
			{
				hud.SetResourceValues (resources, resourceLimits);
				if (findingPlacement) {
					tempBuilding.CalculateBounds ();
					if (CanPlaceBuilding ())
						tempBuilding.SetTransparentMaterial (allowedMaterial, false);
					else
						tempBuilding.SetTransparentMaterial (notAllowedMaterial, false);
				}
			}
			else
			{
				timer_getnewunits += Time.deltaTime;
				if(timer_getnewunits > COMP_GETS_NEW_UNITS)
				{
					timer_getnewunits = 0.0f;
					for(int i = 0; i < 1; ++i)
					{
						Barracks[] b = FindObjectsOfType<Barracks>();
						Barracks bar;
						if(b[0].IsOwnedBy(this)) 
							bar = b[0];
						else{
							if(b.Length > 1){
								bar = b[1];
							}else{
								bar = b[0];
							}

						}
						Vector3 pos = new Vector3(bar.transform.position.x, 0, bar.transform.position.z);
						AddUnit("Swordsman", pos, Quaternion.identity);
					}
				}
				
				timer_findallunits += Time.deltaTime;
				if (timer_findallunits > FIND_ALL_UNITS)
				{
					timer_findallunits = 0.0f;
					findAllUnits ();
					for (int i = 0; i < p2units.Count; ++i)
					{
						if(!p2units[i].getHasAssignment())
						{
							int choice = 1;
							
							if(p1units.Count == 0)
							{
								choice = 1; //no enemies, attack castle
							}
							else
							{
								choice = Random.Range(0, 4);
							}
							if(choice == 0)
							{
								targetAUnit(i);
								p2units[i].setHasAssignment(true);
								p2units[i].setAssignment("unit");
							}
							else if(choice == 1 )
							{
								findCastles();
								targetCastle(i);
								p2units[i].setHasAssignment(true);
								p2units[i].setAssignment("castle");
								
							}
							else if(choice == 2)
							{
								findCastles ();
								p2units[i].setHasAssignment(true);
								p2units[i].setAssignment("moving to own castle");
								moveToOwnCastle(i);
							}
							else if (choice == 3)
							{
								findGoldMine();
								if(playergoldmine != null)
								{
									p2units[i].setHasAssignment(true);
									p2units[i].setAssignment("goldmine");
									moveToPlayerGoldMine(i);
								}
							}
						}
						else
						{
							if(p2units[i].getAssignment() != null)
								p2units[i].setHasAssignment(true);
							if(p2units[i].getAssignment() == "unit")
							{
								if(p1units.Count > 0){
									targetAUnit (i);
								}
								else if(p1units.Count == 0) {
									p2units[i].setAssignment(null);
									p2units[i].setHasAssignment(false);
								}
							}
							if(p2units[i].getAssignment() == "nearbyunit")
							{
								if(p1units.Count == 0) {
									p2units[i].setAssignment(null);
									p2units[i].setHasAssignment(false);
								}
							}
							else if(p2units[i].getAssignment() == "moving to own castle")
							{
								if((playercastle.transform.position - p2units[i].transform.position).magnitude < 3.0f)
								{
									p2units[i].setAssignment("defending castle");
								}	
							}
							else if(p2units[i].getAssignment() == "defending castle")
							{
								if(p1units.Count == 0) {
									p2units[i].setAssignment(null);
									p2units[i].setHasAssignment(false);
								}
								else
									findNearbyEnemy(i);
							}	
							else if(p2units[i].getAssignment() == "castle")
							{
								if(!p2units[i].hasTarget())
								{
									targetCastle(i);
								}
							}
							else if(p2units[i].getAssignment() == "goldmine")
							{
								if(playergoldmine == null)
								{
									p2units[i].setAssignment(null);
									p2units[i].setHasAssignment(false);
								}
							}
						}
					}
				}
			}
		}
	}

	void findGoldMine()
	{
		GoldMine[] tmp = FindObjectsOfType<GoldMine>();
		for(int i = 0; i < tmp.Length; ++i)
		{
			if(tag == "Player2" && tmp[i].tag == "P1GoldMine")
			{
				playergoldmine = tmp[i];
				return;
			}
		}
		Debug.Log("Player 1 GoldMine not found");
	}
	
	void moveToPlayerGoldMine(int index)
	{	
		Debug.Log("goldmine " + playergoldmine.transform.position.ToString());
		p2units[index].receiveNewLocation(playergoldmine.transform.position);
	}

	void findAllUnits()
	{
		p1units.Clear ();
		p2units.Clear ();
		Unit[] allunits = FindObjectsOfType<Unit> ();
		for (int i = 0; i < allunits.Length; ++i) 
		{
			if(allunits[i].tag == "Swordsman")
				p2units.Add (allunits[i]);
			else if( allunits[i].tag == "Orc" || allunits[i].tag == "Demon" || allunits[i].tag == "BlueGoblin" || allunits[i].tag == "SkullWarrior")
				p1units.Add (allunits[i]);
		}
	}
	
	void targetCastle(int index)
	{
		p2units[index].receiveNewLocation(enemycastle.transform.position);
	}
	
	void moveToOwnCastle(int index)
	{
		p2units[index].receiveNewLocation(playercastle.transform.position + new Vector3(15,0,0));
	}

	void findCastles()
	{
		if (eCastle != null && pCastle != null) {
			playercastle = pCastle.GetComponent<Castle> ();
			enemycastle = eCastle.GetComponent<Castle> ();
		} else if (eCastle == null) {
			playercastle = pCastle.GetComponent<Castle> ();
		} else if (pCastle == null) {
			enemycastle = eCastle.GetComponent<Castle> ();
		}
		/*Castle[] tmp = FindObjectsOfType<Castle>();
		for(int i = 0; i < tmp.Length; ++i)
		{
			if (tag == "Player1") 
			{	
				if( tmp[i].tag == "P1Castle")
					playercastle = tmp[i];
				else if(tmp[i].tag == "P2Castle")
					enemycastle = tmp[i];
			}
			else if(tag == "Player2")
			{
				if( tmp[i].tag == "P2Castle")
					playercastle = tmp[i];
				else if(tmp[i].tag == "P1Castle")
					enemycastle = tmp[i];
			}
		}*/
	}
	
	void findNearbyEnemy(int index)
	{
		float closest = Mathf.Infinity;
		int closest_index = -1;
		Debug.Log ("count " + p1units.Count);
		for(int i = 0; i < p1units.Count; ++i)
		{
			if( p1units[i].path != null)
			{
				int c = p1units[i].path.vectorPath.Count;
				float dist = (playercastle.transform.position - p1units[i].path.vectorPath[c-1]).magnitude;
				if(dist < 50.0f && dist < closest)
				{
					closest = dist;
					closest_index = i;
				}
			}
			else 
			{
				float dist = (playercastle.transform.position -p1units[i].transform.position).magnitude;
				if(dist < 50.0f && dist < closest)
				{
					closest = dist;
					closest_index = i;
				}
			}	
		}
		if(closest_index >= 0)
		{
			Vector3 tmp = p1units[closest_index].transform.position;
			p2units[index].receiveNewLocation(tmp);
		}
	}
	
	int nearbyEnemy(int index)
	{
		int closest_index = -1;
		float closest = Mathf.Infinity;
		for(int i = 0; i < p1units.Count; ++i)
		{
			float dist = (p2units[index].transform.position - p1units[i].transform.position).magnitude;
			if(dist < 40.0f && dist < closest)
			{
				closest_index = i;
				closest = dist;
			}
		}
		return closest_index;
	}

	void targetAUnit(int index)
	{
		double closest = Mathf.Infinity;
		//Vector3 closest_dest = new Vector3 (0, 0, 0);
		int closest_index = 0;
		for(int i = 0; i < p1units.Count; ++i)
		{
			if(index < p2units.Count){
				if( p1units[i].path != null)
				{
					int c = p1units[i].path.vectorPath.Count;
					Debug.Log ("p1units count " + p1units.Count + " i " + i) ;
					Debug.Log ("Index: " + index + "Count: " + p2units.Count);
					if(i < p1units.Count){
					double dist = (p2units[index].transform.position - p1units[i].path.vectorPath[c-1]).magnitude;
					
						if(dist < closest)
						{
							closest = dist;
							//closest_dest = p1units[i].path.vectorPath[c-1];
							closest_index = i;
						}
					}
				}
				else 
				{
					double dist = (p2units[index].transform.position -p1units[i].transform.position).magnitude;
					if(dist < closest)
					{
						closest = dist;
						//closest_dest = p1units[i].transform.position;
						closest_index = i;
					}
				}
			}
		}
		p2units[index].receiveNewLocation(p1units[closest_index].transform.position);
	}

	

	
	private Dictionary< ResourceType, int > InitResourceList() {
		Dictionary< ResourceType, int > list = new Dictionary< ResourceType, int >();
		list.Add(ResourceType.Money, 0);
		list.Add(ResourceType.Power, 0);
		return list;
	}
	
	private void AddStartResourceLimits() {
		IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
	}
	
	private void AddStartResources() {
		AddResource(ResourceType.Money, startMoney);
	}
	
	public void AddResource(ResourceType type, int amount) {
		resources[type] += amount;
		Money += amount;
	}
	
	public void IncrementResourceLimit(ResourceType type, int amount) {
		resourceLimits[type] += amount;
	}
	
	public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation) {
		Units units = GetComponentInChildren<Units>();
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName),spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
	}

	public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation, Building creator) {
		Units units = GetComponentInChildren<Units>();
		//Debug.Log ("Spawn Point: " + spawnPoint.x + " , " + spawnPoint.y + " , " + spawnPoint.z);
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName),spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
		Unit unitObject = newUnit.GetComponent<Unit>();
		if (unitObject) {
			unitObject.SetBuilding(creator);
		}
		if(unitName == "Orc" && Application.loadedLevel == 4 && !ResourceManager.GetCompletions(0,1)) ResourceManager.SetCompletions(0,1);
		if(unitName == "Blue Goblin" && Application.loadedLevel == 6 && !ResourceManager.GetCompletions(2,1)) ResourceManager.SetCompletions(2,1);
		if(unitName == "Skull Warrior" && Application.loadedLevel == 8 && !ResourceManager.GetCompletions(4,1)) ResourceManager.SetCompletions(4,1);
		if(unitName == "Demon" && Application.loadedLevel == 10 && !ResourceManager.GetCompletions(6,1)) ResourceManager.SetCompletions(6,1);
	}
	
	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
		tempBuilding = newBuilding.GetComponent< Building >();
		if (tempBuilding) {
			tempCreator = creator;
			findingPlacement = true;
			tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
			tempBuilding.SetColliders(false);
			tempBuilding.SetPlayingArea(playingArea);
		} else Destroy(newBuilding);
	}
	
	public bool IsFindingBuildingLocation() {
		return findingPlacement;
	}
	
	public void FindBuildingLocation() {
		Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
		newLocation.y = 0;
		tempBuilding.transform.position = newLocation;
	}
	
	public bool CanPlaceBuilding() {
		bool canPlace = true;
		
		Bounds placeBounds = tempBuilding.GetSelectionBounds();
		//shorthand for the coordinates of the center of the selection bounds
		float cx = placeBounds.center.x;
		float cy = placeBounds.center.y;
		float cz = placeBounds.center.z;
		//shorthand for the coordinates of the extents of the selection box
		float ex = placeBounds.extents.x;
		float ey = placeBounds.extents.y;
		float ez = placeBounds.extents.z;
		
		//Determine the screen coordinates for the corners of the selection bounds
		List<Vector3> corners = new List<Vector3>();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz-ez)));
		
		foreach(Vector3 corner in corners) {
			GameObject hitObject = WorkManager.FindHitObject(corner);
			if(hitObject && hitObject.name != "Ground") {
				WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
				if(worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) canPlace = false;
			}
		}
		return canPlace;
	}
	
	public void StartConstruction() {
		findingPlacement = false;
		Buildings buildings = GetComponentInChildren< Buildings >();
		if(buildings) tempBuilding.transform.parent = buildings.transform;
		tempBuilding.SetPlayer();
		tempBuilding.SetColliders(true);
		tempCreator.SetBuilding(tempBuilding);
		tempBuilding.StartConstruction();
	}
	
	public void CancelBuildingPlacement() {
		findingPlacement = false;
		Destroy(tempBuilding.gameObject);
		tempBuilding = null;
		tempCreator = null;
	}
}
