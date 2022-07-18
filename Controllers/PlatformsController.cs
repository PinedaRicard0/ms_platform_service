using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data.Interfaces;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;
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
        private readonly ICommandDataClient _commandDataCliente;
        private readonly IMessageBusClient _messageBusCliente;

        public PlatformsController(IPlatformRepo platformRepo, IMapper mapper, ICommandDataClient commandDataCliente, IMessageBusClient messageBusClient)
        {
            _platformRepo = platformRepo;
            _mapper = mapper;
            _commandDataCliente = commandDataCliente;
            _messageBusCliente = messageBusClient;
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

            Platform toSavePlatform = _mapper.Map<Platform>(platformCreate);
            await _platformRepo.CreatePlatform(toSavePlatform);
            await _platformRepo.SaveChanges();
            PlatformReadDto platformResponse = _mapper.Map<PlatformReadDto>(toSavePlatform);

            try
            {
                //Send Sync Message
                await _commandDataCliente.SendPlatformToCommand(platformResponse);
            }
            catch (Exception syn)
            {
                Console.WriteLine($"Could not sent Synchronously message {syn.Message}");
                //return StatusCode(StatusCodes.Status500InternalServerError, syn.Message);
            }
            try
            {
                var platformPublished = _mapper.Map<PlatformPublishedDto>(platformResponse);
                platformPublished.Event = "Platform_Published";
                _messageBusCliente.PublishNewPlatform(platformPublished);
            }
            catch (Exception asyn)
            {
                Console.WriteLine($"Could not sent Asynchronously message {asyn.Message}");
                //return StatusCode(StatusCodes.Status500InternalServerError, asyn.Message);
            }
            return CreatedAtRoute(nameof(GetPlatformsById), new { Id = 15 }, platformResponse);

        }
    }
}
