namespace Forseti
{
    public class Program
    {
        static void Main() => new BotManager().Start().GetAwaiter().GetResult();
    }
}
