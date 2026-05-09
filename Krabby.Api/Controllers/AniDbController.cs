using Krabby.Core.Services.AniDB;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace Krabby.Api.Controllers
{
    [ApiController]
    [Route("api/anidb")]
    public class AniDbController : ControllerBase
    {
        private readonly AniDbService _service;
        private readonly AniDbJobStore _jobStore;

        public AniDbController(AniDbService service, AniDbJobStore jobStore)
        {
            _service = service;
            _jobStore = jobStore;
        }

        // GET /api/anidb/aid/18751
        [HttpGet("aid/{id}")]
        [RequestTimeout(600)] // 10 minutes
        public async Task<IActionResult> GetByAid(int id)
        {
            var result = await _service.GetAnimeDataAsync(id);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET /api/anidb/login/status
        [HttpGet("login/status")]
        public async Task<IActionResult> GetLoginStatus()
        {
            var result = await _service.GetLoginStatusAsync();

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET /api/anidb/login/execute
        [HttpGet("login/execute")]
        public async Task<IActionResult> ExecuteLogin()
        {
            var result = await _service.ExecuteLoginAsync();

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET /api/anidb/aid/episodes/get
        [HttpGet("aid/episodes/get")]
        public async Task<IActionResult> GetEpisodeData()
        {
            var jobId = _jobStore.CreateJob();

            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("[JOB] Started");

                    var result = await _service.GetEpisodeDataAsync();

                    Console.WriteLine("[JOB] Completed");

                    _jobStore.SetResult(jobId, result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[JOB ERROR] " + ex);
                    _jobStore.SetResult(jobId, new { error = ex.Message });
                }
            });

            return Ok(new
            {
                jobId
            });

        }

        // GET /api/anidb/aid/loaded
        [HttpGet("aid/episodes/status/{jobid}")]
        public async Task<IActionResult> GetEpisodesDataStatus(string jobId)
        {
            //var result = await _service.GetEpisodesDataStatusAsync();

            var result = _jobStore.GetResult(jobId);

            if (result == null)
                return Ok(new { status = "processing" });

            return Ok(new
            {
                status = "done",
                data = result
            });
        }
        
        // GET /api/anidb/aid/loaded
        [HttpGet("aid/loaded")]
        public async Task<IActionResult> GetAnimeLoaded()
        {
            var result = await _service.GetAnimeLoadedAsync();

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }

    
}