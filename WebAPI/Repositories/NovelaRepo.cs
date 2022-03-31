using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebAPI.DTOs;
using WebAPI.DTOs.Escena;
using WebAPI.DTOs.Novela;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Repositories
{
    public class NovelaRepo
    {
        private readonly CreanovelDbContext _context;
        private readonly IMapper _mapper; 

        public NovelaRepo(CreanovelDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<NovelaNoEscenasDTO> GetById(Guid novelaId)
        {
            var dbNovela = await _context.Novelas.FindAsync(novelaId);
            var dbNovelaDto = _mapper.Map<NovelaNoEscenasDTO>(dbNovela);
            return dbNovelaDto;
        }
        
        public async Task<NovelaWithEscenasDTO> GetByIdWithEscenas(Guid novelaId)
        {
            var dbNovela = await _context.Novelas.Include(n => n.Escenas)
                .FirstOrDefaultAsync(n => n.NovelaId == novelaId);
            var dbNovelaDto = _mapper.Map<NovelaWithEscenasDTO>(dbNovela);
            return dbNovelaDto;
        }

        public async Task<List<NovelaNoEscenasDTO>> GetAll()
        {
            var dbNovelas = await _context.Novelas.ToListAsync();
            var novelasDto = _mapper.Map<List<NovelaNoEscenasDTO>>(dbNovelas);
            return novelasDto;
        }

        public async Task<NovelaNoEscenasDTO> Create(CreateNovelaDTO novelaDto)
        {
            var novela = _mapper.Map<Novela>(novelaDto);
            await this._context.Novelas.AddAsync(novela);
            await _context.SaveChangesAsync();
            var dbNovelaDto = _mapper.Map<NovelaNoEscenasDTO>(novela);
            return dbNovelaDto;
        }

        public async Task<NovelaNoEscenasDTO> Delete(Guid novelaId)
        {
            var dbNovela = await _context.Novelas.FindAsync(novelaId);
            _context.Novelas.Remove(dbNovela);
            await _context.SaveChangesAsync();
            return _mapper.Map<NovelaNoEscenasDTO>(dbNovela);
        }
    }
}