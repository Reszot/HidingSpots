using System;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI promptText = null;
	[SerializeField]
	private Rigidbody body = null;
	[SerializeField]
	private CapsuleCollider bodyCollider = null;
	[SerializeField, Range(1.25f, 2f)]
	private float standingHeight = 1.5f;
	[SerializeField, Range(0.2f, 1.2f)]
	private float crouchingHeight = 0.8f;

	[Header("Movement")]
	[SerializeField]
	private float mouseSensitivity = 4f;
	[SerializeField]
	private float movementSpeed = 3f;
	[SerializeField]
	private float jumpForce = 10f;

	[Header("Camera")]
	[SerializeField]
	private CinemachineVirtualCamera fppCamera = null;
	[SerializeField, Range(0f, 100f)]
	private float maxUpMove = 10;
	[SerializeField, Range(-100f, 0f)]
	private float maxDownMove = -10;

	private InputAction movementAction;
	private float cameraXRotation;
	private bool crouching;
	private bool canMove = true;
	
	private IInteractableObject currentInteractableObject;
	private HidingSpotBase currentHidingSpot;
	private CinemachineVirtualCamera currentCamera;

	private void Awake()
	{
		promptText.gameObject.SetActive(false);
	}

	private void Start()
	{
		currentCamera = fppCamera;
		var gameplayInputMap = DataContainer.InputManager.PlayerInputs.Gameplay;
		movementAction = gameplayInputMap.Move;
		gameplayInputMap.Crouch.performed += ToggleCrouch;
		gameplayInputMap.Jump.performed += Jump;
		gameplayInputMap.Use.performed += InteractWithObject;
	}

	private void OnDestroy()
	{
		var gameplayInputMap = DataContainer.InputManager.PlayerInputs.Gameplay;
		gameplayInputMap.Crouch.performed -= ToggleCrouch;
		gameplayInputMap.Jump.performed -= Jump;
		gameplayInputMap.Use.performed -= InteractWithObject;
	}

	private void Update()
	{
		Move();
		MouseLook();
	}

	public void ShowButtonPrompt(EPlayerActionType actionType, IInteractableObject interactableObject)
	{
		if (currentInteractableObject == interactableObject)
			return;
		currentInteractableObject = interactableObject;
		promptText.text = DataContainer.InputManager.GetButtonPromptForAction(actionType);
		promptText.gameObject.SetActive(true);
	}

	public void HideButtonPrompt(IInteractableObject interactableObject)
	{
		if (currentInteractableObject != interactableObject)
			return;
		currentInteractableObject = null;
		promptText.gameObject.SetActive(false);
	}

	public void Hide(HidingSpotBase hidingSpot)
	{
		ShowButtonPrompt(EPlayerActionType.StopHiding, null);
		canMove = false;
		currentHidingSpot = hidingSpot;
		body.isKinematic = true;
		body.detectCollisions = false;
		bodyCollider.enabled = false;
		fppCamera.enabled = false;
		currentCamera = hidingSpot.SpotCamera;
		currentCamera.enabled = true;
	}

	private void StopHiding()
	{
		currentCamera = fppCamera;
		body.isKinematic = false;
		body.detectCollisions = true;
		bodyCollider.enabled = true;
		currentHidingSpot.SpotCamera.enabled = false;
		fppCamera.enabled = true;
		canMove = true;
		currentHidingSpot = null;
	}

	private void InteractWithObject(InputAction.CallbackContext ctx)
	{
		if (currentInteractableObject == null)
		{
			if (currentHidingSpot != null)
			{
				StopHiding();
			}
			return;
		}
		
		currentInteractableObject.Use(this);
	}

	private void Move()
	{
		if (movementAction == null || !canMove)
			return;
		
		var movementVector = movementAction.ReadValue<Vector2>();
		body.AddRelativeForce(new Vector3(movementVector.x, 0, movementVector.y) * movementSpeed, ForceMode.Force);
	}

	private void MouseLook()
	{
		var mouseMovement = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;
		cameraXRotation -= mouseMovement.y;
		cameraXRotation = Math.Clamp(cameraXRotation, maxDownMove, maxUpMove);
		if (currentCamera != fppCamera)
		{
			currentCamera.transform.localRotation = Quaternion.Euler(cameraXRotation, currentCamera.transform.localRotation.eulerAngles.y, 0);
			currentCamera.transform.Rotate(Vector3.up * mouseMovement.x);
			return;
		}
		currentCamera.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
		transform.Rotate(Vector3.up * mouseMovement.x);
	}

	private void Jump(InputAction.CallbackContext ctx)
	{
		var rayStart = transform.position - new Vector3(0, bodyCollider.height * 0.45f, 0);
		Physics.Raycast(new Ray(rayStart, Vector3.down), out RaycastHit hit, 0.1f);
		if (hit.collider == null)
			return;
		
		body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
	}

	private void ToggleCrouch(InputAction.CallbackContext ctx)
	{
		crouching = !crouching;
		bodyCollider.height = crouching ? crouchingHeight : standingHeight;
	}
}
