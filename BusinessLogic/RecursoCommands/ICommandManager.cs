namespace CreaNovelNETCore.BusinessLogic.RecursoCommands
{
    public interface ICommandManager
    {
        void Invoke(ICommand command);
        void Undo();
    }
}