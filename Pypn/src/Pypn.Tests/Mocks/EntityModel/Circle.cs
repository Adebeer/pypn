
namespace Pypn.Tests.Mocks.EntityModel
{
    public class Circle : IEntity
    {
        public Circle(int radius)
        {
            Radius = radius;
        }

        public int Id { get; set; }
        public int Radius { get; set; }

        public override string ToString()
        {
            return $"Circle {Radius}";
        }
    }
}
