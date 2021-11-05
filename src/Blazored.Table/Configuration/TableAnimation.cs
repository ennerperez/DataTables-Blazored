namespace Blazored.Table
{
    public class TableAnimation
    {
        public TableAnimationType Type { get; set; }
        public double? Duration { get; set; }

        public TableAnimation(TableAnimationType type, double duration)
        {
            Type = type;
            Duration = duration;
        }

        public static TableAnimation FadeIn(double duration) => new TableAnimation(TableAnimationType.FadeIn, duration);
        public static TableAnimation FadeOut(double duration) => new TableAnimation(TableAnimationType.FadeOut, duration);
        public static TableAnimation FadeInOut(double duration) => new TableAnimation(TableAnimationType.FadeInOut, duration);
    }

    public enum TableAnimationType
    {
        None = 0, // Set to 0 so it is definitely the default value
        FadeIn,
        FadeOut,
        FadeInOut
    }
}
