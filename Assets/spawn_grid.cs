using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn_grid : MonoBehaviour {

	public GameObject[] tileTypes;
	public GameObject player;
	public float tileWidth;
	public float tileHeight;
	public int width;
	public int height;
	GameObject[,] tiles;
	public Vector3 prestpos;
	public Vector2 pcell;
	public Vector2 pdir = new Vector2(0,0);
	public Vector3 nextPos; 
	public Vector3 prevPos;
	float speed = .2f;

	public enum PlayerState {
		idle,
		moving,
		bumping
	}
	
	bool state_done = true;
	PlayerState cur_state = PlayerState.idle;

	float start_transition;
	float transition_time;



	// Use this for initialization
	void Start () {
		tiles = new GameObject[width, height];
		for (int x=0; x < width; x++) {
			for (int z=0; z < height; z++) {
				int tile_choice = Random.Range(0,tileTypes.Length);
				GameObject tile = tileTypes[tile_choice];
				Vector3 local = transform.localPosition;
				Vector3 tilePosition = new Vector3(
					local.x + ((float)x * tileWidth * 10) - (float)width/2 , 
					tileHeight, 
					local.z + ((float)z * tileWidth * 10) - (float)height/2 );
				GameObject new_tile = Instantiate(
					tile, 
					tilePosition,
					Quaternion.identity,
					transform.parent
				);
				tiles[x, z] = new_tile;
				if (tile_choice == 0) {
					player.transform.position = new Vector3(tilePosition.x, player.transform.position.y, tilePosition.z);
					pcell = new Vector2(x, z);
				}
			}
		}
		prestpos = player.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 direction = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));

		if (state_done) {
			if (direction == new Vector2(0,0)) {
				cur_state = PlayerState.idle;
			} else { // switch to move state
				int dx = direction.x == 0 ? 0 : (int)(direction.x/Mathf.Abs(direction.x));
				int dz = direction.y == 0 ? 0 : (int)(direction.y/Mathf.Abs(direction.y));
				int nx = (int)pcell.x + dx;
				int nz = (int)pcell.y + dz;
				GameObject nextCell = tiles[nx, nz];
				
				if (nextCell.transform.localScale.y > .5) { 
					cur_state = PlayerState.bumping;
				} else {
					cur_state = PlayerState.moving;
					pcell = new Vector3(nx, nz);
				}
				prevPos = player.transform.position;
				start_transition = 0f;

				nextPos = new Vector3(nextCell.transform.position.x, prestpos.y, nextCell.transform.position.z);
				
				state_done = false;

				pdir = new Vector2(dx, dz);
			}
		}
		DoState(cur_state);
	}

	void DoState (PlayerState state) {
		switch(state){
			case PlayerState.idle:
				p_idle();
				break;
			case PlayerState.moving:
				p_move();
				break;
			case PlayerState.bumping:
				p_bump();
				break;
		}
	}

	void p_move () {
		float mv_speed = speed;// * Time.deltaTime;
		Vector2 ppos = new Vector2(player.transform.position.x, player.transform.position.z);
		float dist = Vector2.Distance(ppos, new Vector2(nextPos.x, nextPos.z));
		//Debug.Log("dist: " + dist + ", speed: " + mv_speed + ", pdir: " + pdir);
		if (dist <= mv_speed) {
			start_transition = Time.time;
			player.transform.position = new Vector3(nextPos.x, prestpos.y, nextPos.z);
			cur_state = PlayerState.idle;
			return;
		}
		player.transform.position += new Vector3(pdir.x * mv_speed, 0, pdir.y * mv_speed);
	}

	void p_bump() {
		Vector2 ppos = new Vector2(player.transform.position.x, player.transform.position.z);
		float distAway = Vector2.Distance(ppos, new Vector2(prevPos.x, prevPos.z));
		float halfDist = Vector2.Distance(new Vector2(prevPos.x, prevPos.z), new Vector2(nextPos.x, nextPos.z))/2;
		if (distAway > halfDist) {
			if (nextPos != prevPos) {
				pdir = new Vector2(-pdir.x, -pdir.y);
			}
			nextPos = prevPos;
		}
		p_move();
	}

	void p_idle () {
		player.transform.position = new Vector3(player.transform.position.x,
												(Mathf.Sin(Time.time*12)+1)/6 + prestpos.y,
												player.transform.position.z);
		float now = Time.time;
		if (start_transition != 0f && (now - start_transition) > 0.2f) {
			state_done = true;
		}
	}

}



