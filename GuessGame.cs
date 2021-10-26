using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GuessGame : MonoBehaviour
{
    public int maxNumber;
    private int yourGuess;
    private bool gameSolved = false;
    public Item reward;
    Inventory inventory;
    private bool coolOff = false;
    private float coolOffTime = 10.0f;

    [SerializeField] private int secretNumber;
    public DialogueConversation[] conversations;

    private int timesGuessed = 0;
    private int possibleGuesses = 3;
    private int leftGuesses;

    GameObject greetingPanel;
    GameObject gamePanel;
    GameObject endPanel;
    Animator anim;

    GameManager gameManager;

    public EventSystem eventSystem;
    public TMP_Text gamePanelText;
    public TMP_Text endPanelText;
    public InputField inputField;
    public GameObject yesButton;
    public GameObject stopButton;
    public GameObject numberOneButton;

    
    bool isPlayerThere = false;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        secretNumber = Random.Range(1, maxNumber + 1);
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        SetPanelsReferences();
        greetingPanel.SetActive(false);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);
        PushButton.ButtonPressed += AddDigitToInputField;
        inventory = Inventory.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && isPlayerThere && !playerController.playerFrozen && !gameSolved && !coolOff)
        {
            greetingPanel.SetActive(true);
            playerController.playerFrozen = true;
            eventSystem.gameObject.SetActive(true);
            eventSystem.SetSelectedGameObject(yesButton);
            
            //GreetingPanel wird mittels Buttons beendet, die hier nicht im Script stehen.
        }
        SetCorrectDialogue();
    }
    
    public void StartGame()
    {
        greetingPanel.SetActive(false);
        Time.timeScale = 0;
        gamePanel.SetActive(true);
        eventSystem.SetSelectedGameObject(numberOneButton);
        if (gameManager.language == "de")
        {
            gamePanelText.text = ("Nenne eine Zahl zwischen 1 und " + maxNumber + ". Du hast drei Versuche, um auf die Lösung zu kommen. Viel Glück!");   
        }
        else
        {
            gamePanelText.text = ("Tell me a number between 1 and " + maxNumber + ". You may guess three times. Good luck!");           
        }
        
    }

    public void CompareNumbers()
    {
        yourGuess = int.Parse(inputField.text);
        
            if (yourGuess == secretNumber)
            {
                gameSolved = true;
                gamePanel.SetActive(false);
                endPanel.SetActive(true);
                eventSystem.SetSelectedGameObject(stopButton);
                if (gameManager.language == "de")
                    endPanelText.text = "Ja, richtig! Hier ... ich habe diesen Schlüssel im Heuhaufen gefunden. Vielleicht hilft er dir?";
                else
                    endPanelText.text = "Yes, right! Here ... I found this key in the hay stack. Maybe it is of use for you?";
            }
            else
            {
                inputField.text = "";
                timesGuessed++;
                if (timesGuessed == possibleGuesses)
                {
                    timesGuessed = 0;
                    inputField.text = "";
                    gamePanel.SetActive(false);
                    endPanel.SetActive(true);
                    eventSystem.SetSelectedGameObject(stopButton);
                    if (gameManager.language == "de")
                    {
                        endPanelText.text = "Jetzt ist auch genug. Du weißt es einfach nicht. Versuche es später noch einmal.";
                    }
                    else
                    {
                        endPanelText.text = "Enough! You just do not know. Try again later.";
                    }
                    StartCoroutine(CoolOffCounter());
                }
                else
                {
                    leftGuesses = possibleGuesses - timesGuessed;

                    switch ((int)Mathf.Sign(yourGuess - secretNumber))
                    {
                        case 1:
                            if (gameManager.language == "de")
                                gamePanelText.text = ("Leider zu hoch. Du darfst noch " + leftGuesses + " Mal raten.");
                            else
                                gamePanelText.text = ("Sorry, too high. " + leftGuesses + " guesses left.");
                            break;
                        case -1:
                            if (gameManager.language == "de")
                                gamePanelText.text = ("Leider zu gering. Du darfst noch " + leftGuesses + " Mal raten.");
                            else
                                gamePanelText.text = ("Sorry, too low." + leftGuesses + " guesses left.");
                            break;
                    }
                }
            }

        
    }

    public void StopGame()
    {
        
        playerController.playerFrozen = false;
        greetingPanel.SetActive(false);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);
        Time.timeScale = 1;

        if (gameSolved)
        {
            GameObject.Find("QuestMark_GuessGame").SetActive(false);
            anim.SetBool("gameSolved", true);
            //give reward to the player
            inventory.AddItem(reward);
            //INSERT: Sound *chaching*
        }
        else 
        {
            secretNumber = Random.Range(1, maxNumber + 1);
        }
        SetCorrectDialogue();      
    }

    private void SetPanelsReferences()
    {
        greetingPanel = GameObject.Find("GreetingPanel");
        gamePanel = GameObject.Find("GamePanel");
        endPanel = GameObject.Find("EndPanel");
        anim = gameObject.GetComponent<Animator>();
    }

    private void AddDigitToInputField(string digitEntered)
    {
        switch (digitEntered)
        {
            case "*":
                inputField.text = "";
                break;

            case "Guess":
                CompareNumbers();
                break;

            default:
                inputField.text += digitEntered;
                Debug.Log(digitEntered);
                break;
        }
    }

    private IEnumerator CoolOffCounter()
    {
        coolOff = true;
        anim.SetBool("coolOff", true);
        yield return new WaitForSeconds(coolOffTime);
        coolOff = false;
        anim.SetBool("coolOff", false);
    }

    void SetCorrectDialogue()
    {
        if(gameSolved || coolOff)
        {
            gameObject.GetComponent<EnemyInteraction>().thisConversation = conversations[1];
        }
        else
        {
            gameObject.GetComponent<EnemyInteraction>().thisConversation = conversations[0];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayerThere = true;

        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayerThere = false;

        }
    }
}
