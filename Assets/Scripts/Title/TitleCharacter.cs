using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitleCharacter : MonoBehaviour {

    public static List<GameObject> players = new List<GameObject>();

	void Start()
    {
		for(int i = 0; i < players.Count; i++)
        {
			players[i].SetActive(false);
		}
		players [PlayerPrefs.GetInt ("SelectPlayer")].SetActive (true);
	}
}
