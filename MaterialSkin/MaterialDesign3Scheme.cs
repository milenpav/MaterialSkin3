namespace MaterialSkin
{
    using System.Drawing;

    public sealed class MaterialColorRoles
    {
        public MaterialColorRoles(
            Color primary,
            Color onPrimary,
            Color primaryContainer,
            Color onPrimaryContainer,
            Color secondary,
            Color onSecondary,
            Color secondaryContainer,
            Color onSecondaryContainer,
            Color tertiary,
            Color onTertiary,
            Color tertiaryContainer,
            Color onTertiaryContainer,
            Color error,
            Color onError,
            Color errorContainer,
            Color onErrorContainer,
            Color surface,
            Color onSurface,
            Color surfaceContainer,
            Color surfaceContainerHigh,
            Color surfaceVariant,
            Color onSurfaceVariant,
            Color outline,
            Color outlineVariant,
            Color inverseSurface,
            Color inverseOnSurface,
            Color inversePrimary)
        {
            Primary = primary;
            OnPrimary = onPrimary;
            PrimaryContainer = primaryContainer;
            OnPrimaryContainer = onPrimaryContainer;
            Secondary = secondary;
            OnSecondary = onSecondary;
            SecondaryContainer = secondaryContainer;
            OnSecondaryContainer = onSecondaryContainer;
            Tertiary = tertiary;
            OnTertiary = onTertiary;
            TertiaryContainer = tertiaryContainer;
            OnTertiaryContainer = onTertiaryContainer;
            Error = error;
            OnError = onError;
            ErrorContainer = errorContainer;
            OnErrorContainer = onErrorContainer;
            Surface = surface;
            OnSurface = onSurface;
            SurfaceContainer = surfaceContainer;
            SurfaceContainerHigh = surfaceContainerHigh;
            SurfaceVariant = surfaceVariant;
            OnSurfaceVariant = onSurfaceVariant;
            Outline = outline;
            OutlineVariant = outlineVariant;
            InverseSurface = inverseSurface;
            InverseOnSurface = inverseOnSurface;
            InversePrimary = inversePrimary;
        }

        public Color Primary { get; }
        public Color OnPrimary { get; }
        public Color PrimaryContainer { get; }
        public Color OnPrimaryContainer { get; }
        public Color Secondary { get; }
        public Color OnSecondary { get; }
        public Color SecondaryContainer { get; }
        public Color OnSecondaryContainer { get; }
        public Color Tertiary { get; }
        public Color OnTertiary { get; }
        public Color TertiaryContainer { get; }
        public Color OnTertiaryContainer { get; }
        public Color Error { get; }
        public Color OnError { get; }
        public Color ErrorContainer { get; }
        public Color OnErrorContainer { get; }
        public Color Surface { get; }
        public Color OnSurface { get; }
        public Color SurfaceContainer { get; }
        public Color SurfaceContainerHigh { get; }
        public Color SurfaceVariant { get; }
        public Color OnSurfaceVariant { get; }
        public Color Outline { get; }
        public Color OutlineVariant { get; }
        public Color InverseSurface { get; }
        public Color InverseOnSurface { get; }
        public Color InversePrimary { get; }
    }

    internal static class MaterialDesign3Builder
    {
        private static readonly Color LightSurfaceBase = Color.FromArgb(255, 252, 251, 255);
        private static readonly Color DarkSurfaceBase = Color.FromArgb(255, 19, 19, 24);
        private static readonly Color ErrorBase = Color.FromArgb(255, 186, 26, 26);

        public static MaterialColorRoles BuildLight(Color primary, Color darkPrimary, Color lightPrimary, Color accent)
        {
            Color surface = Tint(LightSurfaceBase, primary, 0.035f);
            Color surfaceContainer = Tint(surface, primary, 0.06f);
            Color surfaceContainerHigh = Tint(surface, primary, 0.10f);
            Color surfaceVariant = Tint(lightPrimary, primary, 0.15f);
            Color onSurface = Color.FromArgb(255, 27, 28, 32);
            Color onSurfaceVariant = Blend(onSurface, surface, 0.42f);
            Color outline = Blend(onSurface, surface, 0.58f);
            Color outlineVariant = Blend(onSurface, surface, 0.78f);

            Color primaryContainer = Tint(lightPrimary, primary, 0.32f);
            Color secondary = accent;
            Color secondaryContainer = Tint(accent, Color.White, 0.72f);
            Color tertiary = darkPrimary;
            Color tertiaryContainer = Tint(darkPrimary, Color.White, 0.72f);
            Color errorContainer = Tint(ErrorBase, Color.White, 0.82f);

            return new MaterialColorRoles(
                primary,
                Contrast(primary),
                primaryContainer,
                Contrast(primaryContainer),
                secondary,
                Contrast(secondary),
                secondaryContainer,
                Contrast(secondaryContainer),
                tertiary,
                Contrast(tertiary),
                tertiaryContainer,
                Contrast(tertiaryContainer),
                ErrorBase,
                Contrast(ErrorBase),
                errorContainer,
                Contrast(errorContainer),
                surface,
                onSurface,
                surfaceContainer,
                surfaceContainerHigh,
                surfaceVariant,
                onSurfaceVariant,
                outline,
                outlineVariant,
                Color.FromArgb(255, 47, 48, 54),
                Color.FromArgb(255, 244, 239, 244),
                primaryContainer.Darken(0.25f));
        }

        public static MaterialColorRoles BuildDark(Color primary, Color darkPrimary, Color lightPrimary, Color accent)
        {
            Color surface = Tint(DarkSurfaceBase, primary, 0.08f);
            Color surfaceContainer = Tint(surface, primary, 0.12f);
            Color surfaceContainerHigh = Tint(surface, primary, 0.18f);
            Color surfaceVariant = Tint(primary, lightPrimary, 0.30f);
            Color onSurface = Color.FromArgb(255, 231, 225, 229);
            Color onSurfaceVariant = Blend(onSurface, surface, 0.34f);
            Color outline = Blend(onSurface, surface, 0.52f);
            Color outlineVariant = Blend(onSurface, surface, 0.72f);

            Color primaryRole = Tint(primary, lightPrimary, 0.26f);
            Color primaryContainer = Tint(darkPrimary, primary, 0.20f);
            Color secondary = Tint(accent, Color.White, 0.12f);
            Color secondaryContainer = Tint(accent, DarkSurfaceBase, 0.58f);
            Color tertiary = Tint(lightPrimary, Color.White, 0.10f);
            Color tertiaryContainer = Tint(darkPrimary, DarkSurfaceBase, 0.42f);
            Color error = Tint(ErrorBase, Color.White, 0.10f);
            Color errorContainer = Tint(ErrorBase, DarkSurfaceBase, 0.48f);

            return new MaterialColorRoles(
                primaryRole,
                Contrast(primaryRole),
                primaryContainer,
                Contrast(primaryContainer),
                secondary,
                Contrast(secondary),
                secondaryContainer,
                Contrast(secondaryContainer),
                tertiary,
                Contrast(tertiary),
                tertiaryContainer,
                Contrast(tertiaryContainer),
                error,
                Contrast(error),
                errorContainer,
                Contrast(errorContainer),
                surface,
                onSurface,
                surfaceContainer,
                surfaceContainerHigh,
                surfaceVariant,
                onSurfaceVariant,
                outline,
                outlineVariant,
                Color.FromArgb(255, 231, 225, 229),
                Color.FromArgb(255, 47, 48, 54),
                primaryRole.Lighten(0.18f));
        }

        private static Color Contrast(Color color)
        {
            return color.IsDark() ? Color.White : Color.FromArgb(255, 28, 27, 31);
        }

        private static Color Tint(Color baseColor, Color tintColor, float amount)
        {
            return Blend(baseColor, tintColor, amount);
        }

        private static Color Blend(Color background, Color foreground, float amount)
        {
            amount = amount < 0f ? 0f : amount > 1f ? 1f : amount;
            float inverse = 1f - amount;
            return Color.FromArgb(
                255,
                (int)(background.R * inverse + foreground.R * amount),
                (int)(background.G * inverse + foreground.G * amount),
                (int)(background.B * inverse + foreground.B * amount));
        }
    }
}
