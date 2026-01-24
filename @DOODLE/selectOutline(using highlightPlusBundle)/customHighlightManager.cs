using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightPlus;

using SPACE_UTIL;

namespace SPACE_DOODLE
{
	public class customHighlightManager : MonoBehaviour
	{
		private void Start()
		{
			Debug.Log(C.method(this));
			HE = new List<HighlightEffect>();
			StopAllCoroutines();
			StartCoroutine(STIMULATE());
		}
		[SerializeField] Camera _cam;
		[SerializeField] float _dist = 10f;
		[SerializeField] LayerMask _layerMask = ~0;
		[SerializeField] List<HighlightEffect> HE;

		HighlightEffect prev_he;
		IEnumerator STIMULATE()
		{
			while (true)
			{
				if (prev_he != null)
					prev_he.highlighted = false;

				Ray ray = this._cam.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out RaycastHit hit, this._dist, this._layerMask))
				{
					HighlightEffect he = hit.transform.gc<HighlightEffect>();
					if(he != null)
					{
						he.highlighted = true;
						prev_he = he;
					}
				}
				yield return null;
			}
			yield return null;
		}
	}
}