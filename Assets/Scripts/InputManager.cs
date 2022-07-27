using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager: MonoBehaviour
{
	public PlayerInputs PlayerInputs { get; private set; }

	private void Awake()
	{
		PlayerInputs = new PlayerInputs();
		PlayerInputs.Enable();
		DataContainer.InputManager = this;
		Cursor.visible = false;
	}

	public string GetButtonPromptForAction(EPlayerActionType actionType)
	{
		var inputs = GetInputForAction(actionType);
		switch (actionType)
		{
			case EPlayerActionType.Move:
			{
				return $"Use {inputs} to move around";
			}
			case EPlayerActionType.TakeItem:
			{
				return $"Press {inputs} to take item";
			}
			case EPlayerActionType.StopHiding:
			{
				return $"Press {inputs} to stop hiding";
			}
			default:
			{
				return $"Press {inputs} to {actionType.ToString()}";
			}
		}
	}

	private string GetInputForAction(EPlayerActionType actionType)
	{
		InputActionMap map = null;
		switch (actionType)
		{
			default:
			{
				map = PlayerInputs.Gameplay;
				break;
			}
		}
		switch (actionType)
		{
			case EPlayerActionType.Hide:
			case EPlayerActionType.TakeItem:
			case EPlayerActionType.StopHiding:
			{
				actionType = EPlayerActionType.Use;
				break;
			}
		}
		var actionName = actionType.ToString();
		var action = map.FindAction(actionName);
		if (action == null)
		{
			Debug.LogError($"No action with the name {actionName} found");
			return null;
		}

		if (action.controls.Count == 0)
		{
			Debug.LogError($"Action with the name {actionName} has no controls assigned");
			return null;
		}

		StringBuilder builder = new StringBuilder();
		foreach (var control in action.controls.ToArray())
		{
			builder.Append($"{control.displayName},");
		}
		builder.Remove(builder.Length - 1, 1);
		return builder.ToString();
	}
}
