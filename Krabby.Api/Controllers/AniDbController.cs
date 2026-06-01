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

        /// <summary>
        /// Constructor for AniDbController
        /// </summary>
        /// <param name="service"></param>
        /// <param name="jobStore"></param>
        public AniDbController(AniDbService service, AniDbJobStore jobStore)
        {
            _service = service;
            _jobStore = jobStore;
        }

        /// <summary>
        /// API hook for getting anime by aid from AniDB
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET /api/anidb/aid/18751
        [HttpGet("aid/{id}")]
        public async Task<IActionResult> GetByAid(int id)
        {
            var result = await _service.GetAnimeDataAsync(id);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

                    _jobStore.SetResult(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[JOB ERROR] " + ex);
                    _jobStore.SetResult(new { error = ex.Message });
                }
            });

            return Ok(new
            {
                jobId
            });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        // GET /api/anidb/aid/loaded
        [HttpGet("aid/episodes/status/{jobid}")]
        public async Task<IActionResult> GetEpisodesDataStatus(string jobId)
        {
            //var result = await _service.GetEpisodesDataStatusAsync();

            var result = _jobStore.GetResult();

            if (result == null)
                return Ok(new { status = "processing" });

            return Ok(new
            {
                status = "done",
                data = result
            });
        }

        // /// <summary>
        // /// 
        // /// </summary>
        // /// <returns></returns>
        // // GET /api/anidb/aid/loaded
        // [HttpGet("aid/loaded")]
        // public async Task<IActionResult> GetAnimeLoaded()
        // {
        //     var result = await _service.GetAnimeLoadedAsync();

        //     return Ok(new
        //     {
        //         success = true,
        //         data = result
        //     });
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET /api/anidb/alive
        [HttpGet("alive")]
        public async Task<IActionResult> GetAlive()
        {
            // var result = await _service.GetAnimeLoadedAsync();

            return Ok(new
            {
                success = true,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET /api/anidb/alive
        [HttpGet("aid/clear")]
        public async Task<IActionResult> ClearAnime()
        {
            var result = await _service.ClearAnime();

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }   
}