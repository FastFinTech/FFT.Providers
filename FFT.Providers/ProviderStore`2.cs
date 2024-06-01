using FFT.IgnoreTasks;

namespace FFT.Providers;

/// <summary>
/// Stores providers of type <see cref="TProvider"/> and indexes them using
/// <see cref="TKey"/> as the key. Providers that have an error are disposed
/// and removed from the store. <see cref="TKey"/> MUST BE a
/// memberwise-equality object for the cache-keying to work as expected.
/// </summary>
public abstract class ProviderStore<TKey, TProvider> : DisposeBase
  where TProvider : class, IProvider
  where TKey : notnull
{
  private readonly object _sync = new();
  private readonly Dictionary<TKey, TProvider> _store = new();

  /// <summary>
  /// Gets the provider with the given info from the store if it exists, or creates a new one.
  /// The provider is started automatically, and will be removed from the store automatically if it errors.
  /// </summary>
  public TProvider GetCreate(TKey key)
  {
    lock (_sync)
    {
      EnsureNotDisposed();
      if (!_store.TryGetValue(key, out var provider))
      {
        provider = Create(key);
        _store[key] = provider;
        provider.ErrorTask.ContinueWith(
          t =>
          {
            lock (_sync)
            {
              _store.Remove(key);
            }
          },
          TaskScheduler.Default).Ignore();
        provider.Start();
      }

      return provider;
    }
  }

  /// <summary>
  /// Retrieves the first provider existing in the store that satisfies the given predicate.
  /// Returns null if none exists.
  /// </summary>
  public TProvider GetFirstOrNull(Func<TProvider, bool> predicate)
  {
    lock (_sync)
    {
      EnsureNotDisposed();
      foreach (var kvp in _store)
      {
        if (predicate(kvp.Value))
          return kvp.Value;
      }

      return null;
    }
  }

  protected abstract TProvider Create(TKey key);

  protected override void CustomDispose()
  {
    lock (_sync)
    {
      foreach (var kvp in _store)
        kvp.Value.Dispose();
      _store.Clear();
    }
  }

  private void EnsureNotDisposed()
  {
    if (DisposedToken.IsCancellationRequested)
    {
      throw new ObjectDisposedException(GetType().Name);
    }
  }
}

public static class ProviderStore
{
  public static ProviderStore<TKey, TProvider> Create<TKey, TProvider>(Func<TKey, TProvider> create)
    where TProvider : class, IProvider
    where TKey : notnull
    => new DefaultProviderStore<TKey, TProvider>(create);

  private sealed class DefaultProviderStore<TKey, TProvider> : ProviderStore<TKey, TProvider>
    where TProvider : class, IProvider
    where TKey : notnull
  {
    private readonly Func<TKey, TProvider> _create;

    public DefaultProviderStore(Func<TKey, TProvider> create)
      => _create = create;

    protected override TProvider Create(TKey key)
      => _create(key);
  }
}
