namespace BCallouts.Beans
{
    public class AircraftModel
    {
        public string Model { get; set; }
        public string Name { get; set; }
    }

    public class Configuration
    {
        public AircraftModel[] AircraftModels { get; set; }
    }
}
