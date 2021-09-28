using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Escena;
using CreaNovelNETCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaNovelNETCore.Repositories
{
    public class EscenaRepo
    {
        private readonly IMapper _mapper;
        private readonly CreanovelDbContext _context;
        
        public EscenaRepo(IMapper mapper, CreanovelDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<EscenaDTO> Create(CreateEscenaDTO escenaDto)
        {
            var escena = _mapper.Map<Escena>(escenaDto);
            await _context.Escenas.AddAsync(escena);
            await _context.SaveChangesAsync();
            var dbEscenaDto = _mapper.Map<EscenaDTO>(escena);
            return dbEscenaDto;
        }
        
        public async Task<EscenaDTO> Delete(Guid escenaId)
        {
            var dbEscena = await _context.Escenas.FindAsync(escenaId);
            _context.Escenas.Remove(dbEscena);
            await _context.SaveChangesAsync();
            return _mapper.Map<EscenaDTO>(dbEscena);
        }

        public async Task<EscenaRecursosDTO> GetByIdWithRecursos(Guid escenaId)
        {
            var escena = await _context.Escenas.Include(t => t.Recursos).FirstOrDefaultAsync(e => e.EscenaId == escenaId);
            var escenaDto = _mapper.Map<EscenaRecursosDTO>(escena);
            return escenaDto;
        }
    }
}