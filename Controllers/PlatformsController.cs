using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data.Interfaces;
using PlatformService.Dtos;
using PlatformService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _platformRepo;
        private readonly IMapper _mapper;

        public PlatformsController(IPlatformRepo platformRepo, IMapper mapper)
        {
            _platformRepo = platformRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlatformReadDto>>> GetAllPlatforms()
        {
            var platforms = await _platformRepo.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }
        
        [HttpGet("{id}", Name = "GetPlatformsById")]
        public async Task<ActionResult<PlatformReadDto>> GetPlatformsById([FromRoute]int id)
        {
            var platform = await _platformRepo.GetPlatformById(id);
            if (platform == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> AddPlatform(PlatformCreateDto platformCreate)
        {
            if (platformCreate == null)
            {
                return BadRequest();
            }
            try
            {
                Platform toSavePlatform = _mapper.Map<Platform>(platformCreate);
                await _platformRepo.CreatePlatform(toSavePlatform);
                await _platformRepo.SaveChanges();
                PlatformReadDto platformResponse = _mapper.Map<PlatformReadDto>(toSavePlatform);
                return CreatedAtRoute(nameof(GetPlatformsById), new { Id = platformResponse.Id }, platformResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }

        }
    }
}
