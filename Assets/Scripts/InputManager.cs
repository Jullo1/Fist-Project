using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    Player player;
    GameManager game;
    EventSystem eventSystem;
    int inputMode;

    List<Vector2> initialTouchPos = new List<Vector2>() { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    List<float> touchTimer = new List<float>() { 0, 0, 0, 0 };

    [SerializeField] PlayerInput mobileControls;
    [SerializeField] Canvas mobileControlsUI;
    public TouchStickVisual leftStick;
    [SerializeField] Image leftStickImage;
    [SerializeField] Image leftStickBackground;
    [SerializeField] Selectable invisibleButton;

    float inputX;
    float inputY;

    void Awake()
    {
        player = GetComponent<Player>();
        game = FindObjectOfType<GameManager>();
        eventSystem = FindObjectOfType<EventSystem>();
    }
    void Start()
    {
        if (!Application.isMobilePlatform && !Application.isEditor)
            mobileControlsUI.gameObject.SetActive(false); //hide onscreen controls
    }

    void Update()
    {
        inputMode = CheckInputMode();

        if (!player.freeze && !game.paused)
        {
            Vector2 mobileInput = mobileControls.actions["Move"].ReadValue<Vector2>(); //movement
            if ((Input.GetAxis("Horizontal") > 0.1f) || mobileInput.x > 0.40f) inputX = 1; //right
            else if ((Input.GetAxis("Horizontal") < -0.1f) || mobileInput.x < -0.50f) inputX = -1; //left
            if ((Input.GetAxis("Vertical") > 0.1f) || mobileInput.y > 0.40f) inputY = 1; //up
            else if ((Input.GetAxis("Vertical") < -0.1f) || mobileInput.y < -0.50f) inputY = -1; //down

            player.Move(inputX, inputY);
            inputX = 0; inputY = 0;

            player.channelingSpecial = false; //start with this value as false every frame

            if (Input.touchCount > 0) //touchscreen
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].phase == UnityEngine.TouchPhase.Began)
                    {
                        touchTimer[i] = 0;
                        initialTouchPos[i] = Input.touches[i].position;
                    }
                    touchTimer[i] += Time.deltaTime;

                    if (!player.freezeAttack)
                    {
                        if (touchTimer[i] > 0.2f && (initialTouchPos[i] - Input.touches[i].position).magnitude < 50 && Input.touches[i].position.x > 1500) //hold but not a swipe
                                player.ChannelSpecial(touchTimer[i]);

                        if (Input.touches[i].phase == UnityEngine.TouchPhase.Ended)
                            if (Input.touches[i].position.x > 1500 || Input.touchCount > 1) player.CheckAttack(); //this will only be called if not using the onscreen stick with this finger
                    }
                }
            }
            else if (!player.freezeAttack) //keyboard or controller
            {
                if (Input.GetKey(KeyCode.Space) || Input.GetButton("Attack")) player.ChannelSpecial();
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Attack")) player.CheckAttack();
            }
        }
        if (Input.GetKey(KeyCode.Escape) || Input.GetButton("Exit Game")) game.GameOver();
    }

    int CheckInputMode() // 0 touch screen ; 1 keyboard ; 2 controller
    {
        CheckNavigation();
        if (Input.touchCount > 0) 
            return 0;
        else if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W))
            return 1;
        else if (Input.GetButton("Attack"))
            return 2;
        else return inputMode;
    }

    void CheckNavigation()
    {
        if (inputMode == 2) //enable UI navigation with controller
            eventSystem.sendNavigationEvents = true;
        else
        {
            invisibleButton.Select();
            eventSystem.sendNavigationEvents = false;
        }
    }

    void ShowOnscreenControls(bool show, int opacity = 100)
    {
        if (show)
        {
            leftStickImage.color = new Color32(255, 255, 255, (byte)(opacity * 2));
            leftStickBackground.color = new Color32(0, 0, 0, (byte)opacity);
        }
        else
        {
            leftStickImage.color = new Color32(255, 255, 255, 0);
            leftStickBackground.color = new Color32(0, 0, 0, 0);
        }
    }
}
