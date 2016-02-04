/*
 * Beta.Speckle GH Exporter Component
 * Copyright (C) 2016 Dimitrie A. Stefanescu (@idid) / The Bartlett School of Architecture, UCL
 *
 * This program is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace BetaSpeckle
{
    internal class SuperProperty
    {
        public string name = "";
        public HashSet<string> values = new HashSet<string>();

        public SuperProperty(string _name)
        {

            name = _name;

        }

        public void addValue(string _val)
        {

            values.Add(_val);

        }
    }
}