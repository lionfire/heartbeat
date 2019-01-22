namespace LionFire.Heartbeat
{
    public interface ILionFireCommand
    {
        bool CanExecute(object target, object context);
        void Execute(object target, object context);
    }
}
