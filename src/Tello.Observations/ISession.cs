﻿using Repository;
using System;

namespace Tello.Observations
{
    public interface ISession : IEntity
    {
        DateTime Start { get; set; }
        TimeSpan Duration { get; set; }
    }
}