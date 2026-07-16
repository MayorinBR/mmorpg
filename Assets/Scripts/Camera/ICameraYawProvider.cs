namespace Project.CameraSystem
{
    /// <summary>
    /// Exposes the current horizontal orientation (yaw) of a camera, so
    /// other systems such as movement can align input to the camera's
    /// facing direction without depending on the concrete camera type.
    /// </summary>
    public interface ICameraYawProvider
    {
        /// <summary>Gets the camera's current yaw, in degrees.</summary>
        float CurrentYaw { get; }
    }
}