using UnityEngine;
using System.Collections;
public class CharacterInputController2 : MonoBehaviour
{
	public GameObject player;
	
    static int s_DeadHash = Animator.StringToHash ("Dead");
	static int s_RunStartHash = Animator.StringToHash("runStart");
	static int s_MovingHash = Animator.StringToHash("Moving");
	static int s_JumpingHash = Animator.StringToHash("Jumping");
	static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
	static int s_SlidingHash = Animator.StringToHash("Sliding");
	private int cur_slide_length = 0;
	private int cur_jump = 0;
	private float cur_jumpheight = 0.0f;
	public int speed=50;
	public Animator animator;
	public GameObject character;
	public GameObject blobShadow;
	public float laneChangeSpeed = 1.0f;
	public float lanewidth = 5.0f;
	public float groundzero = 0.0f;
	public TMPro.TMP_Text scoretext;
	private int curScore = 0;


	public bool isJumping { get { return m_Jumping; } }
	public bool isSliding { get { return m_Sliding; } }

	[Header("Controls")]
	public float jumpLength = 2.0f;     // Distance jumped
	public float jumpHeight = 1.2f;

	public float slideLength = 2.0f;

	[Header("Sounds")]
	public AudioClip slideSound;
	public AudioClip jumpSound;
	public AudioClip powerUpUseSound;
	public AudioSource powerupSource;

   
    

	protected bool m_IsInvincible;
	protected bool m_IsRunning;
	
    protected float m_JumpStart;
    protected bool m_Jumping;
	private bool m_Grounding;
	protected bool m_Sliding;
	protected float m_SlideStart;

	protected AudioSource m_Audio;

    protected int m_CurrentLane = k_StartingLane;
    protected Vector3 m_TargetPosition = Vector3.zero;
	Vector3 curlane = new Vector3();
	//protected readonly Vector3 k_StartingPosition = Vector3.forward * 2f;

	protected const int k_StartingLane = 1;
    protected const float k_GroundingSpeed = 80f;
    protected const float k_ShadowRaycastDistance = 100f;
    protected const float k_ShadowGroundOffset = 0.01f;
	public SwipeDetector sd;
	protected void Awake ()
    {
		Application.targetFrameRate = 120;
		curlane= player.transform.position;
		//m_TargetPosition.y = 1;
		sd.upaction = inputup;
		sd.downaction = inputdown;
		sd.leftaction = inputleft;
		sd.rightaction = inputright;

		blobShadow.transform.position = new Vector3(curlane.x, groundzero + k_ShadowGroundOffset, curlane.z);
		m_Sliding = false;
        m_SlideStart = 0.0f;
	    m_IsRunning = true; // transform.position = m_TargetPosition;

		m_CurrentLane = k_StartingLane;
		

      

		m_Audio = GetComponent<AudioSource>();
m_IsRunning = false;
        animator.SetBool(s_DeadHash, false);

		  if (animator)
        {
            animator.Play(s_RunStartHash);
            animator.SetBool(s_MovingHash, true);
			m_IsRunning = true;
        }

    }
	
    private void FixedUpdate()
    {
		scoretext.text ="Score: " + curScore.ToString();
		curlane.y = cur_jumpheight;
		//Debug.Log("curlane y : " + curlane.y + m_Grounding + m_Jumping);
		player.transform.position=Vector3.MoveTowards(player.transform.position, curlane, laneChangeSpeed*Time.deltaTime);
		//this.transform.Translate(curlane, laneChangeSpeed*Time.deltaTime);
	}
    // Cheating functions, use for testing
    public void CheatInvincible(bool invincible)
	{
		m_IsInvincible = invincible;
    }

	public bool IsCheatInvincible()
	{
		return m_IsInvincible;
	}

    public void Init()
    {
      
	
    }

	// Called at the beginning of a run or rerun
	public void Begin()
	{
		
	
	}

    public void StartRunning()
    {   
	   
      
    }

	

