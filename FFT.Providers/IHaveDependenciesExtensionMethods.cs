namespace FFT.Providers;

public static class IHaveDependenciesExtensionMethods
{

  /// <summary>
  /// Gets all of the dependencies of the given <paramref name="target"/>
  /// and all the dependencies' dependencies in a recursive manner. You
  /// can optionally supply the <paramref name="include"/> predicate to
  /// filter out the dependencies (and their dependencies) that you don't
  /// want included.
  /// </summary>
  public static HashSet<object> GetDependenciesRecursive(this IHaveDependencies target, Func<object, bool> include = null)
  {
    var items = new HashSet<object>();
    if (target is null) return items;
    include ??= static x => true;
    Recurse(target, include, items);
    return items;

    static void Recurse(IHaveDependencies target, Func<object, bool> include, HashSet<object> items)
    {
      foreach (var item in target.GetDependencies())
      {
        if (item is not null && include(item) && items.Add(item) && item is IHaveDependencies nextGeneration)
          Recurse(nextGeneration, include, items);
      }
    }
  }
}
