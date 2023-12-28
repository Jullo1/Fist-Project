using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    Player player;
    GameManager game;

    public bool usingMobileControls;
    List<Vector2> initialTouchPos = new List<Vector2>() { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    List<float> touchTimer = new List<float>() { 0, 0, 0, 0 };

    [SerializeField] PlayerInput mobileControls;
    [SerializeField] Canvas mobileControlsUI;
    public TouchStickVisual leftStick;
    [SerializeField] Image leftStickImage;
    [SerializeField] Image leftStickBackground;

    float inputX;
    float inputY;

    void Awake()
    {
        player = GetComponent<Player>();
        game = FindObjectOfType<GameManager>();
    }
    void Start()
    {
        if (!Application.isMobilePlatform && !Application.isEditor)
            mobileControlsUI.gameObject.SetActive(false); //hide onscreen controls
    }

    void Update()
    {
        if (!player.freeze && !game.paused)
        {
            Vector2 mobileInput = mobileControls.actions["Move"].ReadValue<Vector2>(); //movement
            if ((Input.GetAxis("Horizontal") > 0.1f) || mobileInput.x > 0.45f) inputX = 1; //right
            else if ((Input.GetAxis("Horizontal") < -0.1f) || mobileInput.x < -0.45f) inputX = -1; //left
            if ((Input.GetAxis("Vertical") > 0.1f) || mobileInput.y > 0.45f) inputY = 1; //up
            else if ((Input.GetAxis("Vertical") < -0.1f) || mobileInput.y < -0.45f) inputY = -1; //down

            player.Move(inputX, inputY);
            inputX = 0; inputY = 0;

            player.channelingSpecial = false; //start with this value as false every frame

            if (Input.touchCount > 0) //touchscreen
            {
                usingMobileControls = true;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].phase == UnityEngine.TouchPhase.Began)
                    {
                        touchTimer[i] = 0;
                        initialTouchPos[i] = Input.touches[i].position;
                    }
                    touchTimer[i] += Time.deltaTime;

                    if (touchTimer[i] > 0.2f && (initialTouchPos[i] - Input.touches[i].position).magnitude < 50) //hold but not a swipe
                        player.ChannelSpecial();

                    if (Input.touches[i].phase == UnityEngine.TouchPhase.Ended)
                        if (Input.touches[i].position.x > 1500 || Input.touchCount > 1) player.CheckAttack(); //this will only be called if not using the onscreen stick with this finger
                }
                if (!player.channelingSpecial) //check if special was not used after going through the for loop
                    player.specialChannel = 0.2f; //specialChannel begins at 0.2 for mobile, due to the 0.2 hold check
            }
            else if (!usingMobileControls) //keyboard or controller
            {
                if (Input.GetKey(KeyCode.Space) || Input.GetButton("Attack")) player.ChannelSpecial();
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Attack")) player.CheckAttack();
            }
        }
        if (Input.GetKey(KeyCode.Escape) || Input.GetButton("Exit Game")) game.GameOver();
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
