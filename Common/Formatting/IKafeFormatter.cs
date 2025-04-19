using System;

namespace Kafe;

public interface IKafeFormatter : ICustomFormatter
{
    bool CanFormat(string? format, Type argType);
}
