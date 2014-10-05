using UnityEngine;
using System.Collections;
using RTS;

public class Projectile : MonoBehaviour {
	
	public float velocity = 1;
	public int damage = 1;
	public Unit attacker;

	private float range = 1;
	private WorldObject target;

	void Update () {
		if(HitSomething()) {
			InflictDamage();
			Destroy(gameObject);
		}
		if(range>0) {
			float positionChange = Time.deltaTime * velocity;
			range -= positionChange;
			transform.position += (positionChange * transform.forward);
		} else {
			Destroy(gameObject);
		}
	}
	
	public void SetRange(float range) {
		this.range = range;
	}
	
	public void SetTarget(WorldObject target) {
		this.target = target;
	}

	public void SetDamage(int d) {
		this.damage = d;
	}

	private bool HitSomething() {
		if(target && target.GetSelectionBounds().Contains(transform.position)) return true;
		return false;
	}
	
	private void InflictDamage() {
		if (target) {
			target.TakeDamage (damage);
			if (target.hitPoints <= 0){
				if(Application.loadedLevel == 5 && attacker.objectName != "Swordsman") ResourceManager.SetCompletions(1,1);
					attacker.kills++;
				Player player = attacker.transform.root.GetComponentInChildren< Player >();
				player.Money += 25;
			}
		}
	}
}