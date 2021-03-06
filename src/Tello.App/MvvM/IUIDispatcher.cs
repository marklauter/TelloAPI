﻿// <copyright file="IUIDispatcher.cs" company="Mark Lauter">
// Copyright (c) Mark Lauter. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Tello.App.MvvM
{
    public interface IUIDispatcher
    {
        void Invoke(Action action);

        void Invoke(Action<object> action, object state);
    }
}
