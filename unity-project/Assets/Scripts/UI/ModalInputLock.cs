using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global "modal open" lock used to gate gameplay/UI inputs.
/// While locked, only the owning modal UI should accept interaction.
/// </summary>
public static class ModalInputLock
{
    private static readonly HashSet<object> Owners = new HashSet<object>();

    // Used to prevent "Esc closes modal and opens pause menu in the same frame"
    private static int escapeConsumedFrame = -1;

    public static bool IsLocked => Owners.Count > 0;

    public static bool WasEscapeConsumedThisFrame => escapeConsumedFrame == Time.frameCount;

    /// <summary>
    /// Acquire the lock for the given owner. Returns an IDisposable token that will Release() on Dispose().
    /// </summary>
    public static IDisposable Acquire(object owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        Owners.Add(owner);
        return new Token(owner);
    }

    public static void Release(object owner)
    {
        if (owner == null)
        {
            return;
        }

        Owners.Remove(owner);
    }

    /// <summary>
    /// Call this when a modal consumes Escape so other systems won't act on it in the same frame.
    /// </summary>
    public static void ConsumeEscapeThisFrame()
    {
        escapeConsumedFrame = Time.frameCount;
    }

    private sealed class Token : IDisposable
    {
        private object owner;

        public Token(object owner)
        {
            this.owner = owner;
        }

        public void Dispose()
        {
            if (owner == null)
            {
                return;
            }

            Release(owner);
            owner = null;
        }
    }
}


