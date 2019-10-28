using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var mensajeDesdeRepo = await _repo.GetMessage(id);

            if(mensajeDesdeRepo == null)
                return NotFound();

            return Ok(mensajeDesdeRepo);         
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId,
            [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

             messageParams.UserId = userId;   

             var mensajesDesdeRepo = await _repo.GetMessagesForUser(messageParams);

             var mensajes = _mapper.Map<IEnumerable<MessageToReturnDto>>(mensajesDesdeRepo);             
             
             Response.AddPagination(mensajesDesdeRepo.CurrentPage, mensajesDesdeRepo.PageSize,
                 mensajesDesdeRepo.TotalCount, mensajesDesdeRepo.TotalPages);

             return Ok(mensajes);    
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var mensajeDesdeRepo = await _repo.GetMessageThread(userId, recipientId);

            var hiloMensaje = _mapper.Map<IEnumerable<MessageToReturnDto>>(mensajeDesdeRepo);

            return Ok(hiloMensaje);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, 
            MessageForCreationDto messageForCreationDto)
        {
            var usuarioEnvia = await _repo.GetUser(userId);

            if (usuarioEnvia.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("No se pudo encontrar usuario");

            var mensaje = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(mensaje);

            // Permite el retorno solo del mensaje y no el resto de los datos
            

            if (await _repo.SaveAll()) {
                var mensajeParaRetornar = _mapper.Map<MessageToReturnDto>(mensaje);
                return CreatedAtRoute("GetMessage", new {id = mensaje.Id}, mensajeParaRetornar);
            }
                

            throw new Exception("No se pudo grabar el mensaje??");    
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userid)
        {
            if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var mensajesDesdeRepo = await _repo.GetMessage(id);

            if (mensajesDesdeRepo.SenderId == userid)
                mensajesDesdeRepo.SenderDeleted = true;

            if (mensajesDesdeRepo.RecipientId == userid)
                mensajesDesdeRepo.RecipientDeleted = true;

            if (mensajesDesdeRepo.SenderDeleted && mensajesDesdeRepo.RecipientDeleted)
                _repo.Delete(mensajesDesdeRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error borrando el mensaje");    
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var mensaje = await _repo.GetMessage(id);

            if (mensaje.RecipientId != userId)
                return Unauthorized();

            mensaje.IsRead = true;
            mensaje.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }
        
    }
}