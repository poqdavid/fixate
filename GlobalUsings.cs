/*
 *      This file is part of Fixate distribution (https://github.com/vortex1409/fixate).
 *      Copyright (c) 2022 contributors
 *
 *      Fixate is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      Fixate is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with Fixate.  If not, see <https://www.gnu.org/licenses/>.
 */

global using DSharpPlus;
global using DSharpPlus.CommandsNext;
global using DSharpPlus.CommandsNext.Attributes;
global using DSharpPlus.CommandsNext.Exceptions;
global using DSharpPlus.Entities;
global using DSharpPlus.EventArgs;
global using DSharpPlus.SlashCommands;
global using DSharpPlus.VoiceNext;
global using Fixate.Commands;
global using Fixate.Datas;
global using Fixate.Utils;
global using Microsoft.CognitiveServices.Speech;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using NWaves.Operations;
global using NWaves.Signals;
global using Serilog;
global using Serilog.Core;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Runtime.InteropServices;

namespace Fixate;