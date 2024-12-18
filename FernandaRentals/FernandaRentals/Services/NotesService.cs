﻿using AutoMapper;
using FernandaRentals.Database;
using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Notes;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FernandaRentals.Services
{
    public class NotesService : INoteService
    {
        private readonly FernandaRentalsContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<UserEntity> _userManager;

        public NotesService(FernandaRentalsContext context, IMapper mapper, UserManager<UserEntity> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        //Eventos de listar
        public async Task<ResponseDto<List<NoteDto>>> GetNotesListAsync()
        {
            var notesEntity = await _context.Notes.ToListAsync();
            var notes = _mapper.Map<List<NoteDto>>(notesEntity);
            return new ResponseDto<List<NoteDto>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Lista de Categorias Obtenida Correctamente",
                Data = notes
            };
        }
        public async Task<ResponseDto<NoteDto>> GetNoteByIdAsync(Guid id)
        {
            var noteEntity = await _context.Notes.FirstOrDefaultAsync(note => note.Id == id);
            if (noteEntity == null)
            {
                return new ResponseDto<NoteDto>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se Encontro el Registro de la Nota"
                };
            }
            var noteDto = _mapper.Map<NoteDto>(noteEntity);

            return new ResponseDto<NoteDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registro encontrado",
                Data = noteDto

            };
        }
        public async Task<ResponseDto<List<NoteDto>>> GetNoteByEventIdListAsync(Guid id)
        {
            var noteEntities = await _context.Notes.Where(note => note.EventId == id)
                 .Include(e => e.CreatedByUser)
                .ToListAsync();

            if (!noteEntities.Any())
            {
                return new ResponseDto<List<NoteDto>>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se encontraron registros de notas para el evento especificado"
                };
            }
            
            var noteDtos = _mapper.Map<List<NoteDto>>(noteEntities);
  

            return new ResponseDto<List<NoteDto>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registros encontrados",
                Data = noteDtos
            };
        }


        // Ebentos de editar eliminar o crear

        public async Task<ResponseDto<NoteDto>> CreateNoteAsync(NoteCreateDto dto)
        {
            var noteEntity = _mapper.Map<NoteEntity>(dto);

            // para ver que no se repita el titulo
            var existingNote = await _context.Notes.FirstOrDefaultAsync(n => n.Name.ToLower().Trim() == noteEntity.Name.ToLower().Trim());
            if (existingNote != null)
            {
                return new ResponseDto<NoteDto>
                {
                    StatusCode = 400,
                    Status = false,
                    Message = "Una nota con el mismo titulo ya existe",
                    Data = null
                };
            }
            _context.Notes.Add(noteEntity);
            await _context.SaveChangesAsync();

            var noteDto = _mapper.Map<NoteDto>(noteEntity);
            var userEntity = await _userManager.FindByIdAsync(noteEntity.CreatedBy);
            noteDto.UserName = userEntity.Name;
            //exito al registrar
            return new ResponseDto<NoteDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registro Creado de una Nota",
                Data = noteDto
            };

        }

        public async Task<ResponseDto<NoteDto>> DeleteNoteAsync(Guid id)
        {
            var noteEntity = await _context.Notes.FirstOrDefaultAsync(note => note.Id == id);
            if (noteEntity == null)
            {
                return new ResponseDto<NoteDto>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se encontro el resgistro de la nota"
                };

            }

            _context.Notes.Remove(noteEntity);
            await _context.SaveChangesAsync();
            return new ResponseDto<NoteDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "La nota Eliminado con Exito"
            };
        }

        public async Task<ResponseDto<NoteDto>> EditNoteAsync(NoteEditDto dto, Guid id)
        {
            var noteEntity = await _context.Notes.FirstOrDefaultAsync(note => note.Id == id);
            if (noteEntity == null)
            {
                return new ResponseDto<NoteDto>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se Encontro el Registro"
                };
            }
            _mapper.Map<NoteEditDto, NoteEntity>(dto, noteEntity);
            _context.Notes.Update(noteEntity);
            await _context.SaveChangesAsync();

            var noteDto = _mapper.Map<NoteDto>(noteEntity);
            return new ResponseDto<NoteDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Exito al editar",
                Data = noteDto,

            };
        }

    }
}
