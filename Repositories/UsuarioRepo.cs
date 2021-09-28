using System;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaNovelNETCore.Repositories
{
    public class UsuarioRepo
    {
        private readonly IMapper _mapper;
        private readonly CreanovelDbContext _context;

        public UsuarioRepo(IMapper mapper, CreanovelDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        
        // Tested
        public async Task<UsuarioDto> GetById(Guid usuarioId)
        {
            var dbUsuario = await _context.Usuarios
                .Include(u => u.Lecturas)
                .ThenInclude(l => l.Recursos)
                .Include(u => u.NovelasCreadas)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
            var dbUsuarioDto = _mapper.Map<UsuarioDto>(dbUsuario);
            return dbUsuarioDto;
        }
        
        // Tested
        public async Task<UsuarioDto> CreateUsuario(CreateUsuarioDto createUsuarioDto)
        {
            var usuario = _mapper.Map<Usuario>(createUsuarioDto);
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return _mapper.Map<UsuarioDto>(usuario);
        }
    }
}