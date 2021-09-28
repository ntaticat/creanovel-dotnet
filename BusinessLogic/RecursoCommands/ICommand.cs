namespace CreaNovelNETCore.BusinessLogic.RecursoCommands
{
    public interface ICommand
    {
        void Execute();
        bool CanExecute();
        void Undo();
    }
}