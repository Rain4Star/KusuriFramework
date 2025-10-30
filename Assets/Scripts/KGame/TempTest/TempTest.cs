using KEventSys;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempTest : MonoBehaviour
{
	public Text text;

	private int eId;

	public void OnEnable()
	{
		eId = EventSys.Ins.AddListener("SC_MAIN_TEST_ALIVE", (object[] args) =>
		{
			text.text = ((long)args[0]).ToString();		// delay
		});
	}

	public void OnDisable()
	{
		EventSys.Ins.RemoveListener("SC_MAIN_TEST_ALIVE", eId);
		
	}

}
