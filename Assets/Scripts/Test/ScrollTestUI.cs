using KUISys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTestUI : MonoBehaviour
{
	public ScrollBase sv;
	public int cnt = 10;
	public GameObject item;

	public void Awake()
	{
		sv.Init(
			item, 
			(int idx, UIElem elem) => 
			{
				elem.gameObject.name = idx.ToString(); 
			},
			null);
	}

	public void OnGUI()
	{
		if (GUILayout.Button("\n\t\t¸üÐÂ\t\t\n"))
		{
			sv.SetDataNum(cnt, true).Flush();
			cnt <<= 1;
		}
	}
}
