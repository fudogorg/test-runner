using UnityEngine;
using UnityEngine.UI;

public enum UiModes
{
	MainMenu,
	GameOver,
	Game
}

class GameScreen
{
	private readonly Text _coinsText;
	private readonly Text _scoresText;
	private Text _resultText;

	private GameObject _gameOver;
	private GameObject _gameUi;
	private GameObject _mainMenu;

	public GameScreen(Canvas canvas)
	{
		_coinsText = Utils.GetChild<Text>(canvas.gameObject, "text_coins");
		_scoresText = Utils.GetChild<Text>(canvas.gameObject, "text_scores");
		_resultText = Utils.GetChild<Text>(canvas.gameObject, "text_result");

		_gameOver = Utils.GetChild<RectTransform>(canvas.gameObject, "game_over").gameObject;
		_gameUi = Utils.GetChild<RectTransform>(canvas.gameObject, "game").gameObject;
		_mainMenu = Utils.GetChild<RectTransform>(canvas.gameObject, "main_menu").gameObject;
	}

	public void UpdateValues(int coins, int scores)
	{
		_coinsText.text = "Coins: " + coins;
		_scoresText.text = scores.ToString();
	}

	public void SwitchState(UiModes mode)
	{
		_gameOver.SetActive(mode == UiModes.GameOver);
		_gameUi.SetActive(mode == UiModes.Game);
		_mainMenu.SetActive(mode == UiModes.MainMenu);
	}

	public void SetResult(int scores)
	{
		_resultText.text = "Result: " + scores;
	}
}
