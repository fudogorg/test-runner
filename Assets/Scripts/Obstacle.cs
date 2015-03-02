using utils;
using UnityEngine;


class Obstacle
{
	public string Type { get; private set; }
	public GameObject UnityObject { get; private set; }

	public float[] Position { get; private set; }
	public Obstacle(GameObject gameObject, JSONNode cfg)
	{
		UnityObject = gameObject;
		Type = cfg.GetKey("type");
		Position = new float[3];
		Position[0] = cfg.GetFloat("line0");
		Position[1] = cfg.GetFloat("line1");
		Position[2] = cfg.GetFloat("line2");
	}
}
