import UnityEngine
import System
#import System.Random

class AlienBehaviorScript (MonoBehaviour): 
	public awarenessRadius as Single = 30.0F
	public turningSpeed as Single = 50.0F
	public walkDistance as Single = 5.0F
	public doWalk as Boolean = true
	public doShoot as Boolean = true
	public walkSpeed as Single = 1.0F
	
	private soldier as CharacterController
	private scientist as CharacterController
	private rng as System.Random
	private originalPosition as Vector3
	private walkDirection as Vector3
	
	def Awake() as void:
		pass
		
	def Start() as void:
		soldier = gameObject.Find("Soldier").GetComponentInChildren(CharacterController)
		scientist = gameObject.Find("Scientist").GetComponentInChildren(CharacterController)
		originalPosition = transform.position
		rng = System.Random()
		return
	
	def Update() as void:
		transform.Find("GunParticle").gameObject.active = false
		if transform.GetComponent(ViolenceScript).IsDead:
			return
		canSeeSoldier = CanSee(soldier) and not soldier.transform.GetComponent(ViolenceScript).IsDead
		canSeeScientist = CanSee(scientist) and not scientist.transform.GetComponent(ViolenceScript).IsDead
		if canSeeSoldier:
			transform.GetComponent(AnimationManagerScript).Play("Duck")
			transform.GetComponent(AnimationManagerScript).Play("Shoot")
			Rotate(soldier)
			transform.Find("GunParticle").gameObject.active = true
			ShootAt(soldier)
		elif canSeeScientist:
			transform.GetComponent(AnimationManagerScript).Play("Duck")
			transform.GetComponent(AnimationManagerScript).Play("Shoot")
			Rotate(scientist)
			transform.Find("GunParticle").gameObject.active = true
			ShootAt(scientist)
		elif doWalk:
			transform.GetComponent(AnimationManagerScript).Play("Stand")
			Walk()
		return

	def FixedUpdate() as void:
		pass
		
	def InRange(actor as CharacterController) as Boolean:
		return Vector3.Distance(transform.position, actor.transform.position) <= awarenessRadius
		
	def CanSee(actor as CharacterController) as Boolean:
		hit as RaycastHit
		if not InRange(actor):
			return false
		if Physics.Linecast(transform.position, actor.transform.position, hit):
			return hit.transform == actor.transform
		return false
		
	def Rotate(target as CharacterController) as void:
		transform.rotation = Quaternion.Slerp(transform.rotation,
			Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up),
			Time.deltaTime * turningSpeed)
		return 
		
	def ShootAt(target as CharacterController):
		//Debug.Log("shooting at " + target.name)
		if not transform.GetComponent(ViolenceScript).CanShoot:
			return
		transform.GetComponent(ViolenceScript).Fire(transform.position, transform.TransformDirection(Vector3.forward))
		return 
		
	def Walk() as void:
		if Vector3.Distance(transform.position, originalPosition) < 0.1F:
			transform.position = originalPosition
			walkDirection = GetRandomDirection()
			transform.rotation = Quaternion.LookRotation(walkDirection)
		elif Vector3.Distance(transform.position, originalPosition) > walkDistance:
			walkDirection = -walkDirection
			transform.rotation = Quaternion.LookRotation(walkDirection)
		transform.GetComponent(CharacterController).Move(walkDirection)
		transform.GetComponent(AnimationManagerScript).Play("Walk")
		return
		
	private def GetRandomDirection() as Vector3:
		vector as Vector3
		vector = Vector3(rng.NextDouble() * 2 - 1, 0, rng.NextDouble() * 2 - 1)
		vector.Normalize()
		return vector * walkSpeed
		