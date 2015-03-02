using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Utils
{
	public static GameObject Clone(GameObject go)
	{
		var clone = (Object.Instantiate(go) as GameObject);
		if (clone != null)
		{
			clone.transform.SetParent(go.transform.parent, false);
			clone.transform.localScale = go.transform.localScale;
			clone.transform.position = go.transform.position;
		}
		return clone;
	}

	public static T GetChild<T>(GameObject parent, string n) where T : Component
	{
		var children = parent.GetComponentsInChildren<T>();
		return children.FirstOrDefault(c => c.name == n);
	}

}

