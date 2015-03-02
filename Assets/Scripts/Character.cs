using System;
using UnityEngine;
using utils;

public class Character : MonoBehaviour
{
	private float _leftLinePosX;
	private float _rightLinePosX;
	private Vector3 _startPosition;

	private Rigidbody _phisics;
	private int _line = 1;

	private int _sideMoveDir = 0;
	private float _toX;
	private float _sideMoveSpeed;
	private float _jumpPower;
	private bool _canJump = true;

	private bool _swipeDetect;
	private Vector2 _startSwipePos;
	private Vector2 _swipeDir;
	private bool _waitTouchUp;

	private AudioSource _jumpSound;
	private AudioSource _coinSound;

	public Action OnGameOver;
	public Action OnCoinCollect;

	void Start()
	{
		_startPosition = transform.position;
		_phisics = GetComponent<Rigidbody>();
	}

	public void Setup(JSONNode cfg)
	{
		_sideMoveSpeed = cfg.GetFloat("side_move_speed", 1.2f);
		_jumpPower = cfg.GetFloat("jump_power", 500f);
		var lineOffset = cfg.GetFloat("line_offset", 0.85f);
		_leftLinePosX = -lineOffset;
		_rightLinePosX = lineOffset;
	}

	private void MoveLeft()
	{
		if (_line > 0)
		{
			_toX = _line == 2 ? 0f : _leftLinePosX;
			_sideMoveDir = Math.Sign(_toX - transform.position.x);
			--_line;
		}
	}

	private void MoveRight()
	{
		if (_line < 2)
		{
			_toX = _line == 0 ? 0f : _rightLinePosX;
			_sideMoveDir = Math.Sign(_toX - transform.position.x);
			++_line;
		}
	}

	private void Jump()
	{
		if (_canJump)
		{
			//for double jump ignoring:
			_canJump = false;
			_phisics.AddForce(0, _jumpPower, 0, ForceMode.Force);
			if (_jumpSound != null)
				_jumpSound.Play();
		}
	}

	private void UpdateKeyboardInput()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			MoveLeft();
		else if (Input.GetKeyDown(KeyCode.RightArrow))
			MoveRight();
		else if (Input.GetKeyDown(KeyCode.Space))
			Jump();
	}

	private void UpdateTouchInput()
	{
		if (Input.touchCount > 0)
		{
			if (!_swipeDetect)
			{
				_swipeDetect = true;
				_startSwipePos = Input.GetTouch(0).position;
			}
			else if (!_waitTouchUp)
			{
				_swipeDir = Input.GetTouch(0).position - _startSwipePos;
				if (_swipeDir.magnitude > Screen.width * 0.05f)
				{
					if (Mathf.Abs(_swipeDir.x) > Mathf.Abs(_swipeDir.y)) {
						if (_swipeDir.x > 0)
							MoveRight();
						else
							MoveLeft();
					} else if (_swipeDir.y > 0) {
						Jump();
					}
					_waitTouchUp = true;
				}
			}
		}
		else if (_swipeDetect)
		{
			_swipeDetect = false;
			_waitTouchUp = false;
		}
	}

	public void UpdateInput()
	{
#if UNITY_EDITOR
		UpdateKeyboardInput();
#endif
		UpdateTouchInput();
	}

	public void UpdatePos()
	{
		if (_sideMoveDir == 0)
			return;
		var pos = transform.position;
		pos.x += _sideMoveDir * _sideMoveSpeed * Time.deltaTime;
		if ((_sideMoveDir < 0 && pos.x < _toX) || (_sideMoveDir > 0 && pos.x > _toX))
		{
			pos.x = _toX;
			_sideMoveDir = 0;
		}
		transform.position = pos;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.StartsWith("road"))
		{
			_canJump = true;
			return;
		}

		if (collision.gameObject.name == "coin")
		{
			collision.gameObject.SetActive(false);
			
			var pos = transform.position;
			pos.z = _startPosition.z;
			transform.position = pos;
	
			//if jumping collision with coins -> do power jump
			if (!_canJump)
				_phisics.AddForce(0, _jumpPower * 0.5f, 0, ForceMode.Force);

			if (OnCoinCollect != null)
				OnCoinCollect();
			
			if (_coinSound != null)
				_coinSound.Play();
			return;
		}

		//if collision over boxes -> return
		var normal = collision.contacts[0].normal;
		if (Math.Abs(normal.y) > Math.Abs(normal.z))
		{
			_canJump = true;
			return;
		}

		if (OnGameOver != null)
			OnGameOver();
	}

	public void SetStartPosition()
	{
		transform.position = _startPosition;
		_line = 1;
		_sideMoveDir = 0;
	}

	public void SetJumpSound(AudioSource sound)
	{
		_jumpSound = sound;
	}

	public void SetCoinSound(AudioSource sound)
	{
		_coinSound = sound;
	}
}
