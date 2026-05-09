using System;
using System.Collections.Generic;
using System.Text;

namespace Krabby.Core.Services.AniDB
{
    public class AniDbJobStore
    {
        private readonly Dictionary<string, object?> _jobs = new();

        public string CreateJob()
        {
            var id = Guid.NewGuid().ToString();
            Console.WriteLine("[AniDbJobStore: CreateJob] Created Job " + id);
            _jobs[id] = null;
            
            return id;
        }

        public void SetResult(string id, object result)
        {
            Console.WriteLine("[AniDbJobStore: SetResult] Set Job result");
            _jobs[id] = result;
        }

        public object? GetResult(string id)
        {
            return _jobs.TryGetValue(id, out var result) ? result : null;
        }
    }
}
