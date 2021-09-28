namespace CreaNovelNETCore.BusinessLogic.RecursoDecorators
{
    public abstract class RecursoDecorator: Recurso
    {
        private readonly Recurso DecoratedRecurso;

        public RecursoDecorator(Recurso recurso)
        {
            DecoratedRecurso = recurso;
        }
        
        
        
    }
}