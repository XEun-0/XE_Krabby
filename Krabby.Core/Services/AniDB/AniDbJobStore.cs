using System;

namespace Krabby.Core.Services.AniDB
{   
    /// <summary>
    /// 
    /// </summary>
    public class AniDbJobStore
    {
        private string? _currentJobId;
        private object? _currentResult;

        /// <summary>
        /// Creates a new active job and clears
        /// any previous result.
        /// </summary>
        /// <returns>
        /// Newly created job ID.
        /// </returns>
        public string CreateJob()
        {
            _currentJobId = Guid.NewGuid().ToString();
            _currentResult = null;

            Console.WriteLine(
                "[AniDbJobStore: CreateJob] Created Job "
                + _currentJobId);

            return _currentJobId;
        }

        /// <summary>
        /// Stores the result for the current job.
        /// </summary>
        /// <param name="result">
        /// Result object to store.
        /// </param>
        public void SetResult(object result)
        {
            if (_currentJobId == null)
            {
                throw new InvalidOperationException(
                    "No active job exists.");
            }

            Console.WriteLine("[AniDbJobStore: SetResult] Set Job result");

            _currentResult = result;
        }

        /// <summary>
        /// Gets the current job result.
        /// </summary>
        /// <returns>
        /// Current stored result or null.
        /// </returns>
        public object? GetResult()
        {
            return _currentResult;
        }

        /// <summary>
        /// Gets the active job ID.
        /// </summary>
        public string? GetCurrentJobId()
        {
            return _currentJobId;
        }

        /// <summary>
        /// Clears the current job state.
        /// </summary>
        public void Clear()
        {
            _currentJobId = null;
            _currentResult = null;

            Console.WriteLine("[AniDbJobStore: Clear] Cleared Job");
        }

        /// <summary>
        /// Returns whether a job is active.
        /// </summary>
        public bool HasActiveJob()
        {
            return _currentJobId != null;
        }
    }
}