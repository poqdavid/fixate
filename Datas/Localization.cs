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

namespace Fixate.Datas;

public struct Localization
{
    private string defaultTimerCountDown = string.Empty;
    private string defaultTimerStart = string.Empty;
    private string defaultMechanicsFormat = string.Empty;
    private string defaultDefaultPlayerNameFormat = string.Empty;

    public Localization()
    {
       defaultTimerCountDown = string.Empty;
       defaultTimerStart = string.Empty;
       defaultMechanicsFormat = string.Empty;
       defaultDefaultPlayerNameFormat = string.Empty;
    }

    [JsonProperty("timer-count-down")]
    public string TimerCountDown
    {
        get => defaultTimerCountDown;
        set
        {
            defaultTimerCountDown = value;
        }
    }

    [JsonProperty("timer-start")]
    public string TimerStart
    {
        get => defaultTimerStart;
        set
        {
            defaultTimerStart = value;
        }
    }

    [JsonProperty("mechanics-format")]
    public string MechanicsFormat
    {
        get => defaultMechanicsFormat;
        set
        {
            defaultMechanicsFormat = value;
        }
    }

    [JsonProperty("default-player-name-format")]
    public string DefaultPlayerNameFormat
    {
        get => defaultDefaultPlayerNameFormat;
        set
        {
            defaultDefaultPlayerNameFormat = value;
        }
    }
}