using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;


public class Swordsman : Unit {
	private Quaternion aimRotation;

	//A* pathfinding
	public Seeker seeker;
	//public Path path;
	private float nextWayPointDistance = 3.0f;
	private bool Attack_Anim, Run_Anim, Idle_Anim;
	
	
	protected override void Start () {
		base.Start ();
		Attack_Anim = false;
		Idle_Anim = true;
		Run_Anim = false;
		seeker = GetComponent<Seeker>();
		if (Application.loadedLevel >= 8) {
			hitPoints += (int)hitPoints / 2;
			maxHitPoints += (int)maxHitPoints / 2;
			damage += (int)damage / 2;
				}
	}
	
	float timer_attackdelay = 0.0f;
	
	protected override void Update () {
		base.Update();
		
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
			if(dist > 4.0f && (targettype.Contains("Orc") || targettype.Contains("BlueGoblin") || targettype.Contains("Demon") || targettype.Contains("Skull")  ))
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
		
		if ((kills >= level * 2) && level < maxlevel)
		{
			level++;
			hitPoints = (int)(hitPoints * 1.5);
			maxHitPoints = (int)(maxHitPoints * 1.5);
			damage = (int)(damage * 1.5);
		}
		
		if (Run_Anim) {
			animation.CrossFade ("run");
		} else if (Attack_Anim) {
			animation.CrossFade("attack");
		} else if (Idle_Anim) {
			animation.CrossFade ("idle");
		} 
	}
	
	
	IEnumerator Attack1() {
		animation.CrossFade ("attack");
		yield return new WaitForSeconds(animation.clip.length);
	}

	//A* pathfinding
	protected void OnPathComplete(Path p)
	{
		if (!p.error) {
//			Debug.Log (tag + " is at destination");
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
			if(getAttacking())
			{
				return;
			}
			Vector3 dir = (path.vectorPath [currentWaypoint] - transform.position).normalized;
			dir *= moveSpeed * Time.fixedDeltaTime;
			Attack_Anim = false;
			Idle_Anim = false;
			Run_Anim = true;
			GetComponent<Rigidbody> ().MovePosition (rigidbody.position + dir);
			GetComponent<Rigidbody> ().transform.LookAt (path.vectorPath[currentWaypoint]);
			CalculateBounds();
			if (Vector3.Distance (rigidbody.position, path.vectorPath [currentWaypoint]) < nextWayPointDistance) {
				currentWaypoint++;
				return;
			}
		}
		else 
		{
			Run_Anim = false;
			Idle_Anim = true;
		}
	}
	
	protected override void OnCollisionEnter(Collision collision)
	{
		if((collision.gameObject.name.Contains("Orc")  || collision.gameObject.name.Contains("Blue") 
		    || collision.gameObject.name.Contains("Demon") || collision.gameObject.name.Contains("Skull") )&& attacking == false)
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
						//Debug.Log ("assignment before " + getAssignment());
						if(path != null)
							path.vectorPath.Clear();
						attacking = true;
						setHasAssignment(false);

						
						targettype = "Orc";
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
//				if(path != null)
//					path.vectorPath.Clear ();
				attacking = true;
				targettype = "Castle";
			}
		}
		else if(collision.gameObject.name.Contains("Gold"))
		{
			GameObject obj = collision.gameObject;
			Player p = obj.transform.root.GetComponentInChildren<Player>(); 
			if(p == null)
				return;
			
			if(p != player)
			{
				target = obj.transform.root.GetComponentInChildren<GoldMine>();
//				if(path != null)
//					path.vectorPath.Clear ();
				attacking = true;
				targettype = "GoldMine";
			}
		}
	}
	
	void OnCollisionStay(Collision collision)
	{
		if((collision.gameObject.name.Contains("Orc")  || collision.gameObject.name.Contains("Blue") 
		    || collision.gameObject.name.Contains("Demon") || collision.gameObject.name.Contains("Skull") ) && target == null && attacking == false)
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
}
