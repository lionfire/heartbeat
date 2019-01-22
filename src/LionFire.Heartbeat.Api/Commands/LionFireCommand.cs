namespace LionFire.Heartbeat
{
    public abstract class LionFireCommand : ILionFireCommand
    {
        public abstract bool CanExecute(object target, object context);
        public abstract void Execute(object target, object context);
    }
}
