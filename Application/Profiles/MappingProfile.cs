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
using Application.Entities.NovelaPersonaje.Dtos;
using Application.Entities.Personaje.Dtos;
using Application.Entities.PersonajeSprite.Dtos;
using Application.Entities.Background.Dtos;
using Application.Entities.BackgroundSprite.Dtos;
using Application.Entities.NovelaBackground.Dtos;

namespace Application.Profiles
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Novela
            CreateMap<Novela, NovelaNoEscenasDto>();
            CreateMap<Novela, NovelaWithEscenasDto>()
            .ForMember(x => x.Personajes, y => y.MapFrom(z => z.Personajes.Select(p => p.Personaje).ToList()))
            .ForMember(x => x.Backgrounds, y => y.MapFrom(z => z.Backgrounds.Select(p => p.Background).ToList()));

            

            // Personaje
            CreateMap<Personaje, PersonajeDto>();

            // PersonajeSprite
            CreateMap<PersonajeSprite, PersonajeSpriteDto>();

            // Novela-Personaje
            CreateMap<NovelaPersonaje, NovelaPersonajeDto>();

            // Background
            CreateMap<Background, BackgroundDto>();

            // BackgroundSprite
            CreateMap<BackgroundSprite, BackgroundSpriteDto>();

            // Novela-Background
            CreateMap<NovelaBackground, NovelaBackgroundDto>();
            
            // Escena
            CreateMap<Escena, EscenaDto>();
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
            
            // Lectura
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