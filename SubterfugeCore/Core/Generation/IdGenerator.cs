namespace SubterfugeCore.Core.Generation
{
    public class IdGenerator
    {
        static int lastId = 0;

        public static int getNextId()
        {
            lastId++;
            return lastId;
        }
    }
}