using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class HumanoidHandler : MonoBehaviour 
{
	public static List<HumanoidHandler> ListOfHumans = new List<HumanoidHandler>();
	public static List<HumanoidHandler> ListOfZombies = new List<HumanoidHandler>();
	
	/// <summary>
	/// Character attack using state--User defined, type of attack being used
	/// </summary>
	public enum StateCharacterAtkUsing { WeakAttack, MediumAttack, StrongAttack }
	
	
	/// <summary>
	/// Character attack behave state--User defined, stand until attacked or attack nearest
	/// </summary>
	public enum StateCharacterBehave { StandAndRun, Stand, AtkNearest }
	
	/// <summary>
	/// Character state--Zombie or Human.
	/// </summary>
	public enum StateCharacterType { Zombie, Human }
	
	/// <summary>
	/// Zombie type--The type the character is or will turn into if infected.
	/// </summary>
	public enum StateZombieType { Sneeky, Strong, Fast }
	/// <summary>
	/// Animation Atate->animation: Standing->standing, Walking->walking or shambling, 
	///           Running->running, Attacking-> hitting or shooting
	/// 
	/// NOTE: Standing, Walking, Running, and Attacking add HoldGun to respective animation
	/// </summary>
	public enum StateAnim { Standing, Walking, Running, Attacking }
	
	
		
	public static Texture2D FastTex;
	public static Texture2D SneakyTex;
	//public static Texture2D ToughTex;
	public static Texture2D HumanTex;
	//public static Texture2D MilitaryTex;
	
	/// State enum variables--The enum variables representing current states.
	
	public StateCharacterAtkUsing CurrUsingAttack;
	public StateCharacterBehave CurrBehavior;
	public StateCharacterType CurrCharacterType;
	public StateZombieType CurrZombType;
	public StateAnim CurrAnimation;
	
	public bool isSelected; //By the player, needed in AIPather.cs so all zombies aren't moved at once.
	
	
	/// <summary>
	/// isTargeting -> either targeting a walkToPoint or an enemy.
	/// If not targeting, the user is controling the zombie.
	/// </summary>
	public bool isTargeting;
	public bool isArmed;
	public int maxHealth;
	public int currHealth;
	public int maxResistance;// for infecting humans
	public int currResistance;
	public int baseAttackDamage;
	public float timeSinceLastAttack;
	public float speedPerSec;
	
	// the following are used in pathfinding
	private Seeker seeker;
	private Path path;
	private CharacterController controller;
	//<summary>Once this dist is reached, go to the next Path point</summary>
	private float distToIncrementPathPoint = .5f;
	//<summary>The current Path point</summary>
	private int currPathPointIndex = 0;
	
	//<summary>Initial position where a new Path was obtained</summary>
	private Vector3 initTargetPos
	{
		set;
		get;
	}
	
	
	
	//<summary>The Transform of the Target. Used to track target's movement</summary>
	public Transform TransOfTarget 
	{
		set;
		get;
	}
	
	
	//<summary>Self Pos</summary>
	public Vector3 Position
	{
		get
		{
			return transform.position;
		}
	}
	
	// Use this for initialization
	public void Start() 
	{
		// Get components for pathfinding
		seeker = GetComponent<Seeker>();
		controller = GetComponent<CharacterController>();
		
		FastTex = (Texture2D)Resources.Load ("ZombieFastTex");
		SneakyTex = (Texture2D)Resources.Load ("ZombieSneakyTex");
		//ToughTex = (Texture2D)Resources.Load ("ZombieToughTex");
		HumanTex = (Texture2D)Resources.Load ("HumanTex");
		//MilitaryTex = (Texture2D)Resources.Load ("MilitaryTex");
		
		
		CurrUsingAttack = StateCharacterAtkUsing.WeakAttack;
		
		CurrBehavior = StateCharacterBehave.AtkNearest;
		
		//CurrCharacterType = StateCharacterType.Human;
		
		CurrZombType = StateZombieType.Fast;
		
		CurrAnimation = StateAnim.Standing;
		
		//isTargeting = false;
		
		isArmed = false;
		
		maxHealth = 8; // was 75;
		
		currHealth = maxHealth;
		
		maxResistance = 75;
		
		currResistance = maxResistance;
		
		baseAttackDamage = 2;
		
		timeSinceLastAttack = 0;
		
		speedPerSec = 2000.0f;
		
		
		if( CurrCharacterType == StateCharacterType.Human )
		{
		
			
			HumanoidHandler.ListOfHumans.Add (this);
		}
		else
		{
		
			
			HumanoidHandler.ListOfZombies.Add(this);
		}
		
	
	}
	
	
	
	//TODO: remove test()
	void TestingFun()
	{
		
		
	}// TestingFun()
	
	
	
	// Update is called once per frame
	void FixedUpdate () 
	{
	
		if( CurrCharacterType == StateCharacterType.Zombie)
		{
		
			if( isTargeting )
			{
				AnimateZombie();
			}
			else
			{
				// TODO: add function for handling AI according to UI
			}
		}
		else
		{
			AnimateHuman();
		}
		
		if( currHealth < 1 )
		{// remove self from the game
			if( CurrCharacterType == StateCharacterType.Human )
			{
				int thisID = transform.GetInstanceID();
				for( int i = 0; i < HumanoidHandler.ListOfHumans.Count; i++ )
				{
					if( thisID == HumanoidHandler.ListOfHumans[i].transform.GetInstanceID () )
					{
						HumanoidHandler.ListOfHumans.RemoveAt (i);
					}
				}
				// TODO: add random placement upon game over
				
				// if dead place self below ground
				if( transform.position.y > 0.0f )
				{
					transform.Translate ( new Vector3(0.0f, -16.0f - transform.position.y , 0.0f)
					                  , Space.World );
				}
			}
			else
			{// remove zombie self from zombie list
				int thisID = transform.GetInstanceID();
				for( int i = 0; i < HumanoidHandler.ListOfZombies.Count; i++ )
				{
					if( thisID == HumanoidHandler.ListOfZombies[i].transform.GetInstanceID () )
					{
						HumanoidHandler.ListOfZombies.RemoveAt (i);
					}
				}
			}
		}
	}// Update()
	
	
	
	
	
	void AnimateZombie()
	{
		
		switch( CurrAnimation )
		{
			case StateAnim.Running:
				{//TODO: Check if target moved
				 //  --> get path to new target position (switch to standing)
				 //Check if target reached 
				 //  --> switch to Attacking state
				 
					if( !animation.IsPlaying ("Run") )
					{
						animation.Play ("Run");
					}
					
					TranslateAndRotate ( 2 * speedPerSec);// set running speed & move
					
					if( TransOfTarget != null
						&& Vector3.Magnitude ( TransOfTarget.position - transform.position ) < 7.50f )
					{
					
						CurrAnimation = StateAnim.Attacking;
					
					}
					
					break;
				}
			case StateAnim.Walking:
				{//TODO: Check if target moved
				 //  --> get path to new target position (switch to standing)
				 //Check if target reached 
				 //  --> switch to Attacking state
				 
					if( !animation.IsPlaying ("Walk") )
					{
						animation.Play ("Walk");
					}
					
					TranslateAndRotate (speedPerSec);// set to walking speed & move
					
					if( TransOfTarget != null
						&& Vector3.Magnitude ( TransOfTarget.position - transform.position ) < 7.50f )
					{
						CurrAnimation = StateAnim.Attacking;
					
					}
					
					break;
				}
			case StateAnim.Attacking:
				{//TODO: Check if target moved
				 //  --> get path to new target position (switch to standing)
				 //else attack
				 
				//Debug.Log ("Entered Attacking");
					HumanoidHandler CurrTarg = null;
					
					if( !isArmed )
					{
					
					
						if( !animation.IsPlaying ("Hit") )
						{Debug.Log ("Entered Hit");
							animation.Play ("Hit");
						}
						
						CurrTarg = Attack();
					}// attack not armed
					else
					{
						
					}// attack armed
					
					if( CurrTarg != null
						&& CurrTarg.currHealth <= 0 )
					{
						CurrAnimation = StateAnim.Standing;
					}
					
					break;
			     }
			case StateAnim.Standing: // Character is Standing
			// Standing is where all aquisition of new targets 
			// or pathfinding to old, moved targets takes place
				{//TODO: if (hunting)
				 //   find nearest
				 //   if (found)
				 //     -->running/walking
				 // else play()
				 
				 
					if( CurrBehavior == StateCharacterBehave.AtkNearest 
						&& HumanoidHandler.ListOfHumans.Count > 0 )
					{// find nearest
						FindTarget ();
						
						// TODO: handle get/and wait for path to target
						seeker.StartPath( transform.position, TransOfTarget.position, SeekerSetPathToTargetCallBack );
						CurrAnimation = StateAnim.Walking;				
						
					}
					else
					{//Debug.Log ("Play Standing");
						animation.Play ("Standing");
						isTargeting = false;
					}
					
					break;
			    }
		}// body animation blocks
		
		
		
		
		//if ( isArmed )
		//{
		//	animation.Play ("HoldGun");
		//}
		
		
	}// AnimateZombie()
	
	
	
	
	
	void AnimateHuman()
	{
		
	}// AnimateHuman()
	
	/// <summary>
	/// Attacks and returns current target or null if attack fails.
	/// </summary>
	HumanoidHandler Attack()
	{
		HumanoidHandler TargetHumanoid = null;
		if( HumanoidHandler.ListOfHumans.Count > 0 )
		{
			// Find the current target as a HumanoidHandler
			for( int i = 0; i < HumanoidHandler.ListOfHumans.Count; i++)
			{
				if( HumanoidHandler.ListOfHumans[i].transform.GetInstanceID ()
					== TransOfTarget.GetInstanceID () )
				{
					TargetHumanoid = HumanoidHandler.ListOfHumans[i];
				}
			}
			
			// increment time since last attack to prevent doing damage every frame
			timeSinceLastAttack += Time.deltaTime;
			
			// If the target was still in the list (still alive)
			// and it's time to attack, then do damage
			if( TargetHumanoid != null 
				&& timeSinceLastAttack > .5f )
			{
				TargetHumanoid.currHealth -= baseAttackDamage;
				timeSinceLastAttack = 0f;
						
			//Debug.Log ("Entered Atk" + " health " + TargetHumanoid.currHealth);
					
			}
			
		}
		
		return TargetHumanoid;
	}
	
	
	
	/// <summary>
	/// Finds the nearest human.
	/// </summary>
	/// <returns>
	/// The nearest human.
	/// </returns>
	Transform FindNearestHuman()
	{
		if( HumanoidHandler.ListOfHumans.Count == 0 )
		{
			Debug.LogWarning ("Finding target when human list is empty");
			return null;
		}
		
		float minDist = 10000f;
		int index = -1;
		for(int i = 0; i < HumanoidHandler.ListOfHumans.Count; i++ )
		{
			if( Vector3.Magnitude ( 
				HumanoidHandler.ListOfHumans[i].transform.position
				 - transform.position )
				< minDist )
			{
				minDist = Vector3.Magnitude ( HumanoidHandler.ListOfHumans[i].transform.position
				                          - transform.position );
				index = i;
			}
			
		}
		
		return HumanoidHandler.ListOfHumans[index].transform;
	}// FindNearestHuman()
	
	
	
	/// <summary>
	/// Finds the target, the closest human.
	/// </summary>/
	void FindTarget()
	{// TODO: (Maybe, depending on pathfinder functionality) record initial target Pos, in place of using the unneeded local NewTarget variable
		Transform NewTarget = FindNearestHuman();
		TransOfTarget = NewTarget;
	}// FindTarget()
	
	
	/// <summary>
	/// A call back function called by the A* library once a path to the target is found.
	/// Sets the initial position of the target, the path to be followed, and
	/// the current path index.
	/// </summary>
	/// <param name='p'>
	/// P. passed in by the pathfinding library and is set as the path for this humanoid to walk.
	/// </param>
	void SeekerSetPathToTargetCallBack( Path p )
	{
		Debug.Log ( "Returned from Seeker" );
		if( !p.error )
		{
		initTargetPos = TransOfTarget.position;
		path = p;
		currPathPointIndex = 0;
		}
		else
		{
			Debug.Log( "Seeker Error: " + p.error );
		}
		
	}// Seeker CallBack()
	
	
	/// <summary>
	/// Translates and rotates the humanoid.
	/// </summary>
	/// <param name='speed'>
	/// Speed.the speed by which to translate.
	/// </param>
	void TranslateAndRotate(float speed )
	{		
		if( null == path )
		{//Debug.Log( "path == null-->no path yet" );
			return;
		}
		if( currPathPointIndex >= path.vectorPath.Count )
		{ 
			// end of path reached
			//TODO: handle reaching target when reaching end of path but not the target
			return;
		}
		
		
		// get direction to next Path point
		// use the XZ component to rotate then use this 3D point to translate
		Vector3 tmpDir = (path.vectorPath[currPathPointIndex] - transform.position ).normalized;
		tmpDir *=  speed * Time.deltaTime;
		
		// For rotating toward walkpoint, get angle between forward and target position
		// Get XZ forward dir of this Humanoid
		Vector2 tmpFwdXZ = new Vector2( transform.forward.x, transform.forward.z ).normalized;
		// Get 2D dir to target
		Vector2 tmpTargetXZ = new Vector2( tmpDir.x, tmpDir.z ).normalized;
		// Get angle between forward and target vectors to use in rotation
		float angle = Vector2.Angle (tmpTargetXZ,tmpFwdXZ);
		if( angle < 1.0f )
		{// add 180 to correct the angle
			angle += 180.0f;
		}
		// if x of the next walk point is negative turn left else turn right
		if( transform.InverseTransformPoint (path.vectorPath[currPathPointIndex]).x < 0.0f )
		{
			transform.Rotate( new Vector3(0f, 1f, 0f ), -angle * Time.deltaTime, Space.Self );
		}
		else
		{
			transform.Rotate( new Vector3(0f, 1f, 0f ), angle * Time.deltaTime, Space.Self );
		}
		controller.SimpleMove( tmpDir );
		
		
		// if close enough to next Path point
		// increment to the next Path point
		if( Vector3.Magnitude ( path.vectorPath[currPathPointIndex]  - transform.position )
			< 15.0f && currPathPointIndex < path.vectorPath.Count - 1 )//distToIncrementPathPoint )
		{Debug.Log ("incremented path index UBound: " + path.vectorPath.Count 
		+ " Curr Ind: " + currPathPointIndex );
			currPathPointIndex++;
		}
		//Debug.Log ( "Dir: " + tmpDir 
		//+ "Dist to next: " +Vector3.Magnitude ( path.vectorPath[currPathPointIndex]  - transform.position ) );
				
	}// TranslateAndRotate()
}// class HumanoidHandler








 

