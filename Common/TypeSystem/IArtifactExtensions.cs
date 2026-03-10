using System;

namespace Kafe;

public static class IArtifactExtensions
{
    extension(IArtifact artifact)
    {
        public object? GetProperty(string key)
        {
            if (!artifact.Properties.TryGetValue(key, out var value))
            {
                return null;
            }

            return value;
        }

        public T? GetProperty<T>(string key)
        {
            var value = artifact.GetProperty(key);
            if (value is null)
            {
                return default;
            }

            if (value is not T castValue)
            {
                throw new InvalidOperationException(
                    $"Property '{key}' exists but has unexpected type '{value.GetType()}', not '{typeof(T)}'."
                );
            }

            return castValue;
        }

        public object RequireProperty(string key)
        {
            var value = artifact.GetProperty(key);
            if (value is null)
            {
                throw new InvalidOperationException($"Property '{key}' is not set.");
            }

            return value;
        }

        public T RequireProperty<T>(string key)
        {
            var value = artifact.GetProperty<T>(key);
            if (value is null)
            {
                throw new InvalidOperationException($"Property '{key}' is not set.");
            }

            return value;
        }
    }
}
