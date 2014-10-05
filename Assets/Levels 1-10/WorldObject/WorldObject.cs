using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Pathfinding;


public class WorldObject : MonoBehaviour {
	public string objectName;
	public Texture2D buildImage, sellImage;
	public int cost, sellValue, hitPoints, maxHitPoints, level, maxlevel;
	public float weaponRange = 10.0f, weaponRechargeTime = 1.0f, weaponAimSpeed = 1.0f;

	private float currentWeaponChargeTime;
	private List< Material > oldMaterials = new List< Material >();

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;


	protected bool attackBuilding = false;
	protected bool attackUnit = false;

	
	//Movement
	protected Vector3 destination;
	protected GameObject destinationTarget;
	protected bool newdestination;
	protected string targettype;

	protected WorldObject target = null;

	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds ();
	}

	public void SetPlayer() {
		player = transform.root.GetComponentInChildren< Player >();
	}

	protected virtual void Start () {
		SetPlayer();
		if(player) SetTeamColor();
	}
	
	protected virtual void Update () {
		currentWeaponChargeTime += Time.deltaTime;
	}

	protected virtual void FixedUpdate () {
		
	}

	protected virtual void OnGUI() {
		if (currentlySelected && Application.loadedLevel > 3  ) {
			DrawSelection ();
				}
						
	}

	public virtual void SetSelection(bool selected, Rect playingArea) {
		currentlySelected = selected;
		if(selected) this.playingArea = playingArea;
	}

	public string[] GetActions() {
		return actions;
	}
	
	public virtual void PerformAction(string actionToPerform) {
		//it is up to children with specific actions to determine what to do with each of those actions
	}
	
	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		//only handle input if currently selected
		//Debug.Log ("Mouse Click");
		if(currentlySelected && hitObject && hitObject.name != "Ground" && !hitObject.name.Contains("Tree") && !hitObject.name.Contains("Water")  && Application.loadedLevel > 3  ) {
			WorldObject worldObject = hitObject.transform.GetComponent<WorldObject>();
			//Allows models to recognize mouse input
			if(worldObject == null)
				worldObject = hitObject.transform.parent.GetComponent<WorldObject>();

			//clicked on another selectable object
			if(worldObject) {
				Resource resource = hitObject.transform.parent.GetComponent<Resource>();
				if(resource && resource.isEmpty()) return;
				Player owner = hitObject.transform.root.GetComponent<Player>();
				if(owner && player && player.human && player.username != owner.username && CanAttack())
				{
					destination = hitObject.transform.position;
					if(Application.loadedLevel == 5) ResourceManager.SetCompletions(1,0);
					newdestination = true;
				} 
				else 
					ChangeSelection(worldObject, controller);
			}
		}
	}

	// Attack Sequence
	protected virtual void UseWeapon() {
		currentWeaponChargeTime = 0.0f;
	}

	public virtual void TakeDamage(int damage) {
		hitPoints -= damage;
		if (this.GetType ().Name.Equals ("Unit")) {
			ParticleSystem blood = GetComponentInChildren<ParticleSystem> ();
			if(blood) Debug.Log("blood");
			blood.Play ();
		}
		if (hitPoints <= 0) {
			Destroy (gameObject);
			if(Application.loadedLevel == 7 && objectName == "Gold Mine" && IsOwnedBy(player) && player.tag == "Player2")
				ResourceManager.SetCompletions(3,0);
			if(Application.loadedLevel == 9 && objectName == "Barracks" && IsOwnedBy(player) && player.tag == "Player2")
				ResourceManager.SetCompletions(5,0);
			if(Application.loadedLevel == 11 && objectName == "Castle" && IsOwnedBy(player) && player.tag == "Player2")
				ResourceManager.SetCompletions(7,0);
		}
	}

	

	private bool ReadyToFire() {
		if(currentWeaponChargeTime >= weaponRechargeTime) return true;
		return false;
	}



	private bool TargetInFrontOfWeapon() {
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		if(direction.normalized == transform.forward.normalized) return true;
		else return false;
	}

	private Vector3 FindNearestAttackPosition() {
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		float targetDistance = direction.magnitude;
		float distanceToTravel = targetDistance - (0.1f * weaponRange);
		return Vector3.Lerp(transform.position, targetLocation, distanceToTravel / targetDistance);
	}


	public virtual bool CanAttack() {
		//default behaviour needs to be overidden by children
		return false;
	}
	public virtual bool isBuilding() {
		//default behaviour needs to be overidden by children
		return false;
	}
	public virtual bool isUnit() {
		//default behaviour needs to be overidden by children
		return false;
	}

	//--------------------------------
	public virtual void SetHoverState(GameObject hoverObject) {
		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			//something other than the ground is being hovered over
			if(hoverObject.name != "Ground" && !hoverObject.name.Contains("Tree")&& !hoverObject.name.Contains("Water") ) {
				Player owner = hoverObject.transform.root.GetComponent<Player>();
				//Debug.Log ("HOVER OBJECT NAME BEFORE NULL: " + hoverObject.name);
				Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
				Building building = hoverObject.transform.parent.GetComponent<Building>();
				if(owner) { //the object is owned by a player
					if(owner.username == player.username) player.hud.SetCursorState(CursorState.Select);
					else if(CanAttack()) player.hud.SetCursorState(CursorState.Attack);
					else player.hud.SetCursorState(CursorState.Select);
				} else if(unit || building && CanAttack()) player.hud.SetCursorState(CursorState.Attack);
				else player.hud.SetCursorState(CursorState.Select);
			}
		}
	}
	private void ChangeSelection(WorldObject worldObject, Player controller) {
		//this should be called by the following line, but there is an outside chance it will not
		SetSelection(false, playingArea);
		if(controller.SelectedObject) controller.SelectedObject.SetSelection(false, playingArea);
		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true, controller.hud.GetPlayingArea());
	}

	private void DrawSelection() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the playing area
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	protected virtual void DrawSelectionBox(Rect selectBox) {
		GUI.Box(selectBox, "");
		CalculateCurrentHealth(0.35f, 0.65f);
		DrawHealthBar(selectBox, "");
	}
	
	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit) {
		healthPercentage = (float)hitPoints / (float)maxHitPoints;
		if(healthPercentage > highSplit) healthStyle.normal.background = ResourceManager.HealthyTexture;
		else if(healthPercentage > lowSplit) healthStyle.normal.background = ResourceManager.DamagedTexture;
		else healthStyle.normal.background = ResourceManager.CriticalTexture;
	}
	
	protected void DrawHealthBar(Rect selectBox, string label) {
		healthStyle.padding.top = -20;
		healthStyle.fontStyle = FontStyle.Bold;
		GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
	}

	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren< Renderer >()) {
			if(!r.particleSystem)
				selectionBounds.Encapsulate(r.bounds);
		}
	}

	public bool IsOwnedBy(Player owner) {
		if(player && player.Equals(owner)) {
			return true;
		} else {
			return false;
		}
	}

	public Bounds GetSelectionBounds() {
		return selectionBounds;
	}


	public void SetColliders(bool enabled) {
		Collider[] colliders = GetComponentsInChildren< Collider >();
		foreach(Collider collider in colliders) collider.enabled = enabled;
	}
	
	public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
		if(storeExistingMaterial) oldMaterials.Clear();
		Renderer[] renderers = GetComponentsInChildren< Renderer >();
		foreach(Renderer renderer in renderers) {
			if(storeExistingMaterial) oldMaterials.Add(renderer.material);
			renderer.material = material;
		}
	}
	
	public void RestoreMaterials() {
		Renderer[] renderers = GetComponentsInChildren< Renderer >();
		if(oldMaterials.Count == renderers.Length) {
			for(int i = 0; i < renderers.Length; i++) {
				renderers[i].material = oldMaterials[i];
			}
		}
	}
	
	public void SetPlayingArea(Rect playingArea) {
		this.playingArea = playingArea;
	}


	protected void SetTeamColor() {
		TeamColor[] teamColors = GetComponentsInChildren< TeamColor >();
		foreach(TeamColor teamColor in teamColors) teamColor.renderer.material.color = player.teamColor;
	}

}
