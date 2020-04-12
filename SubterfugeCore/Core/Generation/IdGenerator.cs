namespace SubterfugeCore.Core.Generation
{
    public class IdGenerator
    {
        static int _lastId = 0;

        public static int GetNextId()
        {
            _lastId++;
            return _lastId;
        }
    }
}