namespace StranglerPatternExample
{
    internal interface IFeatureFlagService
    {
        public bool IsAccommodationApiEnabled { get; }
    }
}
