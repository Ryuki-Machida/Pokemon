using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
	[SerializeField] Fade fade = null;

	public void OnBattle()
	{
		fade.FadeIn(1, () =>
		{
			fade.FadeOut(1);
		});
	}
}
