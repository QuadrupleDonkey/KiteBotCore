﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KiteBotCore.Modules
{
    /// <summary> Sets how often a user is allowed to use this command. </summary>
    /// <remarks>This is backed by an in-memory collection
    /// and will not persist with restarts.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly bool _noLimitInDMs;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command/group of commands. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        /// <param name="noLimitInDMs">If the ratelimit should be respected when a command is ran from a DM</param>
        public RatelimitAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;

            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
            }
        }

        ///// <summary> Sets how often a user is allowed to use this command. </summary>
        ///// <param name="times">The number of times a user may use the command within a certain period.</param>
        ///// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        ///// <param name="noLimitInDMs">If the ratelimit should be respected when the command is ran from a DM</param>
        //public RatelimitAttribute(uint times, TimeSpan period, bool noLimitInDMs = false)
        //{
        //    _invokeLimit = times;
        //    _noLimitInDMs = noLimitInDMs;
        //    _invokeLimitPeriod = period;
        //}

        /// <inheritdoc />
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IPrivateChannel && _noLimitInDMs)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            CommandTimeout timeout = _invokeTracker.TryGetValue(context.User.Id, out var t)
                           && now - t.FirstInvoke < _invokeLimitPeriod
                ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[context.User.Id] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError($"You are currently in Timeout for another {Math.Round((_invokeLimitPeriod - (now - timeout.FirstInvoke)).TotalSeconds, 1)}s."));
            }
        }

        private class CommandTimeout
        {
            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }

    /// <summary> Sets the scale of the period parameter. </summary>
    public enum Measure
    {
        /// <summary> Period is measured in days. </summary>
        Days,

        /// <summary> Period is measured in hours. </summary>
        Hours,

        /// <summary> Period is measured in minutes. </summary>
        Minutes
    }
}