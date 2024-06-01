using System.Collections.Immutable;
using System.Text;

namespace FFT.Providers;

/// <summary>
/// A class used for presenting IProvider status / debug information to the
/// user. This class is NOT supposed to be used by code for checking the state
/// of a provider, because the method to create it is inefficient, and the
/// IProvider has a "State" property that is more suitable.
/// </summary>
public sealed record ProviderStatus
{
  public string ProviderName { get; set; }
  public string StatusMessage { get; set; }
  public ImmutableList<ProviderStatus>? InternalProviders { get; set; }

  public override string ToString()
  {
    var sb = new StringBuilder();
    sb.AppendLine("Name: " + ProviderName);
    sb.AppendLine("Status: " + StatusMessage);
    if (InternalProviders is { Count: > 0 })
    {
      foreach (var provider in InternalProviders)
      {
        var status = provider.ToString();
        foreach (var line in status.Split('\n'))
        {
          sb.AppendLine("|  " + line.Trim());
        }
      }
    }

    return sb.ToString();
  }
}
