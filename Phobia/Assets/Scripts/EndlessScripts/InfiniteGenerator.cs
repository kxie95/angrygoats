﻿using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using System.Linq;
using System;

/**
 * Class which handles infinite level generation. Refer to level gen script for specifics,
 * only infinite changes are commented here.
 **/
public class InfiniteGenerator : MonoBehaviour {
	
	public int roomsToSpawn = 5;
	public GameObject roomPrefab;
	public GameObject startPrefab;
	public List<GameObject> floorPrefabs;
	
	public int totalEnemies = 15;
	public int maxEnemiesPerRoom = 4;
	public List<GameObject> enemies;
	public List<int> enemyCommonness;
	
	
	public GameObject bossFloor;
	public GameObject boss;
	
	public int minWebs;
	public int maxWebs;
	public GameObject web;

	//Reference to the ramp object present in the scene, used for value extraction.
	public GameObject r;
	
	public GameObject trash;
	
	Dictionary<Vector2,RoomControl> roomsDict = new Dictionary<Vector2,RoomControl>();
	
	public GameObject minimapUI;
	
	public AstarPath aStarGrids;
	
	
	void Start () {
		
		// These variables will change their meaning over the course of the method
		RoomControl thisRoom;
		RoomControl adjRoom;
		RoomControl chosenRoom;
		Vector2 roomVector;
		Vector2 roomVectorRel;
		Ramp r2 = r.GetComponent<Ramp> (); 
		double ramp = Math.Pow(r2.ramp, r2.times);

		// All level components are scaled based on the current required difficulty.
		// Debugging added to aid testing.

		Debug.Log ("We r ramp!");

		double temp;
		temp = (double)roomsToSpawn * ramp;
		roomsToSpawn = (int)temp;
		temp = (double)totalEnemies * ramp;
		totalEnemies = (int)temp;
		temp = (double)maxEnemiesPerRoom * ramp;
		maxEnemiesPerRoom = (int)temp;

		Debug.Log ("Pls make me " + roomsToSpawn.ToString () + " rooms kthanx");
		Debug.Log ("Ramp was " + ramp.ToString () + ". Hopefully that's what you expected.");


		// Reference the minimap, so it can be generated in unison with the actual floor
		MinimapScript minimapScript = minimapUI.GetComponent<MinimapScript>();
		
		// Start room
		
		thisRoom = ((GameObject)Instantiate(roomPrefab, new Vector3(0, 0, 0), Quaternion.identity)).GetComponent<RoomControl>();
		thisRoom.minimapUI = minimapUI;
		thisRoom.Floor = startPrefab; // pick set floor
		
		thisRoom.Index = new Vector2(0,0);
		minimapScript.GenerateMapBlock(thisRoom.Index);
		
		roomsDict.Add(thisRoom.Index,thisRoom);
		
		thisRoom.gameObject.name = "Room " + 0;
		thisRoom.spawnEnemies = false;
		thisRoom.spawnWebs = false;
		
		thisRoom.gameObject.transform.Find("Lights").gameObject.SetActive(true);
		
		// normal rooms
		
		for (int i = 1; i < roomsToSpawn-1; i++) {
			
			int a = 0;
			do {
				adjRoom = RandomRoom();
				if (a > 1000) {
					throw new System.Exception("Infinite loop occuring.");
				}
				a++;
			} while (!IsNextToEmpty(adjRoom));
			
			roomVectorRel = RandomEmpty(adjRoom);
			roomVector = adjRoom.Index + roomVectorRel;
			
			thisRoom = ((GameObject)Instantiate(roomPrefab, RoomControl.IndexToPosition(roomVector), Quaternion.identity)).GetComponent<RoomControl>();
			thisRoom.minimapUI = minimapUI;
			thisRoom.Floor = floorPrefabs[UnityEngine.Random.Range(0, floorPrefabs.Count)];
			thisRoom.Index = roomVector;
			minimapScript.GenerateMapBlock(thisRoom.Index);
			thisRoom.gameObject.name = "Room " + i;
			
			SetAdj(adjRoom, roomVectorRel, thisRoom);
			
			roomsDict.Add(thisRoom.Index,thisRoom);
		}
		
		// boss room
		
		int b = 0;
		do {
			adjRoom = RandomRoom();
			if (b > 1000) {
				throw new System.Exception("Infinite loop occuring.");
			}
			b++;
		} while (!IsNextToEmpty(adjRoom));
		
		roomVectorRel = RandomEmpty(adjRoom);
		roomVector = adjRoom.Index + roomVectorRel;
		
		thisRoom = ((GameObject)Instantiate(roomPrefab, RoomControl.IndexToPosition(roomVector), Quaternion.identity)).GetComponent<RoomControl>();
		thisRoom.minimapUI = minimapUI;
		thisRoom.Floor = bossFloor;
		thisRoom.Index = roomVector;
		minimapScript.GenerateMapBlock(thisRoom.Index);
		thisRoom.gameObject.name = "Boss Room";
		
		SetAdj(adjRoom, roomVectorRel, thisRoom);
		
		roomsDict.Add(thisRoom.Index,thisRoom);
		
		if (boss.transform.name == "Boss3") {
			int temp1 = UnityEngine.Random.Range (-18, 18);
			int temp2 = UnityEngine.Random.Range (-10, 10);
			boss = (GameObject)Instantiate (boss, thisRoom.transform.position + new Vector3 (temp1, 2, temp2), Quaternion.identity);
			BossThree bs3 = boss.GetComponent<BossThree> ();
			bs3.roomCont = thisRoom.GetComponent<RoomControl> ();
		} else if (boss.transform.name == "FlyBoss" || boss.transform.name == "FlyBossTest") {
			boss = (GameObject)Instantiate (boss, thisRoom.transform.position + new Vector3 (0, 4, 0), Quaternion.identity);
		} else {
			boss = (GameObject)Instantiate (boss, thisRoom.transform.position + new Vector3 (0, 2, 0), Quaternion.identity);
		}

		// Buffs boss health and damage based on ramp params.
		temp = (double)boss.GetComponent<EnemyHealth> ().currentHealth * ramp;
		boss.GetComponent<EnemyHealth> ().currentHealth = (int)temp;
		temp = (double)boss.GetComponent<EnemyHealth> ().startingHealth * ramp;
		boss.GetComponent<EnemyHealth> ().startingHealth = (int)temp;
		temp = (double)boss.GetComponent<EnemyAttack> ().damage * ramp;
		boss.GetComponent<EnemyAttack> ().damage = (int)temp;

		boss.GetComponent<AIPath>().target = GameObject.FindWithTag("Player").transform;
		
		
		// link rooms
		// ensures no infinite loops by incrementing i by nine when a door is already there.
		for (int i = 0; i < roomsToSpawn*3; i++) {
			chosenRoom = RandomRoom();
			
			Vector2 chosen = RandomNotEmpty(chosenRoom);
			if (!chosenRoom.adjRoomsDict.ContainsKey(chosen)) {
				i+=9;
			}
			SetAdj(chosenRoom, chosen, get(chosenRoom.Index + chosen));
		}
		
		minimapScript.PlayerEntersRoom(new Vector2(0, 0), get(new Vector2(0, 0)).adjRoomsDict.Keys.ToList());
		
		// add pathfinding graph and webs
		foreach (RoomControl room in roomsDict.Values) {
			room.PopulateCells();
			room.AddGraph();
			if (room.spawnWebs) {
				room.AddWebs(web, UnityEngine.Random.Range(minWebs, maxWebs));
			}
		}
		
		AstarPath.active.Scan();
		
		// populate rooms
		
		int totalTickets = enemyCommonness.Sum();
		
		for (int i = 0; i < totalEnemies; i++) {
			
			int c = 0;
			do {
				chosenRoom = RandomRoom();
				if (c > 1000) {
					throw new System.Exception("Infinite loop occuring.");
				}
				c++;
			} while (chosenRoom.enemies.Count >= maxEnemiesPerRoom || chosenRoom.spawnEnemies == false);
			
			int ticket = UnityEngine.Random.Range(0, totalTickets);
			
			int hopeful = 0;
			int sum = enemyCommonness[hopeful];
			
			while (sum <= ticket) {
				hopeful++;
				sum += enemyCommonness[hopeful];
			}
			// With ramp method used to ensure scaling is performed.
			chosenRoom.AddEnemyWithRamp(enemies[hopeful],ramp);
		}
		aStarGrids.Scan();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	
	// does there exist a room in the location given?
	public bool IsEmpty(Vector2 pos) {
		return !roomsDict.ContainsKey(pos);
	}
	
	// does there exist a room in the location given?
	private RoomControl get(Vector2 pos) {
		RoomControl output = null;
		roomsDict.TryGetValue(pos, out output);
		return output;
	}
	
	// does this room have an adjacent place for a room that is not occupied by a room already?
	private bool IsNextToEmpty(RoomControl room) {
		for (int index = 0; index < 4; index++) {
			if (IsEmpty(room.Index+RoomControl.vectors[index])) {
				return true;
			}
		}
		return false;
	}
	
	private RoomControl RandomRoom() {
		return roomsDict.ElementAt(UnityEngine.Random.Range(0, roomsDict.Count)).Value;
	}
	
	// returns: 0-3  signalling a direction which can give an empty room
	//          -1   if no such direction exists
	private Vector2 RandomEmpty(RoomControl room) {
		if (!IsNextToEmpty(room)) {
			throw new System.InvalidOperationException("Can not give empty room if not next to empty");
		}
		
		Vector2 vect;
		int d = 0;
		do {
			vect = RoomControl.vectors[UnityEngine.Random.Range(0, 4)];
			if (d > 1000) {
				throw new System.Exception("Infinite loop occuring.");
			}
			d++;
		} while (!IsEmpty(room.Index+vect));
		return vect;
	}
	
	// returns: 0-3  signalling a direction which can give an empty room
	private Vector2 RandomNotEmpty(RoomControl room) {
		Vector2 vect;
		int d = 0;
		do {
			vect = RoomControl.vectors[UnityEngine.Random.Range(0, 4)];
			if (d > 1000) {
				throw new System.Exception();
			}
			d++;
		} while (IsEmpty(room.Index+vect));
		return vect;
	}
	
	// Set up the bidirectional relationship between to rooms that will later ensure the doors are linked
	public void SetAdj(RoomControl room, Vector2 dir, RoomControl adjRoom) {
		
		room.SetAdj(dir, adjRoom);
		adjRoom.SetAdj(dir*-1, room);
		
		GameObject.Instantiate(trash, (room.transform.position + adjRoom.transform.position)/2 , Quaternion.identity);
	}
}
