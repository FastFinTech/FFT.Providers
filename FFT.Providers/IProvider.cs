using System;
using FFT.NT8;

namespace FFT.Providers;
public interface IProvider : IHaveUserCountToken, IHaveDependencies, IHaveReadyTask, IHaveErrorTask, IDisposable
{
  string Name { get; }
  ProviderStates State { get; }
  Exception Exception { get; }
  ProviderStatus GetStatus();
  void Start();
}
