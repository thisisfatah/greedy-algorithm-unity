using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public static bool IsGameReady = false;

	[SerializeField] GameObject panelStart;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (!IsGameReady)
		{
			if (Input.anyKeyDown)
			{
				IsGameReady = true;
				panelStart.SetActive(!IsGameReady);
			}
		}
	}

	public void GameOver()
	{
		IsGameReady = !IsGameReady;
		panelStart.SetActive(!IsGameReady);
	}
}
