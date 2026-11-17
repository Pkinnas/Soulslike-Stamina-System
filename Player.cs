using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Player : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject fire_vfx;


    [Header("Camera Impulse")]
    [SerializeField] private CinemachineImpulseSource impulse;


    [SerializeField] private float player_velocity, rotation_speed, sprint_velocity, backwards_velocity, side_walk_velocity, 
        stamina_regeneration_value, stamina_sprint_consumption_value, attack_slide_value, impulse_value, stamina_attack_A, stamina_attack_B, stamina_attack_C,
        yellow_stamina_consumption_value, stamina_charged_attack;

    [Header("Player SFX")]
    [SerializeField] private AudioSource sprint_sound;
    [SerializeField] private AudioSource sword_swing_sound;
    [SerializeField] private AudioSource sword_whoosh_sound;
    [SerializeField] private AudioSource fire_spell_sound;
    [SerializeField] private AudioSource charged_attack_sound;

    [Header("HUD")]
    [SerializeField] private Slider player_stamina, player_stamina_yellow;
    
    private Rigidbody player_body;
    private Animator player_animator;

    private float original_velocity, combo_timer;

    private bool player_is_grounded, has_stopped_sprinting;

    private bool recharge, player_is_attacking, player_is_attacking_with_spell, start_combo_timer, player_attack_B, player_attack_C,
        yellow_stamina_reduction;

    private void Awake()
    {
        original_velocity = player_velocity;
    }

    private void Start()
    {
        Cursor.visible = false;

        player_body = gameObject.GetComponent<Rigidbody>();
        player_animator = gameObject.GetComponent<Animator>();

        has_stopped_sprinting = true;
        
    }


    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        // reduce yellow bar
        if (yellow_stamina_reduction)
            if (player_stamina_yellow.value > player_stamina.value)
                player_stamina_yellow.value -= yellow_stamina_consumption_value * Time.deltaTime;


        // check if player stopped sprinting
        if (has_stopped_sprinting)
        {
            player_animator.SetBool("isSprinting", false);            
            player_velocity = original_velocity;

            // regenerate stamina if its value reaches 0
            if (recharge && !player_is_attacking)
            {
                if (player_stamina_yellow.value > player_stamina.value)
                {
                    // reduce yellow bar
                    player_stamina_yellow.value -= yellow_stamina_consumption_value * Time.deltaTime;
                    yellow_stamina_reduction = true;
                }


                else
                {
                    // start regenerating when the yellow bar reaches the green bar
                    player_stamina.value += stamina_regeneration_value * Time.deltaTime;
                    player_stamina_yellow.value += stamina_regeneration_value * Time.deltaTime;
                    yellow_stamina_reduction = false;
                }

                if (player_stamina.value == player_stamina.maxValue || Input.GetKeyDown(KeyCode.LeftShift))
                    recharge = false;
            }
            else if (!player_is_attacking)
            {
                // regenerate stamina
                if (player_stamina_yellow.value > player_stamina.value)
                {
                    player_stamina_yellow.value -= yellow_stamina_consumption_value * Time.deltaTime;
                    yellow_stamina_reduction = true;
                }


                else
                {
                    yellow_stamina_reduction = false;
                    player_stamina.value += stamina_regeneration_value * Time.deltaTime;
                    player_stamina_yellow.value += stamina_regeneration_value * Time.deltaTime;
                }
            }
            
        }

                   
        if (Input.GetKey(KeyCode.W) && player_is_grounded && !player_is_attacking && !player_is_attacking_with_spell)
        {
            // move forward
            this.transform.position += transform.forward * player_velocity * Time.deltaTime;
            player_animator.SetBool("isRunning", true);

            // start sprinting
            if (Input.GetKey(KeyCode.LeftShift) && player_stamina.value > 0f && !recharge && player_stamina_yellow.value <= player_stamina.value)
            {
                player_velocity = sprint_velocity;
                has_stopped_sprinting = false;
                player_animator.SetBool("isSprinting", true);
                player_stamina.value -= stamina_sprint_consumption_value * Time.deltaTime;
                player_stamina_yellow.value -= stamina_sprint_consumption_value * Time.deltaTime;

                // check if stamina is 0
                if (player_stamina.value == 0)
                {
                    has_stopped_sprinting = true;
                    recharge = true;
                }

            }

        }
        // walk backwards
        else if (Input.GetKey(KeyCode.S) && player_is_grounded && !player_is_attacking && !player_is_attacking_with_spell)
        {
            this.transform.position -= transform.forward * backwards_velocity * Time.deltaTime;
            player_animator.SetBool("isWalkingBackwards", true);
        }

        // sidewalk left
        else if (Input.GetKey(KeyCode.A) && player_is_grounded && !player_is_attacking && !player_is_attacking_with_spell)
        {
            this.transform.position += transform.right * -1 * side_walk_velocity * Time.deltaTime;
            player_animator.SetBool("isWalkingLeft", true);
            player_animator.SetBool("isWalkingRight", false);
        }
        // sidewalk right
        else if (Input.GetKey(KeyCode.D) && player_is_grounded && !player_is_attacking && !player_is_attacking_with_spell)
        {
            this.transform.position += transform.right * side_walk_velocity * Time.deltaTime;
            player_animator.SetBool("isWalkingRight", true);
            player_animator.SetBool("isWalkingLeft", false);
        }

        // Player Attacks
        if (Input.GetKeyDown(KeyCode.Mouse0) && has_stopped_sprinting && player_stamina.value > 10)
        {
            // Attack A
            player_animator.SetBool("Attack_A", true);
            player_is_attacking = true;

            // start the combo timer
            start_combo_timer = true;

            if (combo_timer < 1.5f  && player_attack_B)
            {
                // Attack B
                player_animator.SetBool("Attack_A", false);
                player_animator.SetBool("Attack_B", true);

            }
            else if (combo_timer < 2.5f && player_attack_C)
            {
                // Attack C
                player_animator.SetBool("Attack_A", false);
                player_animator.SetBool("Attack_B", false);
                player_animator.SetBool("Attack_C", true);

            }
        }

        else if (Input.GetKeyDown(KeyCode.Mouse2) && has_stopped_sprinting && !player_is_attacking)
        {
            // Cast fire spell
            player_animator.SetBool("FireSpellCast", true);
            player_is_attacking_with_spell = true;
        }

        else if (Input.GetKey(KeyCode.Mouse0) && has_stopped_sprinting)
        {
            if (player_stamina.value > 20)
            {
                // Charged attack
                player_animator.SetBool("ChargedAttack", true);
                player_is_attacking = true;
            }
        }


        if (start_combo_timer)
        {
            // count
            combo_timer += Time.deltaTime;
            if (combo_timer > 2.5f)
            {
                // stop the combo timer
                start_combo_timer = false;
                combo_timer = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.D))
            player_animator.SetBool("isWalkingRight", false);


        if (Input.GetKeyUp(KeyCode.A))
            player_animator.SetBool("isWalkingLeft", false);


        if (Input.GetKeyUp(KeyCode.S))
            player_animator.SetBool("isWalkingBackwards", false);


        if (Input.GetKeyUp(KeyCode.W))
        {
            player_animator.SetBool("isRunning", false);
            player_animator.SetBool("isSprinting", false);
        }
        
        // rotate camera right
        if (Input.GetKey(KeyCode.E) && !player_is_attacking && !player_is_attacking_with_spell)
                this.transform.Rotate(0, rotation_speed * Time.deltaTime, 0);


        // rotate camera left
        if (Input.GetKey(KeyCode.Q) && !player_is_attacking && !player_is_attacking_with_spell)
                this.transform.Rotate(0, -rotation_speed * Time.deltaTime, 0);


        if (Input.GetKeyUp(KeyCode.LeftShift))
            has_stopped_sprinting = true;


        if (Input.GetKeyUp(KeyCode.Mouse0))
            player_animator.SetBool("ChargedAttack", false);

    }
    
    private void OnTriggerStay(Collider other)
    {
        // check if player is on ground
        if (other.CompareTag("ground"))
        {
            player_is_grounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ground"))
            player_is_grounded = false;
    }


    // Methods -------------------------------------------------

    // Audio
    public void play_sprint_sound()
    {
        sprint_sound.Play();
    }
    
    public void play_sword_sound()
    {
        sword_swing_sound.Play();
    }

    public void play_whoosh_sound()
    {
        sword_whoosh_sound.Play();
    }

    public void play_charged_attack_sound()
    {
        charged_attack_sound.Play();
    }

    public void fire_sound()
    {
        fire_spell_sound.Play();
    }

    // Stop attack animations
    public void stop_attack_A()
    {
        player_animator.SetBool("Attack_A", false);
        player_is_attacking = false;
        player_attack_B = false;
    }

    public void stop_attack_B()
    {
        player_animator.SetBool("Attack_A", false);
        player_animator.SetBool("Attack_B", false);
        player_is_attacking = false;
        player_attack_B = false;
        player_attack_C = false;
    }

    public void stop_attack_C()
    {
        player_animator.SetBool("Attack_C", false);
        player_is_attacking = false;
        player_attack_C = false;
    }

    public void stop_charged_attack()
    {
        player_animator.SetBool("ChargedAttack", false);
        player_is_attacking = false;
    }

    // start attack animations
    public void start_attack_B()
    {
        player_animator.SetBool("Attack_A", false);
        player_attack_B = true;
    }

    public void start_attack_C()
    {
        player_attack_B = false;
        player_attack_C = true;
    }

    // consume stamina
    public void consume_player_stamina_A()
    {
        player_stamina.value -= stamina_attack_A;
    }

    public void consume_player_stamina_B()
    {
        player_stamina.value -= stamina_attack_B;
    }

    public void consume_player_stamina_C()
    {
        player_stamina.value -= stamina_attack_C;
    }

    public void consume_player_stamina_charged_attack()
    {
        player_stamina.value -= stamina_charged_attack;
    }

    // push the player forward after each attack
    public void move_player_on_attack()
    {
        player_body.AddForce(transform.forward * attack_slide_value);
    }

    // fire spell vfx
    public void enable_fire_vfx()
    {
        fire_vfx.SetActive(true);
    }

    public void stop_fire_spell()
    {
        player_animator.SetBool("FireSpellCast", false);
        player_is_attacking_with_spell = false;
        fire_vfx.SetActive(false);
    }

    // impulse on camera
    public void generate_impulse()
    {
        if (player_animator.GetBool("FireSpellCast"))
            impulse.GenerateImpulse(impulse_value * 3);
        else
            impulse.GenerateImpulse(impulse_value);
    }
}
