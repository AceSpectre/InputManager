using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public static class InputManager : object
{
	public static PlayerControls playercontrols = new();

	public static Vector2 GetVector2(string actionName, string mapName)
	{
		playercontrols.Enable();

		switch(mapName)
		{
			case "Player":
				switch(actionName)
				{
					case "Movement":
						return playercontrols.Player.Movement.ReadValue<Vector2>();
					case "MouseDelta":
						return playercontrols.Player.MouseDelta.ReadValue<Vector2>();
					case "MousePos":
						return playercontrols.Player.MousePos.ReadValue<Vector2>();
					default:
						break;
				}
				break;
			case "New action map":
				switch(actionName)
				{
					default:
						break;
				}
				break;
			default:
				break;
		}
		playercontrols.Disable();
		return new Vector2(0f, 0f);
	}

	public static Vector2 GetVector2(string actionName)
	{
		playercontrols.Enable();

		switch(actionName)
		{
					case "Movement":
						return playercontrols.Player.Movement.ReadValue<Vector2>();
					case "MouseDelta":
						return playercontrols.Player.MouseDelta.ReadValue<Vector2>();
					case "MousePos":
						return playercontrols.Player.MousePos.ReadValue<Vector2>();
			default:
				break;
		}
		playercontrols.Disable();
		return new Vector2(0f, 0f);
	}

	public static bool GetButton(string actionName, bool held, string mapName)
	{
		playercontrols.Enable();

		switch(mapName)
		{
			case "Player":
				switch(actionName)
				{
					case "Shift":
						return held ? playercontrols.Player.Shift.triggered : playercontrols.Player.Shift.ReadValue<bool>();
					case "LMB":
						return held ? playercontrols.Player.LMB.triggered : playercontrols.Player.LMB.ReadValue<bool>();
					case "RMB":
						return held ? playercontrols.Player.RMB.triggered : playercontrols.Player.RMB.ReadValue<bool>();
					case "Space":
						return held ? playercontrols.Player.Space.triggered : playercontrols.Player.Space.ReadValue<bool>();
					case "R":
						return held ? playercontrols.Player.R.triggered : playercontrols.Player.R.ReadValue<bool>();
					case "Q":
						return held ? playercontrols.Player.Q.triggered : playercontrols.Player.Q.ReadValue<bool>();
					default:
						break;
				}
				break;
			case "New action map":
				switch(actionName)
				{
					case "New action":
						return held ? playercontrols.Newactionmap.Newaction.triggered : playercontrols.Newactionmap.Newaction.ReadValue<bool>();
					default:
						break;
				}
				break;
			default:
				break;
		}
		playercontrols.Disable();
		return false;
	}

	public static bool GetButton(string actionName, bool held)
	{
		playercontrols.Enable();

		switch(actionName)
		{
					case "Shift":
						return held ? playercontrols.Player.Shift.triggered : playercontrols.Player.Shift.ReadValue<bool>();
					case "LMB":
						return held ? playercontrols.Player.LMB.triggered : playercontrols.Player.LMB.ReadValue<bool>();
					case "RMB":
						return held ? playercontrols.Player.RMB.triggered : playercontrols.Player.RMB.ReadValue<bool>();
					case "Space":
						return held ? playercontrols.Player.Space.triggered : playercontrols.Player.Space.ReadValue<bool>();
					case "R":
						return held ? playercontrols.Player.R.triggered : playercontrols.Player.R.ReadValue<bool>();
					case "Q":
						return held ? playercontrols.Player.Q.triggered : playercontrols.Player.Q.ReadValue<bool>();
					case "New action":
						return held ? playercontrols.Newactionmap.Newaction.triggered : playercontrols.Newactionmap.Newaction.ReadValue<bool>();
			default:
				break;
		}
		playercontrols.Disable();
		return false;
	}

}

