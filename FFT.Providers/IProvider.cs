using FFT.NT8;

namespace FFT.Providers;
public interface IProvider : IHaveUserCountToken, IHaveDependencies, IHaveReadyTask, IHaveErrorTask, IDisposable, IDisposeBase
{
  string Name { get; }
  ProviderStates State { get; }
  ProviderStatus GetStatus();
  void Start();
}
