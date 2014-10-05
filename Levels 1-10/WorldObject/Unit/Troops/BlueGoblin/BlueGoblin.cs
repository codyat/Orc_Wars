using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;


public class BlueGoblin : Unit {
	private Quaternion aimRotation;
	
	//A* pathfinding
	public Seeker seeker;
	//public Path path;
	private float nextWayPointDistance = 3.0f;
	private bool Attack_Anim, Run_Anim, Idle_Anim;
	
	
	protected override void Start () {
		base.Start ();
		//this is fine
		Attack_Anim = false;
		Idle_Anim = true;
		Run_Anim = false;
		
		seeker = GetComponent<Seeker>();
		
	}
	
	float timer_attackdelay = 0.0f;
	
	protected override void Update () {
		base.Update ();
		if(target == null)
		{
			attacking = false;
			timer_attackdelay = 0.0f;
			Attack_Anim = false;
		}
		
		if(newdestination)
		{
			path = seeker.StartPath(transform.position, destination, OnPathComplete);
			newdestination = false;
		}
		
		if(attacking && timer_attackdelay < .4f )
		{
			timer_attackdelay += Time.deltaTime;
			GetComponent<Rigidbody> ().transform.LookAt (target.transform.position);
			Vector3 dir = (target.transform.position - transform.position);
			float dist = dir.magnitude;
			if(dist > 4.0f && targettype.Equals("Swordsman"))
			{	
				dir *= moveSpeed * Time.fixedDeltaTime;
				GetComponent<Rigidbody> ().MovePosition (rigidbody.position + dir);
			}
		}
		else if(attacking && timer_attackdelay >= 0.4f)
		{
			timer_attackdelay = 0.0f;
			Attack_Anim = true;
			Run_Anim = false;
			Idle_Anim = false;
			UseWeapon();
		}
		
		if ((kills >= level * 2) && level < maxlevel) {
			level++;
			if(Application.loadedLevel == 7 && level == 2) ResourceManager.SetCompletions(3,1);
			if(Application.loadedLevel == 9 && level == 3) ResourceManager.SetCompletions(5,1);
			maxHitPoints = (int)(maxHitPoints * 1.5);
			hitPoints = maxHitPoints;
			damage = (int)(damage * 1.5);
		}
		
		if (Run_Anim) {
			animation.CrossFade ("anim_run");
		} else if (Attack_Anim) {
			Debug.Log ("ATTACKING ANIMATION PLAYING NOW");
			animation.CrossFade ("anim_attack_01");
		} else if (Idle_Anim) {
			animation.CrossFade ("anim_idle");
		}
	}
	
	IEnumerator Attack1() {
		animation.CrossFade ("anim_attack_01");
		yield return new WaitForSeconds(animation.clip.length);
	}
	
	
	protected void OnPathComplete(Path p)
	{
		if (!p.error) {
			path = p;
			currentWaypoint = 0;
		}
	}
	
	protected override void FixedUpdate ()
	{
		base.FixedUpdate ();
		if (path == null) {
			return;
		}
		if (currentWaypoint < path.vectorPath.Count) 
		{
			Vector3 dir = (path.vectorPath [currentWaypoint] - transform.position).normalized;
			dir *= moveSpeed * Time.fixedDeltaTime;
			GetComponent<Rigidbody> ().MovePosition (rigidbody.position + dir);
			
			Run_Anim = true;
			Attack_Anim = false;
			Idle_Anim = false;
			
			CalculateBounds();
			GetComponent<Rigidbody> ().transform.LookAt (path.vectorPath[currentWaypoint]);
			if (Vector3.Distance (rigidbody.position, path.vectorPath [currentWaypoint]) < nextWayPointDistance) {
				currentWaypoint++;
				return;
			}
		}
		else //reach the destination
		{
			Run_Anim = false;
			if(!attacking){
				Idle_Anim = true;
			}else{
				Attack_Anim = true;
				Run_Anim = false;
				Idle_Anim = false;
			}
		}
	}
	
	protected override void OnCollisionEnter(Collision collision)
	{
		//Debug.Log ("OnCollision " + collision.gameObject.name);
		
		if(collision.gameObject.name.Contains("Swordsman") && attacking == false)
		{
			GameObject obj = collision.gameObject;
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				WorldObject[] wo = obj.transform.root.GetComponentsInChildren<WorldObject>();
				for(int i = 0; i < wo.Length; ++i)
				{
					if(wo[i].transform.position == obj.transform.position)
					{
						target = wo[i];
						if(path != null)
							path.vectorPath.Clear();
						attacking = true;
						targettype = "Swordsman";
						return;
					}
				}
			}
		}
		else if(collision.gameObject.name.Contains("Castle"))
		{
			GameObject obj = collision.gameObject;
			//Debug.Log ("on collison Castle");
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				target = obj.transform.root.GetComponentInChildren<Castle>();
				if(path != null)
					path.vectorPath.Clear ();
				attacking = true;
				targettype = "Castle";
			}
		}
		else if(collision.gameObject.name.Contains("GoldMine"))
		{
			GameObject obj = collision.gameObject;
			//Debug.Log ("on collison Castle");
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				target = obj.transform.root.GetComponentInChildren<GoldMine>();
				if(path != null)
					path.vectorPath.Clear ();
				attacking = true;
				targettype = "GoldMine";
			}
		}
		else if(collision.gameObject.name.Contains("Barracks"))
		{
			GameObject obj = collision.gameObject;
			//Debug.Log ("on collison Castle");
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				target = obj.transform.root.GetComponentInChildren<Barracks>();
				if(path != null)
					path.vectorPath.Clear ();
				attacking = true;
				targettype = "Barracks";
			}
		}
	}		
	
	void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.name.Contains("Swordsman") && target == null && attacking == false)
		{
			GameObject obj = collision.gameObject;
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				WorldObject[] wo = obj.transform.root.GetComponentsInChildren<WorldObject>();
				for(int i = 0; i < wo.Length; ++i)
				{
					if(wo[i].transform.position == obj.transform.position)
					{
						target = wo[i];
						if(path != null)
							path.vectorPath.Clear();
						attacking = true;
						return;
					}
				}
			}
		}
	}
	
	protected override void UseWeapon () {
		base.UseWeapon();
		//this is fine
		Attack_Anim = true;
		Idle_Anim = false;
		Run_Anim = false;
		Vector3 spawnPoint = transform.position;
		//		spawnPoint.x += (2.1f * transform.forward.x);
		//		spawnPoint.y += 1.4f;
		//		spawnPoint.z += (2.1f * transform.forward.z);
		//Create the projectile
		//if(Random.Range(0,100) < MissRate){
		GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("TankProjectile"), spawnPoint, transform.rotation);
		Projectile projectile = gameObject.GetComponentInChildren< Projectile >();
		projectile.SetRange(0.9f * weaponRange);
		projectile.attacker = this;
		projectile.SetTarget(target);
		projectile.SetDamage (damage);
		//	}
	}
	
	public override bool CanAttack() {
		return true;
	}

	public override void TakeDamage(int damage)
	{
		base.TakeDamage (damage);
		GetComponent<ParticleSystem> ().Play ();
	}
	
	//	public override void TakeDamage(int damage) {
	//		hitPoints -= damage;
	//		Attack_Anim = false;
	//		Idle_Anim = false;
	//		Run_Anim = false;
	//		if(hitPoints<=0) Destroy(gameObject);
	//	}
}
