using UnityEngine;

namespace _Project.Scripts.Interaction
{
    public interface IInteractableEnhanced : IInteractable
    {
        /// <summary>
        /// Perform the interaction.
        /// </summary>
        void Interact(GameObject interactingPlayer);
        
    }
}