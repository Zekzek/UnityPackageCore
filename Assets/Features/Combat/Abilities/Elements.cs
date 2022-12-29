// Lightning >> Water
// Lightning << Earth
// Fire >> Ice
// Water >> Fire
// Life << Death
// Death << Life
// Earth + Water = Life?
using System;

[Flags]
public enum Elements
{
    None = 0,

    Fire = 1 << 0,      // Red
    Ice = 1 << 1,       // Green
    Lightning = 1 << 2, // Blue
    Earth = 1 << 3,     // Green & Blue
    Wind = 1 << 4,      // Blue & Red
    Water = 1 << 5,     // Red & Green

    Life = 1 << 6,      // Blue
    Death = 1 << 7,     // Blue

    Slash = 1 << 8,     // Red & Green
    Pierce = 1 << 9,     // Green
    Crush = 1 << 10,    // Red
}
