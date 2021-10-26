using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform mainCam;
    public Transform thirdPersonCam;
    public Transform combatCam;
    public Animator anim;
    public GameObject gameManager;

    public DialogueConversation default_noTalking;
    public DialogueConversation default_noFighting;
    public DialogueConversation default_drawWeapon;
    public DialogueConversation default_notRightItem;


    public bool playerFrozen = false;
    public bool inCombatMode = false;

    float playerMoveSpeed = 1.5f;
    float playerRun = 7.0f;
    float playerWalk = 1.5f;
    float playerCombatSpeed = 2.0f;

    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private float gravity;
    [SerializeField] float gravityStrength = 1.0f;

    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> targetList = new List<GameObject>();
    public Transform target;
    public float targetRadius = 20f;
    public bool targetAvailable = false;
    bool targetButtonPressed = false;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SimulateGravity();
        TargetButton();

    }

    private void LateUpdate()
    {
        PlayerMovement();
        SwitchMode();
        SwitchBetweenTargets();
    }
    
    
    void PlayerMovement()
    {
        
        //Move the player, if the player is not frozen
        if (!playerFrozen)
        { 
            if (inCombatMode && targetAvailable)
            {
                CombatMovement();
                if(target == null)
                {
                    targetAvailable = false;
                }
            }
            else
            {
                FriendlyMovement();
            }
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
        
    }

    void FriendlyMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float run = Input.GetAxisRaw("Fire2");

        combatCam.gameObject.SetActive(false);
        thirdPersonCam.gameObject.SetActive(true);
        //Check if player is running or walking, then setting correct speed and animation
        if (horizontal != 0 || vertical != 0)
        {
            anim.SetBool("isMoving", true);
            //if (run > 0.002f)
            //{
            //    anim.SetBool("leftMouseKeyDown", false);
            //    playerMoveSpeed = playerWalk;
            //}
            //else
            //{
                anim.SetBool("leftMouseKeyDown", true);
                playerMoveSpeed = playerRun;
            //}
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        Vector3 playerDirection = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (playerDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * playerMoveSpeed * Time.deltaTime);
        }

    }

    void CombatMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        thirdPersonCam.gameObject.SetActive(false);
        combatCam.gameObject.SetActive(true);

        playerMoveSpeed = playerCombatSpeed;

        if (horizontal != 0 || vertical != 0)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
            FaceTarget();
        }

        Vector3 playerDirection = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (playerDirection.magnitude >= 0.1f)
        {
            FaceTarget();

            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * playerMoveSpeed * Time.deltaTime);
        }

        //stellt die Animationen für das "Strafe".
        float horizontalAnim = Input.GetAxisRaw("Horizontal");
        float verticalAnim = Input.GetAxisRaw("Vertical");
        anim.SetFloat("horizontalMovement", horizontalAnim);
        anim.SetFloat("verticalMovement", verticalAnim);
    }

    void SwitchMode()
    {
        // if the player is not frozen by clicking the button, the function switches between Combat-Mode and Adventure-Mode
        if (!playerFrozen)
        {
            if(Input.GetButtonDown("ModeSwitch"))
            {
                
                // Das if prüft, ob die Animations-Variable bereits true ist, der Spieler also gerade schon in der Kampfanimation ist.
                if(anim.GetBool("inCombatMode"))
                {
                    //setzt die CombatMode-Bool für die Animation zurück.
                    anim.SetBool("inCombatMode", false);
                    //INSERT: Waffe-einstecken-Animation wird hier als erstes abgespielt

                    //setzt mögliche Targets zurück und entferne die FoundSpheres
                    FindAllEnemys();
                    foreach(GameObject enemy in enemyList)
                    {
                        enemy.GetComponent<NPCStats>().foundSphere.SetActive(false);
                        enemy.GetComponent<NPCStats>().targetSphere.SetActive(false);
                    }
                    target = null;
                    targetAvailable = false;
                }
                else
                {       
                    //setzt die CombatMode_Bool für die Animation auf true.
                    //und sucht sich das nächste Ziel.
                    anim.SetBool("inCombatMode", true);
                    //INSERT: Waffe-ziehen-Animation wird hier als erstes abgespielt
                    TargetClosestEnemy();
                }
                //switched hier den CombatMode-Status
                inCombatMode = !inCombatMode;
                Debug.Log("Combat-Mode is" + inCombatMode);
            }
        }
    }

    void TargetButton()
    {        
        if (Input.GetButtonDown("Fire2") && inCombatMode)
        {
            if(!targetButtonPressed)
            {
                TargetClosestEnemy();
                targetButtonPressed = true;
                Debug.Log("Target closest enemy!");
            }
            else
            {
                target = null;
                targetAvailable = false;
                targetButtonPressed = false;
                Debug.Log("Untarget enemy.");
            }
        }
    }

    public void TargetClosestEnemy()
    {
        //nimmt sich das closestEnemy und setzt es als Ziel fest.
        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            //INSERT: Mark the closest enemy.
            target = closestEnemy.transform;
            Debug.Log(target + " ist das Ziel.");
            targetAvailable = true;
        }
        else
        {
            Debug.Log("Ich finde hier kein Ziel!");
            //INSERT: Audio.
        }
    }

    public GameObject FindClosestEnemy()
    {
        //Hier errechnet die Funktion, welche enemy aus der targetList am nahesten ist.
        //Dann gibt sie closestEnemy aus.
        FindEnemysInRange();
        GameObject closestEnemy = null;
        float distance = Mathf.Infinity;
        foreach (GameObject enemy in targetList)
        {
            Vector3 enemyPosition = enemy.transform.position;
            Vector3 diff = enemy.transform.position - transform.position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closestEnemy = enemy;
                distance = curDistance;
            }
        }
        if(closestEnemy != null)
        { 
            return closestEnemy;
        }
        else
        {
            return null;
        }
    }

    public void FindEnemysInRange()
    {
        // nimmt sich die enemyList aus "FindAllEnemys" und prüft dann, ob die enemys in Reichweite sind.
        // wenn sie das sind, speichert die Funktion sie in die targetList.
        FindAllEnemys();
        targetList.Clear();
        foreach (GameObject enemy in enemyList)
        {
            Vector3 enemyPosition = enemy.transform.position;
            float targetDistance = Vector3.Distance(enemyPosition, transform.position);
            if (targetDistance < targetRadius)
            {
                //Debug.Log(enemy + " ist nicht im Target-Radius");
                targetList.Add(enemy);
                enemy.GetComponent<NPCStats>().foundSphere.SetActive(true);
            }            
            //Debug.Log("TargetDistance ist " + targetDistance + "; Meine Position: " + this.transform.position + "; Enemys Position: " + enemyPosition);
        }
    }
    public void FindAllEnemys()
    {
        //sucht hier alle Enemys der Scene.
        enemyList.Clear();
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
    }

    public void SwitchBetweenTargets()
    {
        if (inCombatMode)
        {
            if(target == null)
            {
                return;
            }

            int i = targetList.IndexOf(target.gameObject);

                       
            if (Input.GetButtonDown("TargetUp"))
            {
                //Debug.Log("Current Enemy: " + i + "Switching to next enemy.");
                if (i == targetList.Count - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            if (Input.GetButtonDown("TargetDown"))
            {
                //Debug.Log("Switching to before enemy.");
                if (i == 0)
                {
                    i = targetList.Count - 1;
                }
                else
                {
                    i--;
                }
            }
            
            foreach(GameObject enemy in enemyList)
            {
                enemy.GetComponent<NPCStats>().targetSphere.SetActive(false);
            }
            target = targetList[i].transform;
            target.gameObject.GetComponent<NPCStats>().targetSphere.SetActive(true);
        }

    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }


    void SimulateGravity()
    {
        if (controller.isGrounded)
        {
            gravity = 0;
        }
        else
        {
            gravity -= gravityStrength * Time.deltaTime;
        }
        controller.Move(new Vector3(0, gravity, 0));
    }

}
