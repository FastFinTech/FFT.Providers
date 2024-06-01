namespace FFT.Providers;
/// <summary>
/// This is a convenience, utility class that provides boiler-plate code used
/// by many providers. You don't have to inherit this class to create a
/// provider, if it doesn't suit your provider's requirements. Instead, you
/// can just implement the <see cref="IProvider{T}"/> interface.
/// Implementing classes auto-dispose themselves when they reach error state.
/// If they are disposed by user code, they change to error state.
/// </summary>
public abstract class ProviderBase : DisposeBase, IProvider
{
  private readonly object _sync = new();
  private readonly TaskCompletionSource<object> _readyTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource<object> _errorTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly UserTokenMonitor _userTokenMonitor = new();

  private long _started = 0;

  public ProviderBase()
  {
    _userTokenMonitor.UserCountZero += () =>
    {
      if (ShouldDisposeWhenAllUsersAreFinished)
      {
        Dispose(new Exception("User count is zero."));
      }
    };
  }

  /// <inheritdoc />
  public string Name { get; protected set; }

  /// <inheritdoc />
  public ProviderStates State { get; private set; } = ProviderStates.Loading;

  /// <inheritdoc />
  public Task ReadyTask => _readyTCS.Task;

  /// <inheritdoc />
  public Task ErrorTask => _errorTCS.Task;

  /// <inheritdoc />
  public Exception Exception => DisposalReason;

  /// <inheritdoc />
  public bool ShouldDisposeWhenAllUsersAreFinished { get; protected set; } = true;

  /// <inheritdoc />
  public void Start()
  {
    if (Interlocked.Exchange(ref _started, 1) == 1)
      throw new InvalidOperationException($"Provider '{GetType().Name}' can only be started once.");
    CustomStart();
  }

  protected abstract void CustomStart();

  /// <inheritdoc />
  public IDisposable GetUserCountToken() => _userTokenMonitor.GetUserCountToken();

  /// <inheritdoc />
  public abstract IEnumerable<object> GetDependencies();

  /// <inheritdoc />
  public abstract ProviderStatus GetStatus();

  protected void OnReady()
  {
    lock (_sync)
    {
      if (State == ProviderStates.Loading)
      {
        State = ProviderStates.Ready;
        _readyTCS.TrySetResult(null!);
      }
    }
  }

  protected sealed override void CustomDispose()
  {
    OnDisposing();
    lock (_sync)
    {
      State = ProviderStates.Error;
      _errorTCS.TrySetException(DisposalReason!);
      _readyTCS.TrySetException(DisposalReason!);
    }
    OnDisposed();
  }

  protected void RunBackgroundDisposeError(Func<Task> action)
  {
    Task.Run(action).ContinueWith(
      t => Dispose(t.Exception?.InnerException ?? new OperationCanceledException()),
      TaskContinuationOptions.NotOnRanToCompletion);
  }

#pragma warning disable SA1502 // Element should not be on a single line
  protected virtual void OnDisposing() { }
  protected virtual void OnDisposed() { }
}
