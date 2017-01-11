
namespace Pypn.Tests.Mocks.EntityModel
{
    public class Square : IEntity
    {
        public Square(int width)
        {
            Width = width;
        }

        public int Id { get; set; }
        public int Width { get; set; }

        public override string ToString()
        {
            return $"Square {Width}x{Width}";
        }
    }
}
