using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AML.Web.Helper
{
    /// <summary>
    /// Cross-controller helpers for hardening case creation:
    /// - IsSafeName / IsSafePlainText : input validation on fields that travel to C6 + DB.
    /// - CaseCreationDedup            : in-process replay/double-submit suppression.
    /// </summary>
    public static class InputGuards
    {
        // Allow Unicode letters + digits + spaces + hyphens + apostrophes + dots + underscores. No SQL/script chars,
        // no angle brackets, no control characters.
        private static readonly Regex NamePattern = new(
            @"^[\p{L}\p{M}\p{N} _\.\-'’]{0,100}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Reject anything containing tags or SQL-style fragments.
        private static readonly Regex BlockedFragments = new(
            @"(<\/?\s*script|<\s*iframe|;\s*--|/\*|\*/|union\s+select|drop\s+table|insert\s+into|update\s+set|exec\s*\()",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsSafeName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return true; // optional fields are fine
            s = s.Trim(); // Auto-trim to avoid space-rejection at start/end
            if (s.Length > 100) return false;
            if (BlockedFragments.IsMatch(s)) return false;
            return NamePattern.IsMatch(s);
        }

        public static bool IsSafePlainText(string s, int maxLen = 500)
        {
            if (string.IsNullOrWhiteSpace(s)) return true;
            if (s.Length > maxLen) return false;
            if (BlockedFragments.IsMatch(s)) return false;
            return true;
        }
    }

    /// <summary>
    /// In-process short-window dedup: returns true if this key has already been seen
    /// in the last `windowSeconds` seconds (= caller should treat as duplicate).
    /// Not durable across restarts; that's deliberate — this only catches double-submits
    /// and form replays in the same web-process within seconds.
    /// </summary>
    public static class CaseCreationDedup
    {
        private static readonly ConcurrentDictionary<string, DateTime> _seen = new();
        private const int WindowSeconds = 60;

        public static bool TryRegister(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var now = DateTime.UtcNow;
            // GC old entries opportunistically (cheap; avoids unbounded memory).
            if (_seen.Count > 4096)
            {
                foreach (var kv in _seen)
                {
                    if ((now - kv.Value).TotalSeconds > WindowSeconds * 4)
                        _seen.TryRemove(kv.Key, out _);
                }
            }
            if (_seen.TryGetValue(key, out var ts) && (now - ts).TotalSeconds < WindowSeconds)
                return true; // duplicate
            _seen[key] = now;
            return false;
        }

        public static void Release(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            _seen.TryRemove(key, out _);
        }
    }
}
