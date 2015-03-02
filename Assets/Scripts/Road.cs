using System.Collections.Generic;
using System.Linq;
using utils;
using UnityEngine;

class Road
{
	private readonly List<GameObject> _roadBlocks = new List<GameObject>();
	private readonly GameObject _sample;
	private readonly Vector3 _offset;
	private float _firstBlockZ;

	private float _speed;
	private float _levelupSpeedInc;
	private Dictionary<GameObject, int> _obstacles = new Dictionary<GameObject, int>();
	private readonly List<Obstacle> _obstacleInfo = new List<Obstacle>();
	private Dictionary<GameObject, int> _obstaclesPool = new Dictionary<GameObject, int>();
	private int _coinIdx = -1;

	private int _coinsPriority;
	private int _levelsForIncObstacles;

	public Road(GameObject sample, GameObject offset, JSONNode cfg)
	{
		_sample = sample;
		_firstBlockZ = _sample.transform.position.z;
		_offset = offset.transform.position - _sample.transform.position;
		_roadBlocks.Add(_sample);
		_roadBlocks.Add(offset);

		var numBlocks = cfg.GetInt("blocks", 5);
		for (var i = 2; i < numBlocks; ++i)
		{
			AppendBlock();
		}

		_speed = cfg.GetFloat("start_speed", 5.0f);
		_levelupSpeedInc = cfg.GetFloat("levelup_speed_inc", 0.1f);
		_coinsPriority = cfg.GetInt("coins_priority", 50);
		_levelsForIncObstacles = cfg.GetInt("levels_for_inc_obstacles", 3);
	}

	private void AppendBlock()
	{
		var clone = Utils.Clone(_sample);
		clone.transform.position = _roadBlocks[_roadBlocks.Count - 1].transform.position + _offset;
		_roadBlocks.Add(clone);
	}

	public void Update(int level)
	{
		float path = (_speed + _levelupSpeedInc * level * _speed) * Time.deltaTime;
		foreach (var block in _roadBlocks)
		{
			var pos = block.transform.position;
			pos.z -= path;
			block.transform.position = pos;
		}
		if (_firstBlockZ - _roadBlocks[0].transform.position.z > _offset.z)
		{
			SwapBlocks(level);
		}
	}

	private void MoveObstaclesToPool(GameObject block)
	{
		var forRemove = new Dictionary<GameObject, int>();
		foreach (var obstacle in _obstacles) {
			if (obstacle.Key.transform.parent == block.transform) {
				forRemove.Add(obstacle.Key, obstacle.Value);
			}
		}
		foreach (var obstacle in forRemove) {
			_obstacles.Remove(obstacle.Key);
			_obstaclesPool.Add(obstacle.Key, obstacle.Value);
			obstacle.Key.SetActive(false);
		}
	}

	private void SwapBlocks(int level)
	{
		var block = _roadBlocks[0];
		_roadBlocks.RemoveAt(0);
		block.transform.position = _roadBlocks[_roadBlocks.Count - 1].transform.position + _offset;
		_roadBlocks.Add(block);
		
		MoveObstaclesToPool(block);
		
		GenerateObstacles(block, level);
	}

	public void AddObstacle(Obstacle sample)
	{
		_obstacleInfo.Add(sample);
		sample.UnityObject.SetActive(false);
		if (sample.Type == "coin")
			_coinIdx = _obstacleInfo.Count - 1;
	}

	private GameObject GetObstacleFromPool(int idx)
	{
		foreach (var obstacle in _obstaclesPool)
		{
			if (obstacle.Value == idx)
			{
				_obstaclesPool.Remove(obstacle.Key);
				obstacle.Key.SetActive(true);
				return obstacle.Key;
			}
		}
		return null;
	}

	private void GenerateObstacles(GameObject block, int level)
	{
		int count = Random.Range(1, 2 + level / _levelsForIncObstacles);
		float len = _offset.magnitude;
		float dist = _offset.magnitude / (count + 1);
		float z = -len * 0.5f + dist;
		for (var i = 0; i < count; ++i)
		{
			var addCoin = Random.Range(0, 100) < _coinsPriority;
			var obstacleIdx = addCoin ? _coinIdx : Random.Range(0, _obstacleInfo.Count);
			var line = Random.Range(0, 3);
			var obstacle = _obstacleInfo[obstacleIdx];

			obstacle.UnityObject.SetActive(true);
			var clone = GetObstacleFromPool(obstacleIdx);
			if (clone == null)
				clone = Utils.Clone(obstacle.UnityObject);
			clone.transform.parent = block.transform;
			clone.transform.localPosition = new Vector3(obstacle.Position[line] / block.transform.localScale.x, 0, z);
			_obstacles.Add(clone, obstacleIdx);
			clone.name = obstacle.Type;
			obstacle.UnityObject.SetActive(false);

			z += dist;
		}
	}

	public void RegenerateObstacles(int level)
	{
		for (var i = 0; i < _roadBlocks.Count; ++i)
		{
			var block = _roadBlocks[i];
			MoveObstaclesToPool(block);
			if (i > 1)
				GenerateObstacles(block, level);
		}
	}
}
