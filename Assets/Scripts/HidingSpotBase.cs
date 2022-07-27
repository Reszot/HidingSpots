using Cinemachine;
using UnityEngine;

public class HidingSpotBase : MonoBehaviour, IInteractableObject
{
	[field: SerializeField]
	public CinemachineVirtualCamera SpotCamera { get; private set; } = null;

	private void Awake()
	{
		SpotCamera.enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent(out PlayerController player))
			return;

		player.ShowButtonPrompt(EPlayerActionType.Hide, this);
	}
	private void OnTriggerExit(Collider other)
	{
		if (!other.TryGetComponent(out PlayerController player))
			return;

		player.HideButtonPrompt(this);
	}
	public void Use(PlayerController playerController = null)
	{
		if (playerController == null)
			return;
		
		playerController.Hide(this);
	}
}
