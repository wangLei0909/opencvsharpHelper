using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Services
{
  public static class TaskService
    {
       public static async Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout).ContinueWith(_ => default(TResult));
            var completedTasks =
                (await Task.WhenAll(tasks.Select(task => Task.WhenAny(task, timeoutTask)))).
                Where(task => task != timeoutTask);
            return await Task.WhenAll(completedTasks);
        }
    }
}
