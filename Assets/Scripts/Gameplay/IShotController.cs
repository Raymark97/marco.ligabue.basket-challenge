namespace Gameplay {
    /// <summary>
    /// Represents any character capable of performing basketball shots.
    /// Provides an interface for recalculating shot trajectories
    /// based on the current position and game context.
    /// </summary>
    public interface IShotController {
        /// <summary>
        /// Recalculates the shot trajectories (direct and bank)
        /// according to the current player position and target hoop.
        /// Called whenever the shooter moves to a new spot.
        /// </summary>
        void RecalculateTrajectories();
        
    }
}