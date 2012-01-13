import UnityEngine
import System

class ViolenceScript (MonoBehaviour):
	public health as int = 100
	public armour as int = 100
	public canShoot as Boolean
	public canThrow as Boolean
	public canMelee as Boolean
	public shootDamage as int = 20
	public meleeDamage as int = 200
	public throwDamage as int = 100
	public shootAmmo as int = 500
	public throwAmmo as int = 5
	public shootFrequency as Single = 0.1F
	public throwFrequency as Single = 1.0F
	public throwingProjectile as Rigidbody
	public throwingSpeed as Single = 40.0F
	public dropOnThrow as Boolean = false
	
	private animationManager as AnimationManagerScript
	private hasDied as Boolean = false
	private shootTimer as Single = 0.0F
	private throwTimer as Single = 0.0F
	
	CanShoot as Boolean:
		get:
			return canShoot and shootTimer >= shootFrequency
			
	CanThrow as Boolean:
		get:
			return canThrow and throwTimer >= throwFrequency
	
	IsDead as Boolean:
		get:
			if health <= 0:
				return true
	
	def Start ():
		animationManager = GetComponent(AnimationManagerScript)
		shootTimer = shootFrequency
		throwTimer = throwFrequency
	
	def Update ():
		shootTimer += Time.deltaTime
		throwTimer += Time.deltaTime
		
	def Throw() as void:
		Debug.Log("throw!")
		instantiatedProjectile as Rigidbody
		throwAmmo--
		throwTimer = 0
		if throwAmmo < 1:
			canThrow = false
		myCamera as Camera = transform.GetComponentInChildren(Camera)
		instantiatedProjectile = Instantiate(throwingProjectile, transform.position, myCamera.transform.rotation)
		if dropOnThrow:
			instantiatedProjectile.transform.GetComponent(Rigidbody).mass = 100
		else:
			throwDirection = Vector3.forward
			# magic numbers make the world go round and the grenade move correctly
			#throwDirection.z += 1
			#throwDirection.y += .5
		instantiatedProjectile.velocity = myCamera.transform.TransformDirection(throwDirection * throwingSpeed)
		Physics.IgnoreCollision(instantiatedProjectile.collider, transform.collider)
		return
		
	def Fire() as void:
		childCamera = transform.Find("Main Camera")
		Fire(childCamera.transform.position, childCamera.transform.TransformDirection(Vector3.forward))
		return
		
	def Fire(position as Vector3, direction as Vector3) as void:
		hit as RaycastHit
		layerMask as LayerMask = ~(1 << gameObject.layer)
		shootTimer = 0
		shootAmmo--
		if shootAmmo < 1:
			canShoot = false
		animationManager.Play("Shoot")
		if Physics.Raycast(position, direction, hit, Mathf.Infinity, layerMask):
			if not (hit.collider isa Rigidbody or hit.collider isa CharacterController):
				Debug.Log("I hit nothing")
				return
			//Debug.Log("I hit " + hit.collider.name)
			HitIt(hit, shootDamage)
		else:
			Debug.Log("I hit nothing")
		return
		
	def Melee() as void:
		direction = transform.TransformDirection(Vector3.forward)
		hit as RaycastHit
		layerMask as int = ~(1 << gameObject.layer)
		
		animationManager.Play("Melee")
		if Physics.Raycast(transform.position, direction, hit, 1.0F, layerMask):
			if not (hit.collider isa Rigidbody or hit.collider isa CharacterController):
				return
			Debug.Log("I melee'd " + hit.collider.name)
			HitIt(hit, meleeDamage)
		else:
			Debug.Log("I melee'd nothing.")
		return
		
	def HitIt(hit as RaycastHit, damage as int) as void:
		otherViolence = hit.collider.GetComponentInChildren(ViolenceScript)
		if not otherViolence:
			return
		otherViolence.HitMe(damage)
		return
		
	public def HitMe(damage as int) as void:
		armour -= damage
		if armour < 0:
			health += armour
			armour = 0
		if health < 0 and not hasDied:
			animationManager.Play("Die")
			hasDied = true
		return