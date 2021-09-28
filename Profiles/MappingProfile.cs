using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CreaNovelNETCore.DTOs;
using CreaNovelNETCore.DTOs.Escena;
using CreaNovelNETCore.DTOs.Lectura;
using CreaNovelNETCore.DTOs.LecturaRecursos;
using CreaNovelNETCore.DTOs.Recursos;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.Profiles
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Novela
            CreateMap<CreateNovelaDTO, Novela>();
            CreateMap<Novela, NovelaNoEscenasDTO>();
            CreateMap<Novela, NovelaWithEscenasDTO>();
            
            // Escena
            CreateMap<Escena, EscenaDTO>();
            CreateMap<CreateEscenaDTO, Escena>();
            CreateMap<EscenaDTO, Escena>();

            CreateMap<Escena, EscenaRecursosDTO>();
            
            // Recurso
            CreateMap<Recurso, RecursoDTO>()
                .IncludeAllDerived();
            
            CreateMap<RecursoConversacion, RecursoConversacionDTO>();
            CreateMap<RecursoDecision, RecursoDecisionDTO>();
            CreateMap<RecursoDecisionOpcion, RecursoDecisionOpcionDTO>();
            
            CreateMap<CreateRecursoDecisionOpcionDTO, RecursoDecisionOpcion>();
            CreateMap<CreateRecursoDTO, RecursoDecision>();
            CreateMap<CreateRecursoDTO, RecursoConversacion>();
            
            // Lecutra
            CreateMap<CreateLecturaDto, Lectura>();
            CreateMap<LecturaDto, Lectura>();
            CreateMap<Lectura, LecturaDto>();
            
            // Usuario
            CreateMap<CreateUsuarioDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<UsuarioDto, Usuario>();
            
            // Lectura Recursos
            CreateMap<LecturaRecursosDto, LecturaRecursos>();
            CreateMap<LecturaRecursos, LecturaRecursosDto>();
        }
    }
}