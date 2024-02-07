namespace Cat.SDKProvider
{
    public interface ISDKProvider : IEnabledSetable, ITransformGetable
    {
        /// <summary>
        /// Is the provider initialized for use.
        /// The functions in the provider should not be called
        /// if the provider is  not initialized correctly.
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// Is the provider available for the current platform.
        /// The script should not be activated if the provider is not available.
        /// </summary>
        bool IsAvailable { get; }
    }
}