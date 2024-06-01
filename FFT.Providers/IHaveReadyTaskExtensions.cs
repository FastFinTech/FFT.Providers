namespace FFT.NT8;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

public static class IHaveReadyTaskExtensions
{
  /// <summary>
  /// Asynchronously waits for the given <paramref name="provider"/> to reach
  /// its ready state.
  /// If the cancellation token is cancelled first, its task will throw an OperationCanceledException.
  /// If the provider reaches error state first, its task will throw the exception that caused the provider error.
  /// If the provider reaches ready state first, no exception will be thrown.
  /// </summary>
  [DebuggerStepThrough]
  public static async Task WaitForReadyAsync(this IHaveReadyTask provider, CancellationToken ct)
  {
    if (ct.IsCancellationRequested)
      throw new OperationCanceledException(ct);

    if (provider.ReadyTask.IsCompleted)
      return;

    if (ct.CanBeCanceled)
    {
      var tcs = new TaskCompletionSource<object>();
      using var registration = ct.Register(tcs.SetCanceled);
      await await Task.WhenAny(tcs.Task, provider.ReadyTask);  // throws OperationCanceledException if the completed task is the cancellation token task.
    }
    else
    {
      await provider.ReadyTask;
    }
  }

  /// <summary>
  /// Asynchronously waits for all the given providers to reach their ready state.
  /// If the cancellation token is cancelled first, its task will throw an OperationCanceledException.
  /// If any provider reaches error state first, its task will throw the exception that caused the provider error.
  /// If all providers reach ready state first, no exception will be thrown.
  /// </summary>
  [DebuggerStepThrough]
  public static async Task WaitForReadyAsync(this IEnumerable<IHaveReadyTask> providers, CancellationToken ct)
  {
    if (ct.IsCancellationRequested)
      throw new OperationCanceledException(ct);

    var tasks = providers
      .Select(p => p.ReadyTask)
      .Where(t => !t.IsCompleted)
      .ToList();

    if (tasks.Count == 0)
      return;

    if (ct.CanBeCanceled)
    {
      var tcs = new TaskCompletionSource<object>();
      using var registration = ct.Register(tcs.SetCanceled);
      tasks.Add(tcs.Task);
      while (tasks.Count > 1) // until only the cancellation task is left
      {
        var completedTask = await Task.WhenAny(tasks);
        await completedTask; // throws OperationCanceledException if the completed task is the cancellation token task.
        tasks.Remove(completedTask);
      }
    }
    else
    {
      while (tasks.Count > 0)
      {
        var completedTask = await Task.WhenAny(tasks);
        await completedTask;
        tasks.Remove(completedTask);
      }
    }
  }
}
