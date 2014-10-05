using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour {
	private Player player;
	public AudioClip barracks,castle,goldmine,orc,bluegoblin,skullwarrior,demon,Button;
	private float timer;
	private float counter;
	private static int counts = 0;
	private bool pause = false;
	
	// Use this for initialization
	void Start () {
		counter = 0f;
		timer = 5f;
		player = transform.root.GetComponent<Player> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.P)) {
			Time.timeScale = 0;
			counts = 1;
			pause = true;
		}

		counter += Time.deltaTime;
		if (counter >= timer && Application.loadedLevel > 3) {
			counter = 0f;
		}
		if (player.human && Application.loadedLevel > 3 ) {
						MoveCamera ();
						RotateCamera ();
						MouseActivity ();
		} else if(Application.loadedLevel < 4 ) {
						MouseActivity ();
				}
	}

	void OnGUI(){
		if (counts == 1) {
			if (pause) {
				GUI.Box (new Rect (580, 200, 100, 105), "Pause");
				if (GUI.Button (new Rect (590, 220, 80, 20), "Resume")) {
					pause = false;
					counts = 0;
					Time.timeScale = 1;
				}
				if (GUI.Button (new Rect (590, 240, 80, 20), "Retry")) {
					pause = false;
					counts = 0;
					Time.timeScale = 1;
					ResourceManager.ResetCompletions(Application.loadedLevel - 4, 0);
					ResourceManager.ResetCompletions(Application.loadedLevel - 4, 1);
					AutoFade.LoadLevel (Application.loadedLevel, 1f, 1f, Color.black);
				}
				if (GUI.Button (new Rect (590, 260, 80, 20), "Main Menu")) {
					pause = false;
					counts = 0;
					Time.timeScale = 1;
					AutoFade.LoadLevel (0, 1f, 1f, Color.black);
				}
				if (GUI.Button (new Rect (590, 280, 80, 20), "Quit")) {
					Application.Quit ();
				}
			}
		}
	}

	private void MoveCamera() {
		Vector3 movement = new Vector3(0,0,0);
		bool mouseScroll = false;

		////////////////////////////////////////////////////////////////////////////////////////////////
		//horizontal camera movement
		if(Input.GetKey(KeyCode.A))
			movement.x -= ResourceManager.ScrollSpeed;
		else if(Input.GetKey(KeyCode.D))
			movement.x += ResourceManager.ScrollSpeed;
		
		//vertical camera movement
		if(Input.GetKey(KeyCode.S))
			movement.z -= ResourceManager.ScrollSpeed;
		else if(Input.GetKey(KeyCode.W))
			movement.z += ResourceManager.ScrollSpeed;
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;
		
		//away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;

		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.x > ResourceManager.MaxCameraX) {
			destination.x = ResourceManager.MaxCameraX;
		} else if(destination.x < ResourceManager.MinCameraX) {
			destination.x = ResourceManager.MinCameraX;
		}

		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}

		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.z > ResourceManager.MaxCameraZ) {
			destination.z = ResourceManager.MaxCameraZ;
		} else if(destination.z < ResourceManager.MinCameraZ) {
			destination.z = ResourceManager.MinCameraZ;
		}

		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}

		if(!mouseScroll) {
			player.hud.SetCursorState(CursorState.Select);
		}

	}
	
	private void RotateCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		if (ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			destination.x += ResourceManager.RotateAmount;
			player.hud.SetCursorState (CursorState.PanDown);    
		} 
		else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			destination.x -= ResourceManager.RotateAmount;
			player.hud.SetCursorState (CursorState.PanUp);
		}
		
		if (xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			destination.y -= ResourceManager.RotateAmount;
			player.hud.SetCursorState(CursorState.PanLeft);
		} 
		else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			destination.y += ResourceManager.RotateAmount;
			player.hud.SetCursorState(CursorState.PanRight);
		}
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}

	private void MouseActivity() {
		if(Input.GetMouseButtonDown(0)) LeftMouseClick();
		else if(Input.GetMouseButtonDown(1)) RightMouseClick();
		MouseHover();
	}
	
	private void LeftMouseClick() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) player.StartConstruction();
			} else {
				GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
					if(player.SelectedObject) player.SelectedObject.MouseClick(hitObject, hitPoint, player);
					else if(hitObject.name == "LowWall"){
						//Play Sound Here
						audio.PlayOneShot(Button);
						if(Application.loadedLevel == 0){  
							AutoFade.LoadLevel(4,1f,1f,Color.black);
						}else if(Application.loadedLevel == 1){
							AutoFade.LoadLevel(0,1f,1f,Color.black);
						}else if(Application.loadedLevel == 2 && ResourceManager.previousLevel != 11){
							//Load The Next level..
							Debug.Log ("CLICKED NEXT LEVEL");
							AutoFade.LoadLevel(ResourceManager.previousLevel + 1,1f,1f,Color.black);
						}else if(ResourceManager.previousLevel == 11){////////////////////////////////////////////////////////////////////////////////////////////////
							AutoFade.LoadLevel(0, 1f, 1f, Color.black);
						}
						////////////////////////////////////////////////////////////////////////////////////////////////
					}else if(hitObject.name == "Back"){
						//Play Sound Here
						audio.PlayOneShot(Button);
						AutoFade.LoadLevel(0,1f,1f,Color.black);
					}else if(hitObject.name == "LowWall2"){
						//Play Sound Here
						audio.PlayOneShot(Button);
						AutoFade.LoadLevel(3,1f,1f,Color.black);
					}else if(hitObject.name == "LowWall3"){
						//Play Sound Here
						audio.PlayOneShot(Button);
						Application.Quit();
					}
					else if(hitObject.name != "Ground" && !hitObject.name.Contains("Tree")&& !hitObject.name.Contains("Water")  && Application.loadedLevel > 3  ) {
						//Debug.Log("UserInput child : " + hitObject.name);
						WorldObject worldObject = hitObject.transform.GetComponent<WorldObject>();
						//Allows models to recognize mouse input
						if(worldObject == null)
								worldObject = hitObject.transform.parent.GetComponent<WorldObject>();

						if(worldObject) {
							//Debug.Log("Name Of PARENT: " + hitObject.transform.parent.name);
							//we already know the player has no selected object
							//Debug.Log("UserInput LC is WorldObject: " + hitObject.name);
							player.SelectedObject = worldObject;
							worldObject.SetSelection(true, player.hud.GetPlayingArea());
							if(worldObject.objectName == "Barracks") {
								if(Application.loadedLevel == 4) ResourceManager.SetCompletions(0, 0);
								audio.PlayOneShot(barracks);
							}	
							if(worldObject.objectName == "Castle") {
								if(Application.loadedLevel == 4) ResourceManager.SetCompletions(0, 0);
								audio.PlayOneShot(castle);
							}
							if(worldObject.objectName == "Gold Mine"){
								if(Application.loadedLevel == 4) ResourceManager.SetCompletions(0, 0);
								audio.PlayOneShot(goldmine);
							}
							if(worldObject.objectName == "Orc")
								audio.PlayOneShot(orc);
							if(worldObject.objectName == "Blue Goblin")
								audio.PlayOneShot(bluegoblin);
							if(worldObject.objectName == "Skull Warrior")
								audio.PlayOneShot(skullwarrior);
							if(worldObject.objectName == "Demon")
								audio.PlayOneShot(demon);
						}
					}
				}
			}
		}
	}

	private void RightMouseClick() {
		if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject) {
			if(player.IsFindingBuildingLocation()) {
				player.CancelBuildingPlacement();
			} else {
				player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
				player.SelectedObject = null;
			}
		}
	}

	private void MouseHover() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				player.FindBuildingLocation();
			}else{
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
				if(hoverObject) {
					if(player.SelectedObject) player.SelectedObject.SetHoverState(hoverObject);
					else if(hoverObject.name != "Ground") {
						Player owner = hoverObject.transform.root.GetComponent< Player >();
						if(owner) {
							Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
							Building building = hoverObject.transform.parent.GetComponent< Building >();
							if(owner.username == player.username && (unit || building)) player.hud.SetCursorState(CursorState.Select);
						}
					}
				}
			}
		}
	}



}
