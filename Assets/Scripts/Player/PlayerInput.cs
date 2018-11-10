using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

    private Player player;
    private PlayerRolling rollScript;
    private PlayerSwordAttack attackScript;

	// Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        rollScript = GetComponent<PlayerRolling>();
        attackScript = GetComponent<PlayerSwordAttack>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(KeyCode.Space)) {
            player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            player.OnJumpInputUp();
        }
        if (Input.GetButtonDown("Fire1")) {
            attackScript.OnRightClickInputDown();
        }
        if (Input.GetButtonDown("Fire2")) {
            rollScript.OnRightClickInputDown();
        }
        if (Input.GetButtonUp("Fire2")) {
            player.OnRightClickInputUp();
            rollScript.OnRightClickInputUp();
        }
    }
}
