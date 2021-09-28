using System;
using System.Collections.Generic;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.BusinessLogic.RecursoCommands
{
    public class CommandManager : ICommandManager
    {
        private Stack<ICommand> _commands = new Stack<ICommand>();
        private readonly CreanovelDbContext _context;
        
        public CommandManager(CreanovelDbContext context)
        {
            _context = context;
        }
        
        public void GetCommands()
        {
            
        }
        
        public void Invoke(ICommand command)
        {
            if (command.CanExecute())
            {
                _commands.Push(command);
                command.Execute();
            }
        }

        public void Undo()
        {
            var command = _commands.Pop();
            command.Undo();
        }
        
        
    }
}