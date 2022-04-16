using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Commander.Models;
using Commander.Data;
using AutoMapper;
using Commander.Dtos;
using Microsoft.AspNetCore.JsonPatch;

namespace Commander.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    { 
        private readonly ICommanderRepo _repository ;
        private readonly IMapper _mapper;

        public CommandsController(ICommanderRepo res, IMapper mapper)
        {
            _repository = res;
            _mapper = mapper;
        }
        
        [HttpGet]
        //get all
        public ActionResult <IEnumerable<CommandReadDtos>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();
            return Ok(_mapper.Map<IEnumerable<CommandReadDtos>>(commandItems));
        }

        //this one is to get specific command with id
        [HttpGet("{id}", Name="GetCommandByID")]
        public ActionResult <CommandReadDtos> GetCommandByID(int id)
        {
            var commandItems = _repository.GetCommandByID(id);
            if (commandItems!=null)
            {
                return Ok(_mapper.Map<CommandReadDtos>(commandItems));
            }
            else{
                return NotFound();
            }
        }

        //create or POST
        [HttpPost]
        public ActionResult <CommandReadDtos> CreateCommand(CommandCreateDtos commandCreateDtos)
        {
            var commandModel = _mapper.Map<Command>(commandCreateDtos);
            _repository.CreateCommand(commandModel);
            _repository.SaveChanges();

            var commandReadDtos = _mapper.Map<CommandReadDtos>(commandModel); //this to make the cmd just changed to be read in dto
            return CreatedAtRoute(nameof(GetCommandByID),new{Id=commandReadDtos.Id}, commandReadDtos);
        }

        // PUT or update the whole thing
        [HttpPut("{Id}")]
        public ActionResult UpdateCommand (int id, CommandUpdateDtos commandUpdateDtos)
        {
            var checkCommandFromRepo = _repository.GetCommandByID(id);
            if (checkCommandFromRepo==null)
            {
                return NotFound();
            }
            _mapper.Map(commandUpdateDtos,checkCommandFromRepo);
            _repository.UpdateCommand(checkCommandFromRepo);
            _repository.SaveChanges();
            return NoContent();
        }

        //PATCH which is update command partially
        [HttpPatch("{Id}")]
        public ActionResult UpdateCommandPartially (int id, JsonPatchDocument<CommandUpdateDtos> patchDoc)
        {
            var checkCommandFromRepo = _repository.GetCommandByID(id);
            if (checkCommandFromRepo==null)
            {
                return NotFound();
            } 

            var commandtoPatch = _mapper.Map<CommandUpdateDtos>(checkCommandFromRepo);
            patchDoc.ApplyTo(commandtoPatch,ModelState); //turn json patchdoc file into dto command that can be updated

            if (!TryValidateModel(commandtoPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(commandtoPatch,checkCommandFromRepo);
            _repository.UpdateCommand(checkCommandFromRepo);
            _repository.SaveChanges();
            return NoContent();
        }

        //Delete a command
        [HttpDelete("{Id}")]
        public ActionResult DeleteCommand (int id)
        {
            var checkCommandFromRepo = _repository.GetCommandByID(id);
            if (checkCommandFromRepo==null)
            {
                return NotFound();
            } 

            _repository.DeleteCommand(checkCommandFromRepo);
            _repository.SaveChanges();
            return NoContent();
        }
    }
}