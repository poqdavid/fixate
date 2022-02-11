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

public class BossData
{
    private string defaultName = string.Empty;
    private BossMechanics[] defaultMechanics = Array.Empty<BossMechanics>();
    private int defaultTimeLimit = 0;

    [JsonProperty("name")]
    public string Name
    {
        get => defaultName;
        set
        {
            defaultName = value;
        }
    }

    [JsonProperty("mechanics")]
    public BossMechanics[] Mechanics
    {
        get => defaultMechanics;
        set
        {
            defaultMechanics = value;
        }
    }

    [JsonProperty("timeLimit")]
    [DefaultValue(0)]
    public int TimeLimit
    {
        get => defaultTimeLimit;
        set
        {
            defaultTimeLimit = value;
        }
    }
}