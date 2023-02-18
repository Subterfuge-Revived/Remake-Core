namespace Subterfuge.Remake.Core.Generation
{
    public class IdGenerator
    {
        int _lastId = 0;

        public string GetNextId()
        {
            _lastId++;
            return _lastId.ToString();
        }
    }
}