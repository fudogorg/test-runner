using utils;
using UnityEngine;


public class GameContext : MonoBehaviour
{
	private Road _road;
	private Character _character;
	private bool _isGameStarted;
	private GameScreen _gui;

	private float _scores = 0;
	private int _coins = 0;
	private int _levelupScores;
	private int _levelupStep;
	private int _level = 0;
	private int _scoresPerSeconds;
	private int _scoresPerCoin;

	private AudioSource _menuMusic;
	private AudioSource _gameMusic;

	void Start()
	{
		var res = Resources.Load<TextAsset>("params");
		var data = JSONNode.Parse(res.text);

		_road = new Road(Utils.GetChild<Transform>(gameObject, "road0").gameObject, Utils.GetChild<Transform>(gameObject, "road1").gameObject, data["road"]);
		
		_character = GetComponentInChildren<Character>();
		_character.Setup(data["character"]);
		_character.OnGameOver += OnGameOver;
		_character.OnCoinCollect += OnCoinCollect;

		_gui = new GameScreen(Utils.GetChild<Canvas>(gameObject, "ui"));
		_gui.SwitchState(UiModes.MainMenu);

		_levelupStep = data.GetInt("levelup_step", 5000);
		
		var arObstacles = data.GetArray("obstacles");
		for (var i = 0; i < arObstacles.Count; ++i)
		{
			var objName = arObstacles[i].GetKey("name");
			var obsGameObject = Utils.GetChild<BoxCollider>(gameObject, objName);
			if (obsGameObject != null)
			{
				var obstacle = new Obstacle(obsGameObject.gameObject, arObstacles[i]);
				_road.AddObstacle(obstacle);
			}
			else
				Debug.LogError("Not found GameObject for obstacle with name: " + objName);
		}
		
		_scoresPerSeconds = data.GetInt("scores_per_second", 50);
		_scoresPerCoin = data.GetInt("scores_per_coin", 100);

		_menuMusic = Utils.GetChild<AudioSource>(gameObject, "menu");
		_gameMusic = Utils.GetChild<AudioSource>(gameObject, "game");
		_character.SetJumpSound(Utils.GetChild<AudioSource>(gameObject, "jump"));
		_character.SetCoinSound(Utils.GetChild<AudioSource>(gameObject, "coin"));
		
		_menuMusic.Play();
	}

	public void OnStartClick()
	{
		_gui.SwitchState(UiModes.Game);
		_levelupScores = _levelupStep;
		_level = 0;
		_scores = 0;
		_coins = 0;
		_character.SetStartPosition();
		_isGameStarted = true;
		_road.RegenerateObstacles(_level);
		_menuMusic.Stop();
		_gameMusic.Play();
	}

	public void OnExitClick()
	{
		_menuMusic.Stop();
		Application.Quit();
	}

	// Update is called once per frame
	void Update()
	{
		if (_isGameStarted)
		{
			_character.UpdateInput();
			_character.UpdatePos();
			_road.Update(_level);
			_scores += Time.deltaTime * _scoresPerSeconds;
			if (_scores > _levelupScores)
			{
				++_level;
				_levelupScores += _levelupStep;
			}

			_gui.UpdateValues(_coins, (int)_scores);
		}
	}

	private void OnGameOver()
	{
		_isGameStarted = false;
		_gui.SetResult((int)_scores);
		_gui.SwitchState(UiModes.GameOver);
		_gameMusic.Stop();
		_menuMusic.Play();
	}

	private void OnCoinCollect()
	{
		++_coins;
		_scores += _scoresPerCoin;
	}

}
