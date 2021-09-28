using CreaNovelNETCore.Models;
using CreaNovelNETCore.Repositories;

namespace CreaNovelNETCore.BusinessLogic.RecursoCommands
{
    public class NextRecursoCommand : ICommand
    {
        private readonly RecursoRepo _recursoRepo;
        private readonly CreanovelDbContext _context;
        
        public NextRecursoCommand(RecursoRepo recursoRepo, CreanovelDbContext context)
        {
            _recursoRepo = recursoRepo;
            _context = context;
        }
        
        public void Execute()
        {
            // go to next resource in database
            throw new System.NotImplementedException();
        }

        public bool CanExecute()
        {
            // Validate if can go to next recurso
            return true;
        }

        public void Undo()
        {
            // go to previous resource in database
            throw new System.NotImplementedException();
        }
    }
}