namespace FFT.Providers;

public interface IHaveDependencies
{
  IEnumerable<object> GetDependencies();
}
