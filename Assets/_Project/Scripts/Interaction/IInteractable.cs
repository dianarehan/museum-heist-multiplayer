/// <summary>
/// Interface for any object that can be interacted with.
/// Implement this on doors, items, charging stations, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Check if the player can interact with this object.
    /// </summary>
    /// <param name="playerTag">The player's tag ("Guard" or "Thief")</param>
    /// <returns>True if interaction is allowed</returns>
    bool CanInteract(string playerTag);
    
    /// <summary>
    /// Perform the interaction.
    /// </summary>
    void Interact();
    
    /// <summary>
    /// Get the prompt message to display (e.g., "Open Door", "Pick Up", "Steal")
    /// </summary>
    string GetInteractionPrompt();
    
    /// <summary>
    /// Get the transform where the prompt should appear.
    /// </summary>
    UnityEngine.Transform GetPromptPosition();
}
