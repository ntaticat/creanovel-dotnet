using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Application.Entities.Auth.Dtos;
using Application.Entities.Escena.Dtos;
using Application.Entities.Lectura.Dtos;
using Application.Entities.LecturaRecurso.Dtos;
using Application.Entities.Novela.Dtos;
using Application.Entities.Recurso.Dtos;
using Application.Entities.Usuario.Dtos;
using Domain.Models;

namespace Application.Profiles
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Novela
            CreateMap<CreateNovelaDto, Novela>();
            CreateMap<Novela, NovelaNoEscenasDto>();
            CreateMap<Novela, NovelaWithEscenasDto>();
            
            // Escena
            CreateMap<Escena, EscenaDto>();
            CreateMap<CreateEscenaDto, Escena>();
            CreateMap<EscenaDto, Escena>();

            CreateMap<Escena, EscenaRecursosDto>();
            
            // Recurso
            CreateMap<Recurso, RecursoDto>()
                .IncludeAllDerived();
            
            CreateMap<RecursoConversacion, RecursoConversacionDto>();
            CreateMap<RecursoDecision, RecursoDecisionDto>();
            CreateMap<RecursoDecisionOpcion, RecursoDecisionOpcionDto>();
            
            CreateMap<CreateRecursoDecisionOpcionDto, RecursoDecisionOpcion>();
            CreateMap<CreateRecursoDto, RecursoDecision>();
            CreateMap<CreateRecursoDto, RecursoConversacion>();
            
            // Lecutra
            CreateMap<CreateLecturaDto, Lectura>();
            CreateMap<LecturaDto, Lectura>();
            CreateMap<Lectura, LecturaDto>();
            
            // Usuario
            CreateMap<CreateUsuarioDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<UsuarioDto, Usuario>();
            
            // Lectura Recursos
            CreateMap<LecturaRecursoDto, LecturaRecursos>();
            CreateMap<LecturaRecursos, LecturaRecursoDto>();

            // Auth
            CreateMap<UserCredentials, Usuario>();
            CreateMap<Usuario, UserCredentials>();
        }
    }
}