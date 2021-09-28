using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Recursos;
using CreaNovelNETCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaNovelNETCore.Repositories
{
    public class RecursoRepo
    {
        private readonly IMapper _mapper;
        private readonly CreanovelDbContext _context;
        
        public RecursoRepo(IMapper mapper, CreanovelDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        
        public async Task<RecursoDTO> GetById(Guid recursoId)
        {
            var dbRecurso = await _context.Recursos.FindAsync(recursoId);

            var type = dbRecurso.GetType() == typeof(RecursoConversacion);
            if (type)
            {
                return _mapper.Map<RecursoConversacionDTO>(dbRecurso);
            }
            else
            {
                return _mapper.Map<RecursoDecisionDTO>(dbRecurso);
            }

        }
        
        public async Task<List<RecursoDTO>> GetAll()
        {
            var dbRecursos = await _context.Recursos.ToListAsync();
            var dbRecursosDto = _mapper.Map<List<RecursoDTO>>(dbRecursos);
            return dbRecursosDto;
        }
        
        public async Task<List<RecursoDTO>> GetAllConversaciones()
        {
            var dbRecursos = await _context.Recursos.OfType<RecursoConversacion>().ToListAsync();
            var dbRecursosDto = _mapper.Map<List<RecursoDTO>>(dbRecursos);
            return dbRecursosDto;
        }
        
        public async Task<List<RecursoDTO>> GetAllDecisiones()
        {
            var dbRecursos = await _context.Recursos.OfType<RecursoDecision>().ToListAsync();
            var dbRecursosDto = _mapper.Map<List<RecursoDTO>>(dbRecursos);
            return dbRecursosDto;
        }
        
        public async Task<RecursoDTO> Create(CreateRecursoDTO createRecursoDto)
        {
            Recurso recurso;
            RecursoDTO recursoDto;
            switch (createRecursoDto.TipoRecurso)
            {
                case "recurso_conversacion":
                    recurso = _mapper.Map<RecursoConversacion>(createRecursoDto);
                    break;
                case "recurso_decision":
                    recurso = _mapper.Map<RecursoDecision>(createRecursoDto);
                    break;
                default:
                    recurso = _mapper.Map<RecursoConversacion>(createRecursoDto);
                    break;
            }
            
            await _context.Recursos.AddAsync(recurso);
            await _context.SaveChangesAsync();
            
            switch (createRecursoDto.TipoRecurso)
            {
                case "recurso_conversacion":
                    recursoDto = _mapper.Map<RecursoConversacionDTO>(recurso);
                    break;
                case "recurso_decision":
                    recursoDto = _mapper.Map<RecursoDecisionDTO>(recurso);
                    break;
                default:
                    recursoDto = _mapper.Map<RecursoConversacionDTO>(recurso);
                    break;
            }
            
            return recursoDto;
        }
        
        public async Task<RecursoDTO> Delete(Guid recursoId)
        {
            var dbRecurso = await _context.Recursos.FindAsync(recursoId);
            _context.Recursos.Remove(dbRecurso);
            await _context.SaveChangesAsync();
            return _mapper.Map<RecursoDTO>(dbRecurso);
        }
        
        public async Task<RecursoDTO> SetSiguienteRecursoToConversacion(Guid recursoId, Guid recursoSiguienteId)
        {
            var dbRecurso = await _context.Recursos.OfType<RecursoConversacion>().FirstOrDefaultAsync(r => r.RecursoId == recursoId);
            var siguienteRecurso = await _context.Recursos.FindAsync(recursoSiguienteId);
            dbRecurso.SiguienteRecurso = siguienteRecurso;
            dbRecurso.SiguienteRecursoId = recursoSiguienteId;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<RecursoDTO>(dbRecurso);
        }
        
        public async Task<RecursoDecisionOpcionDTO> SetOpcionToDecision(CreateRecursoDecisionOpcionDTO opcionDto)
        {
            var opcion = _mapper.Map<RecursoDecisionOpcion>(opcionDto);
            _context.RecursoDecisionOpciones.Add(opcion);
            await _context.SaveChangesAsync();

            var dbRecursoDecision = await _context.RecursosDecision.FindAsync(opcion.RecursoDecisionId);
            dbRecursoDecision.Opciones.Add(opcion);
            await _context.SaveChangesAsync();
            
            return _mapper.Map<RecursoDecisionOpcionDTO>(opcion);
        }
    }
}