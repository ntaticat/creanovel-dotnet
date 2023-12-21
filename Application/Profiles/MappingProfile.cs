using System;
using System.Collections.Generic;
using System.Linq;
using Application.Dtos.Auth;
using Application.Dtos.Background;
using Application.Dtos.BackgroundSprite;
using Application.Dtos.Escena;
using Application.Dtos.Lectura;
using Application.Dtos.LecturaRecurso;
using Application.Dtos.Novela;
using Application.Dtos.NovelaBackground;
using Application.Dtos.NovelaPersonaje;
using Application.Dtos.NovelaVersion;
using Application.Dtos.Personaje;
using Application.Dtos.PersonajeSprite;
using Application.Dtos.Recurso;
using Application.Dtos.Usuario;
using AutoMapper;
using Domain.Models;

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

            // NovelaVersion
            CreateMap<NovelaVersion, NovelaVersionDto>();

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
            
            // Lectura
            CreateMap<LecturaDto, Lectura>();
            CreateMap<Lectura, LecturaDto>();
            
            // Usuario
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