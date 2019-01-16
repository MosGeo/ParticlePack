/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the MIT License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     MIT License for more details.
 *  
 *     You should have received a copy of the MIT License
 *     along with MIConvexHull.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at https://designengrlab.github.io/MIConvexHull/
 *************************************************************************/
namespace MIConvexHull
{
    using UnityEngine;
    public class Vertex : IVertex
    {
        public double[] Position { get; set; }
        public Vertex(double x, double y, double z)
        {
            Position = new double[3] { x, y, z };
        }
        public Vertex(Vector3 ver)
        {
            Position = new double[3] { ver.x, ver.y, ver.z };
        }
        public Vector3 ToVec()
        {
            return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        }
    }
}