    public void StopMoving()
    {
	    m_IsRunning = false;
      
        if (animator)
        {
            animator.SetBool(s_MovingHash, false);
        }
    }
	//collisions
	private void OnTriggerEnter(Collider other)
	{
		if (!m_Sliding)
		if (other.gameObject.tag == "Collectable") 
		{
			if (other.gameObject.name.Contains("Pickup"))
            {
				curScore += 10;
					m_Audio.PlayOneShot(powerUpUseSound);
				}
			if (other.gameObject.name.Contains("Cube"))
			{
				curScore -= 50;
					m_Audio.PlayOneShot(jumpSound);
				}
			other.gameObject.SetActive(false);
		
		}
	}
    private void inputup()
    {
		if (!m_Jumping)
			Jump();

	}
	private void inputdown()
	{
		if (!m_Jumping)
			if (!m_Sliding)
				Slide();

	}
	private void inputleft()
	{
		if (!m_Jumping)
			ChangeLane(-1);

	}
	private void inputright()
	{
		if (!m_Jumping)
			ChangeLane(1);

	}


	protected void Update ()
    {

        // Use key input in editor or standalone
        // disabled if it's tutorial and not thecurrent right tutorial level (see func TutorialMoveCheck)

        if (Input.GetKeyDown(KeyCode.LeftArrow) )
        {
			if (!m_Jumping)
				ChangeLane(-1);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
			if (!m_Jumping)
				ChangeLane(1);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow) )
        {
			if (!m_Jumping)
            Jump();
        }
		else if (Input.GetKeyDown(KeyCode.DownArrow) )
		{
			if (!m_Jumping)
				if (!m_Sliding)
				Slide();
		}


       // Vector3 verticalTargetPosition = m_TargetPosition;

		if (m_Sliding)
		{
			
			
			if (cur_slide_length >= slideLength)
			{
                // We slid to (or past) the required length, go back to running
				StopSliding();
			}
			cur_slide_length++;
		}

        if(m_Jumping && !m_Grounding)
        {
		
                // Same as with the sliding, we want a fixed jump LENGTH not fixed jump TIME. Also, just as with sliding,
                // we slightly modify length with speed to make it more playable.
			cur_jump++;
			if (cur_jump >  jumpLength)
			{

				m_Grounding = true;
				cur_jumpheight = groundzero;
				cur_jump = 0;

			}
			
			


		}
		if (m_Grounding)
        {
			if (player.transform.position.y < groundzero+0.1f)
						{StopJumping();
					
						}	
        }

        //characterCollider.transform.localPosition = Vector3.MoveTowards(transform.localPosition, 0, laneChangeSpeed * Time.deltaTime);

       
	}

    public void Jump()
    {
	    if (!m_IsRunning)
		    return;
	    
        if (!m_Jumping)
        {
			if (m_Sliding)
				StopSliding();

			cur_jump = 0;
			float animSpeed = 1 / speed;
			cur_jumpheight= jumpHeight;
            animator.SetFloat(s_JumpingSpeedHash, animSpeed);
            animator.SetBool(s_JumpingHash, true);
			m_Audio.PlayOneShot(jumpSound);
			m_Jumping = true;
			m_Grounding = false;
		}
    }

    public void StopJumping()
    {
        if (m_Jumping)
        {
			m_Grounding = false;
			m_Jumping = false;
				
			animator.SetBool(s_JumpingHash, false);
           
        }
    }

	public void Slide()
	{
		if (!m_IsRunning)
			return;
		
		if (!m_Sliding)
		{

		    if (m_Jumping)
		        StopJumping();

			player.transform.rotation = Quaternion.AngleAxis(45, Vector3.right);
			float animSpeed = 1/ speed;
			animator.SetFloat(s_JumpingSpeedHash, animSpeed);
			animator.SetBool(s_SlidingHash, true);
			m_Audio.PlayOneShot(slideSound);
			m_Sliding = true;
		}
	}

	public void StopSliding()
	{
		if (m_Sliding)
		{
			player.transform.rotation = Quaternion.AngleAxis(0, Vector3.right);
			animator.SetBool(s_SlidingHash, false);
			m_Sliding = false;
			cur_slide_length = 0;
		}
	}

	public void ChangeLane(int direction)
    {
		if (!m_IsRunning)
			return;

        int targetLane = m_CurrentLane + direction;

        if (targetLane < 0 || targetLane > 2)
            // Ignore, we are on the borders.
            return;
		
        m_CurrentLane = targetLane;
		if (targetLane == 0)
			curlane = new Vector3(-lanewidth,cur_jumpheight, 0);
		else if (targetLane == 1)
			curlane = new Vector3(0, cur_jumpheight, 0);
		else if (targetLane == 2)
			curlane = new Vector3( lanewidth, cur_jumpheight, 0);
	}

   

}
