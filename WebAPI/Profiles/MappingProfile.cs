using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using WebAPI.DTOs.Auth;
using WebAPI.DTOs.Escena;
using WebAPI.DTOs.Lectura;
using WebAPI.DTOs.LecturaRecursos;
using WebAPI.DTOs.Novela;
using WebAPI.DTOs.Recursos;
using WebAPI.DTOs.Usuario;
using WebAPI.Models;

namespace WebAPI.Profiles
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

            // Auth
            CreateMap<UserCredentials, Usuario>();
            CreateMap<Usuario, UserCredentials>();
        }
    }
}