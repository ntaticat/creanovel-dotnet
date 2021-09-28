using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Lectura;
using CreaNovelNETCore.DTOs.LecturaRecursos;
using CreaNovelNETCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaNovelNETCore.Repositories
{
    public class LecturaRepo
    {
        private readonly CreanovelDbContext _context;
        private readonly IMapper _mapper;
        
        public LecturaRepo(CreanovelDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LecturaDto> AddLectura(CreateLecturaDto createLecturaDto)
        {
            var lectura = _mapper.Map<Lectura>(createLecturaDto);
            await _context.Lecturas.AddAsync(lectura);
            await _context.SaveChangesAsync();
            return _mapper.Map<LecturaDto>(lectura);
        }
        
        public async Task<LecturaDto> DeleteLectura(Guid lecturaId)
        {
            var dbLectura = await _context.Lecturas.FindAsync(lecturaId);
            _context.Lecturas.Remove(dbLectura);
            await _context.SaveChangesAsync();
            return _mapper.Map<LecturaDto>(dbLectura);
        }
        
        public async Task<LecturaDto> AddRecurso(LecturaRecursosDto lecturaRecursosDto)
        {
            var lecturaRecursos = _mapper.Map<LecturaRecursos>(lecturaRecursosDto);
            lecturaRecursos.Lectura = await _context.Lecturas.FindAsync(lecturaRecursos.LecturaId);
            lecturaRecursos.Recurso = await _context.Recursos.FindAsync(lecturaRecursos.RecursoId);
            await _context.LecturaRecurso.AddAsync(lecturaRecursos);
            await _context.SaveChangesAsync();
            
            var dbLectura = await _context.Lecturas.Include(l => l.Recursos).FirstOrDefaultAsync(l => l.LecturaId == lecturaRecursos.LecturaId);
            
            return _mapper.Map<LecturaDto>(dbLectura);
        }

        public async Task<List<LecturaDto>> GetLecturasFromUsuario(Guid usuarioId)
        {
            var dbUsuario = await _context.Usuarios.Include(u => u.Lecturas).FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
            return _mapper.Map<List<LecturaDto>>(dbUsuario.Lecturas);
        }
    }
}