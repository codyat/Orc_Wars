using UnityEngine;
using System.Collections;
using Pathfinding;
using RTS;

public class Unit : WorldObject {
	private Quaternion targetRotation;
	public float moveSpeed, rotateSpeed;
	protected int currentWaypoint = 1;
	public int damage , kills = 0;
	public AudioClip attacksound;
	public Path path;
	public float buildTime;
	public int MissRate;
	
	//AI
	protected bool has_assignment = false;
	protected bool is_attacking = false;
	protected string objectattacking; 

	protected bool attacking = false; 
	protected bool p1defendingcastle = false;
	
	/*** Game Engine methods, all can be overridden by subclass ***/
	
	protected override void Awake() {
		base.Awake();
	}
	
	protected override void Start () {
		base.Start();
	}
	
	//TROOP MOVEMENT
	protected override void Update () {
		base.Update();
	}
	
	protected override void FixedUpdate () {
		base.FixedUpdate();
	}
	
	protected virtual void OnCollisionEnter(Collision collision)
	{
		
	}
	
	protected override void OnGUI() {
		base.OnGUI();
	}
	
	public virtual void SetBuilding(Building creator) {
		//specific initialization for a unit can be specified here
	}
	
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			bool moveHover = false;
			if(hoverObject.name == "Ground"  && hoverObject.name.Contains("Tree")) {
				moveHover = true;
			} else {
				//Resource resource = hoverObject.transform.parent.GetComponent< Resource >();
				//if(resource && resource.isEmpty()) moveHover = true;
			}
			if(moveHover) player.hud.SetCursorState(CursorState.Move);
		}
	}
	
	
	public override bool isUnit() {
		//default behaviour needs to be overidden by children
		return true;
	}


	private void CalculateTargetDestination() {
		//calculate number of unit vectors from unit centre to unit edge of bounds
		Vector3 originalExtents = selectionBounds.extents;
		Vector3 normalExtents = originalExtents;
		normalExtents.Normalize();
		float numberOfExtents = originalExtents.x / normalExtents.x;
		int unitShift = Mathf.FloorToInt(numberOfExtents);
		
		//calculate number of unit vectors from target centre to target edge of bounds
		WorldObject worldObject = destinationTarget.GetComponent< WorldObject >();
		if(worldObject) originalExtents = worldObject.GetSelectionBounds().extents;
		else originalExtents = new Vector3(0.0f, 0.0f, 0.0f);
		normalExtents = originalExtents;
		normalExtents.Normalize();
		numberOfExtents = originalExtents.x / normalExtents.x;
		int targetShift = Mathf.FloorToInt(numberOfExtents);
		
		//calculate number of unit vectors between unit centre and destination centre with bounds just touching
		int shiftAmount = targetShift + unitShift;
		
		//calculate direction unit needs to travel to reach destination in straight line and normalize to unit vector
		Vector3 origin = transform.position;
		Vector3 direction = new Vector3(destination.x - origin.x, 0.0f, destination.z - origin.z);
		direction.Normalize();
		
		//destination = center of destination - number of unit vectors calculated above
		//this should give us a destination where the unit will not quite collide with the target
		//giving the illusion of moving to the edge of the target and then stopping
		for(int i = 0; i < shiftAmount; i++) destination -= direction;
		destination.y = destinationTarget.transform.position.y;
	}

	//Not Used
	/*private void MakeMove() {
		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		if(transform.position == destination) {
			moving = false;
			movingIntoPosition = false;
		}
		CalculateBounds();
	}*/
	
	
	
	
	
	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.MouseClick(hitObject, hitPoint, controller);
		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			bool clickedOnEmptyResource = false;
			if(hitObject.transform.parent) {
				Resource resource = hitObject.transform.parent.GetComponent< Resource >();
				if(resource && resource.isEmpty()) clickedOnEmptyResource = true;
			}
			if((hitObject.name == "Ground" || clickedOnEmptyResource) && hitPoint != ResourceManager.InvalidPosition) {
				float x = hitPoint.x;
				//makes sure that the unit stays on top of the surface it is on
				float y = hitPoint.y + player.SelectedObject.transform.position.y;
				float z = hitPoint.z;
				destination = new Vector3(x,y,z);
				newdestination = true;
			}
		}
	}
	

	//
	//
	//AI
	public bool getIsAttacking()
	{
		return is_attacking;
	}
	
	public bool getHasAssignment()
	{
		return has_assignment;
	}
	
	public void setHasAssignment(bool value)
	{
		has_assignment = value;
	}

	public string getAssignment()
	{
		return objectattacking;
	}

	public void setAssignment(string value)
	{
		objectattacking = value;
	}

	public void Sell() {
		if(player) player.AddResource(ResourceType.Money, sellValue);
		if(currentlySelected) SetSelection(false, playingArea);
		Destroy(this.gameObject);
	}

	public void receiveNewLocation(Vector3 dest)
	{
		destination = dest;
		newdestination = true;
	}
	
	public Vector3 getCurrentDestination()
	{
		return destination;
	}
	
	public bool hasTarget()
	{
		if(target != null)
			return true;
		else
			return false;
	}

	public bool getP1DefendingCastle()
	{
		return p1defendingcastle;
	}

	public void setP1DefendingCastle(bool b)
	{
		p1defendingcastle = b;
	}

	public bool getAttacking()
	{
		return attacking;
	}
}