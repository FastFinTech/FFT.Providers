
using System.Diagnostics;

namespace FFT.Providers;
public static class IProviderExtensions
{
  /// <summary>
  /// Throws an exception if the provider is in error state.
  /// </summary>
  [DebuggerStepThrough]
  public static void ThrowIfInError(this IProvider provider)
  {
    if (provider.State == ProviderStates.Error)
      throw new Exception("Error in " + provider.Name, provider.DisposalReason);
  }

  /// <summary>
  /// Throws an exception if any of the providers are in error state.
  /// </summary>
  [DebuggerStepThrough]
  public static void ThrowIfAnyHasError(this IEnumerable<IProvider> providers)
  {
    foreach (var provider in providers)
      provider.ThrowIfInError();
  }
}
